using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using GridBlock.Content.Surprises;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace GridBlock.Common;

public class GridBlockWorld : ModSystem {
    public const string SAVE_VERSION = "test-1";

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

    public static GridBlockWorld Instance => ModContent.GetInstance<GridBlockWorld>();

    // world-specific variables
    public GridMap2D<GridBlockChunk> Chunks { get; private set; }
    public string WorldVersion { get; private set; }
    public int GridSeed { get; private set; }

    public override void PostWorldGen() {
        GridSeed = WorldGen.genRand.Next();
        WorldVersion = SAVE_VERSION;
        RegenerateChunks();
    }

    public override void OnWorldUnload() {
        WorldVersion = "";
        GridSeed = 0;
        Chunks = null;
    }

    void RegenerateChunks() {
        List<GridBlockChunk> chunksToCollapseNeighboursFor = [];

        Chunks = new GridMap2D<GridBlockChunk>(40, Main.maxTilesX, Main.maxTilesY);
        Chunks.Fill((map, id) => {
            var group = GridBlockChunk.CalculateGroup(map, id, out var unlocked);

            var chunk = new GridBlockChunk(id) { Group = group, IsUnlocked = unlocked };
            if (unlocked) chunksToCollapseNeighboursFor.Add(chunk);

            return chunk;
        });

        chunksToCollapseNeighboursFor.ForEach(c => c.CollapseNeighboursUnlockCost());
    }

    public override void PostUpdatePlayers() {
        if (PlayerInput.GetPressedKeys().Any(k => k == Microsoft.Xna.Framework.Input.Keys.G))
            RegenerateChunks();

        if (Chunks != null && PlayerInput.GetPressedKeys().Any(k => k == Microsoft.Xna.Framework.Input.Keys.F)) {
            var chunk = Chunks.GetByWorldPos(Main.MouseWorld);
            if (chunk.IsUnlocked = !chunk.IsUnlocked) {
                chunk.CollapseNeighboursUnlockCost();
            }
        }
    }

    public override void PostDrawTiles() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (Main.LocalPlayer?.active != true || gridWorld.Chunks is null)
            return;

