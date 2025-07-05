using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace RiftWardPlus.Shared;

public class BlockEntityRiftPingerDetector : BlockEntity
{
    public static uint MaxDetectorBlocksToScan { get; internal set; } = 0;

    private long? refreshRiftWardsTickListener = null;

    // List of positions and distances of rift pinger
    private List<KeyValuePair<string, double>> riftWardDistances = [];
    private uint blocksScanned = 0;

    private bool alreadyScanning = false;

    public override void Initialize(ICoreAPI api)
    {
        base.Initialize(api);
        if (api.Side == EnumAppSide.Client) return;

        Debug.LogDebug("Block Initialized");
        refreshRiftWardsTickListener = RegisterGameTickListener(RefreshRiftWards, Configuration.detectorTickrate, 0);
    }

    public override void OnBlockUnloaded()
    {
        base.OnBlockUnloaded();

        if (refreshRiftWardsTickListener is not null)
            UnregisterGameTickListener((long)refreshRiftWardsTickListener);
    }

    public static uint EstimateTotalBlocksToScan(int radiusX, int radiusY, int radiusZ)
    {
        double a = radiusX;
        double b = radiusY;
        double c = radiusZ;

        double outer = 4.0 / 3.0 * Math.PI * a * b * c;
        double inner = 4.0 / 3.0 * Math.PI * Math.Max(0, a - 1) * Math.Max(0, b - 1) * Math.Max(0, c - 1);

        return (uint)Math.Ceiling(outer - inner);
    }
    
    private void RefreshRiftWards(float obj)
    {
        if (alreadyScanning) return;

        Debug.LogDebug("Scanning rift pingers started");

        int radiusX = Configuration.detectorRadius;
        int radiusY = Configuration.detectorYRadius;
        int radiusZ = Configuration.detectorRadius;

        blocksScanned = 0;

        alreadyScanning = true;
        // Running on secondary thread to not overload server with big radius
        // or high amount of rift pinger detectors
        Task.Run(async () =>
        {
            AssetLocation targetCode = new("riftwardplus:riftwardpinger");
            int targetBlockId = Api.World.GetBlock(targetCode).Id;

            BlockPos origin = new(Pos.X, Pos.Y, Pos.Z);
            List<KeyValuePair<string, double>> distances = [];

            int maxRadius = Math.Max(Math.Max(radiusX, radiusY), radiusZ);

            HashSet<string> visited = [];
            blocksScanned = 0;

            for (int r = 1; r <= maxRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = -r; dy <= r; dy++)
                    {
                        for (int dz = -r; dz <= r; dz++)
                        {
                            // Ignore points outside the ellipse (different radius on each axis)
                            if (Math.Abs(dx) > radiusX || Math.Abs(dy) > radiusY || Math.Abs(dz) > radiusZ)
                                continue;

                            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);

                            // Only visit points in the current "shell"
                            if (dist < r - 1 || dist > r)
                                continue;

                            // Do not check the same block
                            string key = $"{dx},{dy},{dz}";
                            if (!visited.Add(key))
                                continue;

                            BlockPos checkPos = origin.AddCopy(dx, dy, dz);
                            Block block = Api.World.BlockAccessor.GetBlock(checkPos);

                            if (block.Id == targetBlockId)
                            {
                                Debug.LogDebug($"Rift Pinger Finded in {key}");
                                distances.Add(new($"{checkPos.X},{checkPos.Y},{checkPos.Z}", checkPos.DistanceTo(origin)));
                            }
                            else if (Configuration.ENABLERADIUSBLOCKCHECK && block.Code.ToString() != "riftwardplus:riftpingerdetector")
                            {
                                Api.World.BlockAccessor.SetBlock(0, checkPos);
                            }

                            if (distances.Count >= Configuration.detectorMaxScannedRiftWards)
                            {
                                Debug.LogDebug("Scanning pinger dector finded max rift ward pingers...");
                                riftWardDistances = distances;
                                alreadyScanning = false;
                                MarkDirty();
                                return;
                            }

                            blocksScanned++;
                            await Task.Delay(Configuration.detectorMillisecondsSleepBetweenBlockCheck);
                            MarkDirty();
                        }
                    }
                }
            }

            riftWardDistances = distances;
            alreadyScanning = false;
            MarkDirty();

            Debug.LogDebug("Scanning rift pinger finish!!");
        });
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);

        if (riftWardDistances.Count == 0)
        {
            dsc.AppendLine(Lang.Get("riftwardplus:scanning-riftpinger"));
            if (Configuration.enableExtendedLogs)
            {
                dsc.AppendLine($"{blocksScanned}/{MaxDetectorBlocksToScan} blocks scanned");
            }
            return;
        }
        dsc.AppendLine(Lang.Get("riftwardplus:available-riftpingers"));

        foreach (KeyValuePair<string, double> riftWardPosDistance in riftWardDistances)
        {
            string[] riftWardCoords = riftWardPosDistance.Key.Split(",");

            if (Configuration.compassRose)
                dsc.AppendLine($"{(uint)riftWardPosDistance.Value}{Lang.Get(RiftWardData.GetCompassRoseOrientationBaseOnTwoPositions(Pos, new((int)double.Parse(riftWardCoords[0]), (int)double.Parse(riftWardCoords[1]), (int)double.Parse(riftWardCoords[2]))))}");
            else
                dsc.AppendLine($"{(uint)riftWardPosDistance.Value}");
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        tree.SetString("riftWardDistances", JsonSerializer.Serialize(riftWardDistances));
        tree.SetInt("riftWardBlocksScanned", (int)blocksScanned);

        base.ToTreeAttributes(tree);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        string riftWardDistancesString = tree.GetString("riftWardDistances", null);
        int blocksScannedInt = tree.GetInt("riftWardBlocksScanned", 0);
        if (string.IsNullOrEmpty(riftWardDistancesString)) return;

        riftWardDistances = JsonSerializer.Deserialize<List<KeyValuePair<string, double>>>(riftWardDistancesString);
        blocksScanned = (uint)blocksScannedInt;

        base.FromTreeAttributes(tree, worldAccessForResolve);
    }
}