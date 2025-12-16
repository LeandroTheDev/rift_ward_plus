using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;

namespace RiftWardPlus.Shared;

public class BlockEntityRiftWardSimpleDetector : BlockEntity
{
    // List of positions and distances of rift wards
    private List<KeyValuePair<string, double>> riftWardDistances = null;

    public void RefreshRiftWards(IPlayer player)
    {
        List<RiftWardInfo> riftWardDatas = [.. RiftWardData.activeRiftsWards.Where(data => data.placedBy == player.PlayerUID)];

        List<KeyValuePair<string, double>> newRiftWardDistances = [];
        BlockPos origin = new(Pos.X, Pos.Y, Pos.Z);
        foreach (RiftWardInfo wardInfo in riftWardDatas)
        {
            BlockPos originWard = new(wardInfo.x, wardInfo.y, wardInfo.z);
            newRiftWardDistances.Add(new(wardInfo.uniqueid, originWard.DistanceTo(origin)));
        }
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);

        if (riftWardDistances is null)
        {
            dsc.AppendLine(Lang.Get("riftwardplus:disabled-riftwarddetector"));
            return;
        }

        dsc.AppendLine(Lang.Get("riftwardplus:available-riftwards"));

        foreach (KeyValuePair<string, double> riftWardPosDistance in riftWardDistances)
        {
            string[] riftWardCoords = riftWardPosDistance.Key.Split(",");

            if (Configuration.detectorOnlyActiveRiftWards)
            {
                string uniquePosition = $"{riftWardCoords[0]},{riftWardCoords[1]},{riftWardCoords[2]}";
                if (RiftWardData.activeRiftsWards.FirstOrDefault(data => data.uniqueid == uniquePosition, null) == null)
                {
                    Debug.LogDebug("Looking at deactivated rift, ignoring...");
                    dsc.AppendLine(Lang.Get("riftwardplus:disabled-riftward"));
                    return;
                }
            }

            if (Configuration.compassRose)
                dsc.AppendLine($"{(uint)riftWardPosDistance.Value}{Lang.Get(RiftWardData.GetCompassRoseOrientationBaseOnTwoPositions(Pos, new((int)double.Parse(riftWardCoords[0]), (int)double.Parse(riftWardCoords[1]), (int)double.Parse(riftWardCoords[2]))))}");
            else
                dsc.AppendLine($"{(uint)riftWardPosDistance.Value}");
        }
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        tree.SetString("riftWardDistances", JsonSerializer.Serialize(riftWardDistances));

        base.ToTreeAttributes(tree);
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        string riftWardDistancesString = tree.GetString("riftWardDistances", null);
        if (string.IsNullOrEmpty(riftWardDistancesString)) return;

        riftWardDistances = JsonSerializer.Deserialize<List<KeyValuePair<string, double>>>(riftWardDistancesString);

        base.FromTreeAttributes(tree, worldAccessForResolve);
    }

}

public class BlockRiftWardSimpleDetector : Block
{
    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
    {
        base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);

        if (byEntity is not EntityPlayer playerEntity) return;
        IPlayer player = playerEntity.Player;
        BlockEntity blockEntity = byEntity.World.BlockAccessor.GetBlockEntity(blockSel.Position);
        if (blockEntity is not BlockEntityRiftWardSimpleDetector blockEntityRiftWard) return;

        blockEntityRiftWard.RefreshRiftWards(player);

        Debug.LogDebug($"[Rift Ward Simple Detector] refreshed distances to: {player.PlayerName}");
    }
}