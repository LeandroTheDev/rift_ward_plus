using System;
using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace RiftWardPlus.Shared;

static internal class RiftWardData
{
    static internal List<string> activeRiftsWards = [];

    static internal string GetCompassRoseOrientationBaseOnTwoPositions(BlockPos pos1, BlockPos pos2)
    {
        // Difference between positions
        double dx = pos2.X - pos1.X;
        double dz = pos2.Z - pos1.Z;

        // Same position
        if (dx == 0 && dz == 0)
            return "riftwardplus:center";

        // Angle to north (-Z), set to 0° = North
        double angleRad = Math.Atan2(dx, -dz);
        double angleDeg = (angleRad * (180.0 / Math.PI) + 360) % 360;

        // Compass rose with 16 points (22.5° per direction)
        string[] directions =
        [
            "riftwardplus:north",
            "riftwardplus:north",
            "riftwardplus:northeast",
            "riftwardplus:northeast",
            "riftwardplus:east",
            "riftwardplus:southeast",
            "riftwardplus:southeast",
            "riftwardplus:south",
            "riftwardplus:south",
            "riftwardplus:southwest",
            "riftwardplus:southwest",
            "riftwardplus:west",
            "riftwardplus:west",
            "riftwardplus:northwest",
            "riftwardplus:northwest",
            "riftwardplus:north"
        ];

        int index = (int)Math.Round(angleDeg / 22.5) % 16;
        return directions[index];
    }

}
