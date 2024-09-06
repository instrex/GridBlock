using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common;

// chunk data
public class GridBlockChunk(int Id) {
    public int Id { get; init; } = Id;

    // state variables (should be saved)

    /// <summary>
    /// Checks if the chunk is safe to traverse.
    /// </summary>
    public bool IsUnlocked { get; set; }

    /// <summary>
    /// Checks if chunk has its reward decided.
    /// </summary>
    public bool IsUnlockCostCollapsed { get; set; }

    /// <summary>
    /// Item cost of this chunk to unlock.
    /// </summary>
    public Item UnlockCost { get; set; }

    // determenistic variables

    /// <summary>
    /// Cost group this chunk is assigned to.
    /// </summary>
    public CostGroup Group { get; set; }

    /// <summary>
    /// Only chunks with altered state should be saved.
    /// Currently, it's for following reasons:
    /// <list type="bullet">
    /// <item> The chunk is unlocked, meaning this progress has to persist. </item>
    /// <item> <see cref="UnlockCost"/> is decided. </item>
    /// <item> Surprise flags or other ratios are adjusted, as this process is done only during worldgen. </item>
    /// </list>
    /// </summary>
    public bool ShouldBeSaved => IsUnlocked || IsUnlockCostCollapsed;

    public void CollapseUnlockCost() {
        if (IsUnlocked || IsUnlockCostCollapsed)
            return;

        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        var pool = CostPoolGenerator.GetPool(Group, gridWorld.GridSeed + Id);

        List<GridBlockChunk> neighbours = [];
        for (var i = -1; i < 2; i++) {
            for(var k = -1; k < 2; k++) {
                if (i == 0 && k == 0) 
                    continue;

                var coord = gridWorld.Chunks.ToChunkCoord(Id);
                neighbours.Add(gridWorld.Chunks.GetByChunkCoord(coord + new Point(i, k)));
            }
        }

        // adjust weights so that items are duplicated less
        foreach (var neighbour in neighbours.Where(n => n != null && n.IsUnlockCostCollapsed && n.UnlockCost != null)) {
            pool.elements.RemoveAll(p => p.Item1.type == neighbour.UnlockCost.type);
        }

        // decide the item
        IsUnlockCostCollapsed = true;
        UnlockCost = pool.Get();
    }

    public void CollapseNeighboursUnlockCost() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        for (var i = -1; i < 2; i++) {
            for (var k = -1; k < 2; k++) {
                if (i == 0 && k == 0)
                    continue;

                var coord = gridWorld.Chunks.ToChunkCoord(Id);
                gridWorld.Chunks.GetByChunkCoord(coord + new Point(i, k)).CollapseUnlockCost();
            }
        }
    }

    /// <summary>
    /// Calculate CostGroup for a given chunk.
    /// </summary>
    public static CostGroup CalculateGroup(GridMap2D<GridBlockChunk> map, int id, out bool unlocked) {
        static bool CheckRegionSafety(Rectangle rect) {
            for (var x = rect.Left; x <= rect.Right; x++) {
                for (var y = rect.Top; y <= rect.Bottom; y++) {
                    if (Framing.GetTileSafely(x, y) is { HasTile: true, TileType: var tileType } && GridBlockWorld.SafetyTileExclusionSet.Contains(tileType))
                        return false;
                }
            }

            return true;
        }

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

        // make sure dungeon chunks are accessible at all times
        if (!CheckRegionSafety(new(chunkCoord.X * map.CellSize, chunkCoord.Y * map.CellSize, map.CellSize, map.CellSize))) {
            return CostGroup.Common;
        }

        // make 8x6 area around spawn substantially easier to unlock
        if (spawnDistX <= 4 && spawnDistY <= 3) {
            return CostGroup.Beginner;
        }

        // make 16x12 area around spawn slightly easier to unlock
        if (spawnDistX <= 8 && spawnDistY <= 8) {
            return CostGroup.Common;
        }

        if (dist < 0.75f) {
            return normalizedCoord.Y switch {
                > 0.8f => CostGroup.Hardcore,
                > 0.25f => CostGroup.Advanced,
                _ => CostGroup.Common
            };
        }

        return CostGroup.Hardcore;
    }
}
