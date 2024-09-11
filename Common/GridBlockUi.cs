using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;

namespace GridBlock.Common;

public class GridBlockUi {
    public bool BlockMouseLeft; 
    public bool IsMouseConsumedByUi; 

    public void Draw() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        BlockMouseLeft = false;

        if (Main.LocalPlayer?.active != true || gridWorld.Chunks is null)
            return;

        //Main.spriteBatch.End();
        //Main.spriteBatch.Begin(SpriteSortMode.Deferred,
        //    BlendState.AlphaBlend,
        //    SamplerState.LinearWrap,
        //    DepthStencilState.None,
        //    RasterizerState.CullNone,
        //    null,
        //    Main.GameViewMatrix.TransformationMatrix);

        var currentChunkCoord = new Point((int)Main.LocalPlayer.Center.X / 16 / gridWorld.Chunks.CellSize,
            (int)Main.LocalPlayer.Center.Y / 16 / gridWorld.Chunks.CellSize);

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

                var bgColor = Color.DarkRed * (0.35f + 0.05f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2 + nearbyChunk.Id * 0.05f));
                Main.spriteBatch.Draw(pixel, worldPos - Main.screenPosition,
                    null, bgColor, 0, Vector2.Zero, gridWorld.Chunks.CellSize * 16 / 2 - 2, 0, 0);

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

                    var isHoveringChunk = worldBounds.Contains(Main.MouseWorld.ToPoint());
                    var item = nearbyChunk.UnlockCost;
                    var playerHasItem = Main.LocalPlayer.CountItem(item.type, item.stack) >= item.stack;
                    var textColor = playerHasItem ? (isHoveringChunk ? Color.Gold : Color.White) : Color.Lerp(Color.Gray, Color.Red, 0.25f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.125f);
                    Utils.DrawBorderString(Main.spriteBatch, Lang.GetItemNameValue(nearbyChunk.UnlockCost.type),
                        itemIconPos + new Vector2(0, 24) - Main.screenPosition, textColor, isHoveringChunk ? 1.1f : 1, 0.5f);

                    ItemSlot.Draw(Main.spriteBatch, ref item, ItemSlot.Context.CreativeInfinite,
                        itemIconPos + new Vector2(-16) - Main.screenPosition, playerHasItem ? Color.White : Color.Gray);

                    // update cost in real-time and draw special reward indicator
                    if (nearbyChunk.Group == CostGroup.Expensive) {
                        nearbyChunk.UnlockCost.stack = Main.hardMode ? 50 : 15;

                        var rewardTex = ModContent.Request<Texture2D>("GridBlock/Assets/RewardIndicator");
                        var rewardPos = itemIconPos + new Vector2(0, 82) - Main.screenPosition;

                        for (var i = 0; i < 4; i++) {
                            Main.spriteBatch.Draw(rewardTex.Value, rewardPos
                                + (i * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly).ToRotationVector2()
                                * (4 + 2 * MathF.Sin(Main.GlobalTimeWrappedHourly * 6)),
                                null, Color.Gold with { A = 0 } * 0.25f, 0, rewardTex.Size() * 0.5f, 1f, 0, 0);
                        }

                        Main.spriteBatch.Draw(rewardTex.Value, rewardPos, null, Color.White, 0, rewardTex.Size() * 0.5f, 1f, 0, 0);
                    }

                    if (playerHasItem && worldBounds.Contains(Main.MouseWorld.ToPoint()) && Main.mouseLeft && Main.mouseLeftRelease) {
                        nearbyChunk.Unlock(Main.LocalPlayer);
                    }

                    if (nearbyChunk.UnlockCost.stack != 1) {
                        Utils.DrawBorderString(Main.spriteBatch, $"x{nearbyChunk.UnlockCost.stack}",
                            itemIconPos + new Vector2(0, 42 * (isHoveringChunk ? 1f : 1)) - Main.screenPosition, textColor, isHoveringChunk ? 1.1f : 1, 0.5f);
                    }
                }
            }
        }

        var topLeft = (Main.screenPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f).ToTileCoordinates();

        Main.spriteBatch.Draw(pixel, topLeft.ToWorldCoordinates(), null, Color.Green, 0, Vector2.Zero, 32, 0, 0);

        //Main.spriteBatch.End();
    }
}
