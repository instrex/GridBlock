using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Utilities;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace GridBlock.Common.Costs;

internal static class ChunkGroupGenerator {
    /// <summary>
    /// Tiles that contribute to "no surprise" status of the chunk.
    /// </summary>
    public static readonly HashSet<int> SafetyTileExclusionSet = [
        TileID.BlueDungeonBrick,
        TileID.GreenDungeonBrick,
        TileID.PinkDungeonBrick,
        TileID.LihzahrdBrick,
        TileID.LihzahrdAltar
    ];

    /// <summary>
    /// Calculate CostGroup for a given chunk.
    /// </summary>
    public static CostGroup CalculateGroup(GridMap2D<GridBlockChunk> map, UnifiedRandom gridRng, GridBlockChunk chunk, int id, out bool unlocked) {
        var chunkCoord = map.ToChunkCoord(id);
        var normalizedCoord = new Vector2(chunkCoord.X / (float)map.Bounds.X, chunkCoord.Y / (float)map.Bounds.Y);
        var dist = MathF.Abs((normalizedCoord.X - 0.5f) / 0.5f);
        unlocked = false;

        // unlock small area
        var spawnChunkCoord = new Point(Main.spawnTileX / map.CellSize, Main.spawnTileY / map.CellSize);

        var spawnDistX = Math.Abs(spawnChunkCoord.X - chunkCoord.X);
        var spawnDistY = Math.Abs(spawnChunkCoord.Y - chunkCoord.Y);

        // clear 3x3 area around spawn point
        if (spawnDistX <= 1 && spawnDistY <= 1) {
            unlocked = true;
            return CostGroup.Beginner;
        }

        // make 12x6 area around spawn substantially easier to unlock
        if (!Main.hardMode && spawnDistX <= 6 && spawnDistY <= 3) {
            return CostGroup.Beginner;
        }

        // make 20x12 area around spawn slightly easier to unlock
        if (spawnDistX <= 10 && spawnDistY <= 8) {
            return CostGroup.Common;
        }

        // check for empty tiles first
        if (gridRng.NextFloat() < 0.05f && chunk.EmptyTileAmount > 200) {
            return CostGroup.PaidReward;
        }

        if (dist < 0.75f) {
            return normalizedCoord.Y switch {
                > 0.8f => gridRng.NextFloat() < 0.025f ? CostGroup.Adventure : CostGroup.Advanced,
                > 0.25f => CostGroup.Advanced,
                _ => CostGroup.Common
            };
        }

        return gridRng.NextFloat() < 0.03f ? CostGroup.Adventure : CostGroup.Advanced;
    }
}
