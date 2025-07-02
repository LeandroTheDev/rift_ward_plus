using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

namespace RiftWardPlus.Shared;

public class BlockEntityRiftWardPinger : BlockEntity
{
    public double? riftWardDistance = null;
    public double? riftWardX = null;
    public double? riftWardY = null;
    public double? riftWardZ = null;

    public override void OnBlockPlaced(ItemStack byItemStack = null)
    {
        base.OnBlockPlaced(byItemStack);
        if (byItemStack is null) return;

        riftWardX = byItemStack.Attributes.GetDouble("riftWardPosition_X", -1);
        riftWardY = byItemStack.Attributes.GetDouble("riftWardPosition_Y", -1);
        riftWardZ = byItemStack.Attributes.GetDouble("riftWardPosition_Z", -1);
        if (riftWardX == -1 || riftWardY == -1 || riftWardZ == -1) return;

        double dx = Pos.X - (double)riftWardX;
        double dy = Pos.Y - (double)riftWardY;
        double dz = Pos.Z - (double)riftWardZ;

        riftWardDistance = Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public override void ToTreeAttributes(ITreeAttribute tree)
    {
        base.ToTreeAttributes(tree);
        if (riftWardDistance != null &&
            riftWardX != null &&
            riftWardY != null &&
            riftWardZ != null)
        {
            tree.SetDouble("riftWardDistance", (double)riftWardDistance);
            tree.SetDouble("riftWardPosition_X", (double)riftWardX);
            tree.SetDouble("riftWardPosition_Y", (double)riftWardY);
            tree.SetDouble("riftWardPosition_Z", (double)riftWardZ);
        }
    }

    public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
    {
        base.FromTreeAttributes(tree, worldAccessForResolve);
        double distance = tree.GetDouble("riftWardDistance", -1);
        if (distance != -1)
        {
            riftWardDistance = distance;
            riftWardX = tree.GetDouble("riftWardPosition_X", 0);
            riftWardY = tree.GetDouble("riftWardPosition_Y", 0);
            riftWardZ = tree.GetDouble("riftWardPosition_Z", 0);
        }
    }

    public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
    {
        base.GetBlockInfo(forPlayer, dsc);
        if (riftWardDistance is null)
        {
            dsc.AppendLine(Lang.Get("riftwardplus:empty-riftward"));
            return;
        }

        // Detect if the rift is active
        if (Configuration.pingerOnlyActiveRiftWards)
        {
            if (!RiftWardData.activeRiftsWards.Contains($"{riftWardX},{riftWardY},{riftWardZ}"))
            {
                dsc.AppendLine(Lang.Get("riftwardplus:disabled-riftward"));
                return;
            }
        }

        if (Configuration.compassRose)
            dsc.AppendLine($"{Lang.Get("riftwardplus:available-riftward", (uint)riftWardDistance)}{Lang.Get(RiftWardData.GetCompassRoseOrientationBaseOnTwoPositions(new(Pos.X, Pos.Y, Pos.Z), new((int)riftWardX, (int)riftWardY, (int)riftWardZ)))}");
        else
            dsc.AppendLine(Lang.Get("riftwardplus:available-riftward", (uint)riftWardDistance));
    }
}

public class BlockRiftWardPinger : Block
{
    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
    {
        BlockEntity blockEntity = world.BlockAccessor.GetBlockEntity(pos);
        ItemStack[] droppedItems;
        if (blockEntity is not BlockEntityRiftWardPinger pinger) return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
        else droppedItems = base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);

        foreach (ItemStack item in droppedItems)
        {
            if (pinger.riftWardDistance is null) continue;

            item.Attributes.SetDouble("riftWardPosition_X", (double)pinger.riftWardX);
            item.Attributes.SetDouble("riftWardPosition_Y", (double)pinger.riftWardY);
            item.Attributes.SetDouble("riftWardPosition_Z", (double)pinger.riftWardZ);
        }

        return droppedItems;
    }

    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

        if (api is not ClientCoreAPI clientApi) return;

        EntityPos pos = clientApi.World.Player.Entity.Pos;

        double x = inSlot.Itemstack.Attributes.GetDouble("riftWardPosition_X", -1);
        double y = inSlot.Itemstack.Attributes.GetDouble("riftWardPosition_Y", -1);
        double z = inSlot.Itemstack.Attributes.GetDouble("riftWardPosition_Z", -1);
        if (x == -1 || y == -1 || z == -1) return;

        // Detect if the rift is active
        if (Configuration.pingerOnlyActiveRiftWards)
        {
            if (!RiftWardData.activeRiftsWards.Contains($"{x},{y},{z}"))
            {
                dsc.AppendLine(Lang.Get("riftwardplus:disabled-riftward"));
                return;
            }
        }

        double dx = pos.X - x;
        double dy = pos.Y - y;
        double dz = pos.Z - z;

        if (Configuration.compassRose)
            dsc.AppendLine($"{Lang.Get("riftwardplus:available-riftward", (uint)Math.Sqrt(dx * dx + dy * dy + dz * dz))}{Lang.Get(RiftWardData.GetCompassRoseOrientationBaseOnTwoPositions(new((int)pos.X, (int)pos.Y, (int)pos.Z), new((int)x, (int)y, (int)z)))}");
        else
            dsc.AppendLine(Lang.Get("riftwardplus:available-riftward", (uint)Math.Sqrt(dx * dx + dy * dy + dz * dz)));
    }
}