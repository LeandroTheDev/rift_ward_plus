using System.Collections.Generic;
using RiftWardPlus.Shared;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace RiftWardPlus;

public class Initialization : ModSystem
{
    private readonly OverwriteRiftWard overwriteRiftWard = new();

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        if (Configuration.pingerOnlyActiveRiftWards)
        {
            // Save active rifts when the server save the world
            api.Event.GameWorldSave += ()
                => api.WorldManager.SaveGame.StoreData(
                    "RiftWardData_ActiveRiftWards",
                    RiftWardData.activeRiftsWards);

            // Load active rift
            byte[] data = api.WorldManager.SaveGame.GetData("RiftWardData_ActiveRiftWards");
            if (data is null) return;
            RiftWardData.activeRiftsWards = SerializerUtil.Deserialize<List<RiftWardInfo>>(data);
        }
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Debug.LoadLogger(api.Logger);
        api.RegisterBlockEntityClass("BlockEntityRiftWardDetector", typeof(BlockEntityRiftWardDetector));
        api.RegisterBlockEntityClass("BlockEntityRiftWardSimpleDetector", typeof(BlockEntityRiftWardSimpleDetector));
        api.RegisterBlockEntityClass("BlockEntityRiftPingerDetector", typeof(BlockEntityRiftPingerDetector));
        api.RegisterBlockEntityClass("BlockEntityRiftWardPinger", typeof(BlockEntityRiftWardPinger));
        api.RegisterBlockClass("BlockRiftWardPinger", typeof(BlockRiftWardPinger));
        api.RegisterBlockClass("BlockRiftWardSimpleDetector", typeof(BlockRiftWardSimpleDetector));

        overwriteRiftWard.OverwriteNativeFunctions(api);

        BlockEntityRiftWardDetector.MaxDetectorBlocksToScan = BlockEntityRiftWardDetector.EstimateTotalBlocksToScan(
            Configuration.detectorRadius, Configuration.detectorYRadius, Configuration.detectorRadius);
        BlockEntityRiftPingerDetector.MaxDetectorBlocksToScan = BlockEntityRiftPingerDetector.EstimateTotalBlocksToScan(
            Configuration.detectorRadius, Configuration.detectorYRadius, Configuration.detectorRadius);

        if (Configuration.ENABLERADIUSBLOCKCHECK)
        {
            Debug.LogWarn("----------------------------------------------------------");
            Debug.LogWarn("ENABLERADIUSBLOCKCHECK IS ENABLED THIS WILL DELETE THE SCANNED BLOCKS SO YOU CAN VIEW WHAT THE DETECTOR IS SCANNING");
            Debug.LogWarn("----------------------------------------------------------");
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        Configuration.UpdateBaseConfigurations(api);
    }
}

public class Debug
{
    static private ILogger logger;

    static public void LoadLogger(ILogger _logger) => logger = _logger;
    static public void Log(string message)
    {
        logger?.Log(EnumLogType.Notification, $"[RiftWardPlus] {message}");
    }
    static public void LogDebug(string message)
    {
        if (Configuration.enableExtendedLogs)
            logger?.Log(EnumLogType.Debug, $"[RiftWardPlus] {message}");
    }
    static public void LogWarn(string message)
    {
        logger?.Log(EnumLogType.Warning, $"[RiftWardPlus] {message}");
    }
    static public void LogError(string message)
    {
        logger?.Log(EnumLogType.Error, $"[RiftWardPlus] {message}");
    }
}