using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        // clear 3x3 area around spawn point
        if (spawnDistX <= 1 && spawnDistY <= 1) {
            unlocked = true;
            return CostGroup.Beginner;
        }

        // make sure dungeon chunks are accessible at all times
        if (!CheckRegionSafety(new(chunkCoord.X * map.ChunkSize, chunkCoord.Y * map.ChunkSize, map.ChunkSize, map.ChunkSize))) {
            return CostGroup.Common;
        }

        // make 8x6 area around spawn slightly easier to unlock
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
            .ToDictionary(k => k, CostPoolGenerator.GetPool);

        Chunks = new GridMap2D<GridBlockChunk>(20, Main.maxTilesX, Main.maxTilesY);
        Chunks.Fill((map, id) => {
            var group = CalculateGroup(map, id, out var unlocked);
            return new(id) { UnlockCost = pools[group].Get(), Group = group, IsUnlocked = unlocked };
        });
    }

    public override void PostUpdatePlayers() {
        if (PlayerInput.GetPressedKeys().Any(k => k == Microsoft.Xna.Framework.Input.Keys.G))
            RegenerateChunks();

        
    }

    public override void UpdateUI(GameTime gameTime) {

    }

    public override void PostDrawTiles() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (Main.LocalPlayer?.active != true || gridWorld.Chunks is null)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix);

        var currentChunkCoord = new Point((int)Main.LocalPlayer.Center.X / 16 / gridWorld.Chunks.ChunkSize,
            (int)Main.LocalPlayer.Center.Y / 16 / gridWorld.Chunks.ChunkSize);

        var currentChunk = gridWorld.Chunks.GetByChunkCoord(currentChunkCoord);

        Utils.DrawBorderString(Main.spriteBatch, $"ID: {currentChunk.Id}\nCOORD: {currentChunkCoord}\nUNLOCK: {currentChunk.UnlockCost?.Name}", 
            Main.LocalPlayer.Center + new Vector2(0, 32) - Main.screenPosition, Color.White);

        var pixel = ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;

        var radiusX = Main.screenWidth / 16 / gridWorld.Chunks.ChunkSize;
        var radiusY = Main.screenHeight / 16 / gridWorld.Chunks.ChunkSize;

        for (var x = -radiusX; x <= radiusX; x++) {
            for (var y = -radiusY; y <= radiusY; y++) {
                var nearbyChunkCoord = currentChunkCoord + new Point(x, y);
                var nearbyChunk = gridWorld.Chunks.GetByChunkCoord(nearbyChunkCoord);
                Main.spriteBatch.Draw(pixel, nearbyChunkCoord.ToVector2() * gridWorld.Chunks.ChunkSize * 16 - Main.screenPosition,
                    null, (nearbyChunk.IsUnlocked ? Color.Green : Color.Red) * 0.5f, 0, Vector2.Zero, gridWorld.Chunks.ChunkSize * 16 / 2 - 2, 0, 0);
            }
        }

        var topLeft = (Main.screenPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f).ToTileCoordinates();

        Main.spriteBatch.Draw(pixel, topLeft.ToWorldCoordinates(), null, Color.Green, 0, Vector2.Zero, 32, 0, 0);

        Main.spriteBatch.End();
    }
}
