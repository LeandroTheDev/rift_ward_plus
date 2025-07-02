using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace RiftWardPlus.Shared;

[HarmonyPatchCategory("riftwarrdrodpinger_riftward")]
class OverwriteRiftWard
{
    public Harmony overwriter;

    public void OverwriteNativeFunctions(ICoreAPI api)
    {
        if (!Harmony.HasAnyPatches("riftwarrdrodpinger_riftward"))
        {
            overwriter = new Harmony("riftwarrdrodpinger_riftward");
            overwriter.PatchCategory("riftwarrdrodpinger_riftward");
            Debug.Log("Rift ward has been overwrited");
        }
        else
        {
            if (api.Side == EnumAppSide.Client) Debug.Log("Block break overwriter has already patched, probably by the singleplayer server");
            else Debug.LogError("ERROR: Block break overwriter has already patched, did some mod already has riftwarrdrodpinger_riftward in harmony?");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(BlockEntityRiftWard), "OnInteract")]
    public static void OnInteract(BlockSelection blockSel, IPlayer byPlayer)
    {
        if (byPlayer?.Entity?.ActiveHandItemSlot?.Itemstack?.Collectible?.Code?.ToString() == "riftwardplus:riftwardpinger")
        {
            byPlayer.Entity.ActiveHandItemSlot.Itemstack.Attributes.SetDouble("riftWardPosition_X", blockSel.Position.X);
            byPlayer.Entity.ActiveHandItemSlot.Itemstack.Attributes.SetDouble("riftWardPosition_Y", blockSel.Position.Y);
            byPlayer.Entity.ActiveHandItemSlot.Itemstack.Attributes.SetDouble("riftWardPosition_Z", blockSel.Position.Z);
            Debug.LogDebug($"{byPlayer.PlayerName} interacted with rift ward");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlockEntityRiftWard), "Activate")]
    public static void Activate(BlockEntityRiftWard __instance)
    {
        if (__instance.Api?.Side != EnumAppSide.Server) return;

        // Getting protected variable
        bool hasFuel = Traverse.Create(__instance).Property("HasFuel").GetValue<bool>();

        if (hasFuel && __instance.Api != null)
        {
            string uniquePosition = $"{__instance.Pos.X},{__instance.Pos.Y},{__instance.Pos.Z}";
            if (RiftWardData.activeRiftsWards.Contains(uniquePosition)) return;
            else RiftWardData.activeRiftsWards.Add(uniquePosition);

            Debug.LogDebug($"New rift ward activated in: {uniquePosition}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlockEntityRiftWard), "Deactivate")]
    public static void Deactivate(BlockEntityRiftWard __instance)
    {
        if (__instance.Api?.Side != EnumAppSide.Server) return;

        string uniquePosition = $"{__instance.Pos.X},{__instance.Pos.Y},{__instance.Pos.Z}";
        RiftWardData.activeRiftsWards.Remove(uniquePosition);

        Debug.LogDebug($"Rift ward deactivated in: {uniquePosition}");
    }
}