        Main.spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearWrap,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null,
            Main.GameViewMatrix.TransformationMatrix);

        var currentChunkCoord = new Point((int)Main.LocalPlayer.Center.X / 16 / gridWorld.Chunks.CellSize,
            (int)Main.LocalPlayer.Center.Y / 16 / gridWorld.Chunks.CellSize);

        var currentChunk = gridWorld.Chunks.GetByChunkCoord(currentChunkCoord);

        var pixel = ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;

        var radiusX = Main.screenWidth / 16 / gridWorld.Chunks.CellSize;
        var radiusY = Main.screenHeight / 16 / gridWorld.Chunks.CellSize;

        for (var x = -radiusX; x <= radiusX; x++) {
            for (var y = -radiusY; y <= radiusY; y++) {
                var nearbyChunkCoord = currentChunkCoord + new Point(x, y);
                var nearbyChunk = gridWorld.Chunks.GetByChunkCoord(nearbyChunkCoord);

                if (nearbyChunk is null || nearbyChunk.IsUnlocked)
                    continue;

                var worldPos = nearbyChunkCoord.ToVector2() * gridWorld.Chunks.CellSize * 16;
                var worldBounds = new Rectangle((int)worldPos.X, (int)worldPos.Y,
                    gridWorld.Chunks.CellSize * 16, gridWorld.Chunks.CellSize * 16);

                Main.spriteBatch.Draw(pixel, worldPos - Main.screenPosition,
                    null, (nearbyChunk.IsUnlocked ? Color.Green : Color.DarkRed) * 0.5f, 0, Vector2.Zero, gridWorld.Chunks.CellSize * 16 / 2 - 2, 0, 0);

                if (nearbyChunk.IsUnlockCostCollapsed && nearbyChunk.UnlockCost != null) {
                    var screenBounds = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y,
                        Main.screenWidth, Main.screenHeight);

                    screenBounds.Inflate(-64, -72);

                    var itemIconPos = worldPos + new Vector2(gridWorld.Chunks.CellSize * 16 * 0.5f);

                    if (!screenBounds.Contains(itemIconPos.ToPoint())) {
                        itemIconPos.X = Math.Clamp(itemIconPos.X, 
                            MathF.Min(screenBounds.Left, worldBounds.Right - 64), 
                            MathF.Max(screenBounds.Right, worldBounds.Left + 64));

                        itemIconPos.Y = Math.Clamp(itemIconPos.Y, 
                            MathF.Min(screenBounds.Top, worldBounds.Bottom - 76), 
                            MathF.Max(screenBounds.Bottom, worldBounds.Top + 32));
                    }

                    var item = nearbyChunk.UnlockCost;
                    var playerHasItem = Main.LocalPlayer.CountItem(item.type, item.stack) >= item.stack;
                    var textColor = playerHasItem ? Color.White : Color.Lerp(Color.Gray, Color.Red, 0.25f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.125f);
                    Utils.DrawBorderString(Main.spriteBatch, Lang.GetItemNameValue(nearbyChunk.UnlockCost.type),
                            itemIconPos + new Vector2(0, 24) - Main.screenPosition, textColor, 1, 0.5f);

                    ItemSlot.Draw(Main.spriteBatch, ref item, ItemSlot.Context.CreativeInfinite, 
                        itemIconPos + new Vector2(-16) - Main.screenPosition, playerHasItem ? Color.White : Color.Gray);

                    if (playerHasItem && worldBounds.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease) {
                        for (var i = 0; i < item.stack; i++) Main.LocalPlayer.ConsumeItem(item.type);
                        SoundEngine.PlaySound(SoundID.Unlock);

                        // TODO: unlock chunks more elegantly
                        nearbyChunk.CollapseNeighboursUnlockCost();
                        nearbyChunk.IsUnlocked = true;

                        // fun!
                        // CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Gold, "ДА БУДЕТ СВЕТ!", true);
                        //CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Red, "Липкая ситуация...", true);
                        CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Red, "Упс!  Пол изменился!", true);
                        //CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Red, "Гости из будущего...", true);
                        //CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Gold, "Награда за смелость!", true);
                        //CombatText.NewText(new((int)itemIconPos.X - 16, (int)itemIconPos.Y + 16, 32, 32), Color.Gold, "Хилимся-живём!", true);
                        ModContent.GetInstance<GenderSwapSurprise>().Trigger(Main.LocalPlayer, nearbyChunk);
                    }

                    if (nearbyChunk.UnlockCost.stack != 1) {
                        Utils.DrawBorderString(Main.spriteBatch, $"x{nearbyChunk.UnlockCost.stack}",
                            itemIconPos + new Vector2(0, 42) - Main.screenPosition, textColor, 1, 0.5f);
                    }
                }    
            }
        }

        var topLeft = (Main.screenPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f).ToTileCoordinates();

        Main.spriteBatch.Draw(pixel, topLeft.ToWorldCoordinates(), null, Color.Green, 0, Vector2.Zero, 32, 0, 0);

        Main.spriteBatch.End();
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["GridVersion"] = WorldVersion;
        tag["GridSeed"] = GridSeed;

        List<TagCompound> savedChunks = [];
        for (var id = 0; id < Chunks.Length; id++) {
            var chunk = Chunks.GetById(id);
            if (!chunk.ShouldBeSaved) 
                continue;

            var chunkTag = new TagCompound { ["ChunkId"] = id };
            savedChunks.Add(chunkTag);

            if (chunk.IsUnlocked) {
                // if it's unlocked, no other information is required
                chunkTag[nameof(GridBlockChunk.IsUnlocked)] = true;
                continue;
            }

            if (chunk.IsUnlockCostCollapsed) {
                if (chunk.UnlockCost is null) {
                    ModContent.GetInstance<GridBlock>().Logger.Warn("Attempted to save a chunk with collapsed unlock cost but no item instance? Weird...");
                }

                chunkTag[nameof(GridBlockChunk.IsUnlockCostCollapsed)] = true;
                chunkTag[nameof(GridBlockChunk.UnlockCost)] = chunk.UnlockCost;
            }
        }

        tag["GridChunks"] = savedChunks;
    }

    public override void LoadWorldData(TagCompound tag) {
        // create determenistic chunks
        RegenerateChunks();

        try {
            WorldVersion = tag.Get<string>("GridVersion");
            GridSeed = tag.Get<int>("GridSeed");

            foreach (var chunkTag in tag.Get<List<TagCompound>>("GridChunks")) {
                var id = chunkTag.Get<int>("ChunkId");

                if (Chunks.GetById(id) is not GridBlockChunk chunk) {
                    Mod.Logger.Warn($"Attempted to set non-existent chunk during loading. (Id: {id})");
                    continue;
                }

                // unlock the chunk if its set as unlocked
                if (chunkTag.ContainsKey(nameof(GridBlockChunk.IsUnlocked))) {
                    chunk.IsUnlocked = true;
                    continue;
                }

                // set collapsed unlock
                if (chunkTag.ContainsKey(nameof(GridBlockChunk.IsUnlockCostCollapsed))) {
                    var item = chunkTag.Get<Item>(nameof(GridBlockChunk.UnlockCost));
                    chunk.IsUnlockCostCollapsed = true;
                    chunk.UnlockCost = item;
                    continue;
                }
            }

        } catch (Exception ex) {
            Mod.Logger.Error("Error loading GridBlock data!");
            Mod.Logger.Error(ex);
        }
    }
}
