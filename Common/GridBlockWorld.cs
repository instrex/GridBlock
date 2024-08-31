using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace GridBlock.Common;

public class GridBlockWorld : ModSystem {
    public const string SAVE_VERSION = "1";

    static readonly HashSet<int> SafetyTileExclusionSet = [
        TileID.BlueDungeonBrick,
        TileID.GreenDungeonBrick,
        TileID.PinkDungeonBrick,
        TileID.LihzahrdBrick,
        TileID.LihzahrdAltar,
        TileID.Hive
    ];

    // world-specific variables
    public GridMap2D<GridBlockChunk> Chunks { get; private set; }
    public string WorldVersion { get; private set; }

    public override void PostWorldGen() {
        WorldVersion = SAVE_VERSION;
        RegenerateChunks();
    }

    static CostGroup CalculateGroup(GridMap2D<GridBlockChunk> map, int id, out bool unlocked) {
        var chunkCoord = new Point(id % map.Bounds.X, id / map.Bounds.X);
        var normalizedCoord = new Vector2(chunkCoord.X / (float)map.Bounds.X, chunkCoord.Y / (float)map.Bounds.Y);
        var dist = MathF.Abs((normalizedCoord.X - 0.5f) / 0.5f);
        unlocked = false;

        // unlock small area
        var spawnChunkCoord = new Point(Main.spawnTileX / map.ChunkSize, Main.spawnTileY / map.ChunkSize);

        var spawnDistX = Math.Abs(spawnChunkCoord.X - chunkCoord.X);
        var spawnDistY = Math.Abs(spawnChunkCoord.Y - chunkCoord.Y);

        if (spawnDistX <= 1 && spawnDistY <= 1) {
            unlocked = true;
            return CostGroup.Beginner;
        }

        if (!CheckRegionSafety(new(chunkCoord.X * map.ChunkSize, chunkCoord.Y * map.ChunkSize, map.ChunkSize, map.ChunkSize))) {
            return CostGroup.Common;
        }

        // BEGINNER group: about 33% of the world horizontally and 45% vertically
        if (spawnDistX <= 4 && spawnDistY <= 3) {
            return CostGroup.Beginner;
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

    static bool CheckRegionSafety(Rectangle rect) {
        for (var x = rect.Left; x <= rect.Right; x++) {
            for (var y = rect.Top; y <= rect.Bottom; y++) {
                if (Framing.GetTileSafely(x, y) is { HasTile: true, TileType: var tileType} && SafetyTileExclusionSet.Contains(tileType))
                    return false;
            }
        }

        return true;
    }

    void RegenerateChunks() {
        var pools = (new[] { CostGroup.Beginner, CostGroup.Common, CostGroup.Advanced, CostGroup.Hardcore })
            .ToDictionary(k => k, v => CostPoolGenerator.GetPool(v));

        Chunks = new GridMap2D<GridBlockChunk>(40, Main.maxTilesX, Main.maxTilesY);
        Chunks.Fill((map, id) => {
            var group = CalculateGroup(map, id, out var unlocked);
            return new(id) { UnlockCost = pools[group].Get(), Group = group, IsUnlocked = unlocked };
        });
    }

    public override void PostUpdatePlayers() {
        if (PlayerInput.GetPressedKeys().Any(k => k == Microsoft.Xna.Framework.Input.Keys.G))
            RegenerateChunks();
    }
}
