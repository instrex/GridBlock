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
using Terraria.Localization;
using Terraria.GameContent;
using Microsoft.CodeAnalysis;
using Terraria.Audio;
using Terraria.ID;

namespace GridBlock.Common;

public class GridBlockUi {
    GridBlockChunk _lastHoveredChunk;
    float _holdDuration;
    bool _waitForMouseDownBeforeUnlocking;

    List<ItemFlowAnimation> _itemFlow = [];

    class ItemFlowAnimation {
        public Item item;
        public int timer;

        public float rotation, rotationSpeed;
        public float arcAmount;

        public Vector2 origin, target;
    }

    public void Draw() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (Main.LocalPlayer?.active != true || gridWorld.Chunks is null)
            return;

        var currentChunkCoord = new Point((int)Main.LocalPlayer.Center.X / 16 / gridWorld.Chunks.CellSize,
            (int)Main.LocalPlayer.Center.Y / 16 / gridWorld.Chunks.CellSize);

        var pixel = ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;

        var radiusX = Main.screenWidth / 16 / gridWorld.Chunks.CellSize;
        var radiusY = Main.screenHeight / 16 / gridWorld.Chunks.CellSize;

        GridBlockChunk currentlyHoveredChunk = default;

        const float UnlockHoldDuration = 90f;

        for (var x = -radiusX; x <= radiusX; x++) {
            for (var y = -radiusY; y <= radiusY; y++) {
                var nearbyChunkCoord = currentChunkCoord + new Point(x, y);
                var nearbyChunk = gridWorld.Chunks.GetByChunkCoord(nearbyChunkCoord);

                if (nearbyChunk is null || nearbyChunk.IsUnlocked)
                    continue;

                nearbyChunk.Update(Main.LocalPlayer);

                var worldPos = nearbyChunkCoord.ToVector2() * gridWorld.Chunks.CellSize * 16;
                var worldBounds = new Rectangle((int)worldPos.X, (int)worldPos.Y,
                    gridWorld.Chunks.CellSize * 16, gridWorld.Chunks.CellSize * 16);

                var bgColor = (nearbyChunk.Group == CostGroup.Expensive ? Color.Gold * 0.5f : Color.DarkRed) 
                    * (0.25f + 0.05f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2 + nearbyChunk.Id * 0.05f));
                Main.spriteBatch.Draw(pixel, worldPos - Main.screenPosition,
                    null, bgColor, 0, Vector2.Zero, gridWorld.Chunks.CellSize * 16 / 2, 0, 0);

                var gradientTex = ModContent.Request<Texture2D>("GridBlock/Assets/BorderGradient").Value;

                {
                    var size = gridWorld.Chunks.CellSize * 16f;
                    for (var i = 0f; i < size; i += 64) {
                        for (var k = 0f; k < size; k += 64) {
                            if (i != 0 && i != size - 64 && k != 0 && k != size - 64)
                                continue;

                            var isCorner = (i == 0 && k == 0) || (i == size - 64 && k == 0)
                                || (i == size - 64 && k == size - 64)
                                || (i == 0 && k == size - 64);

                            var rotation = 0f;
                            if (i == 0 && k == 0) {
                                rotation = 0;
                            } else if (i == size - 64 && k == 0) {
                                rotation = MathHelper.PiOver2;
                            } else if (i == size - 64 && k == size - 64) {
                                rotation = MathHelper.Pi;
                            } else if (i == 0 && k == size - 64) {
                                rotation = -MathHelper.PiOver2;
                            } else if (k == 0) {
                                rotation = MathHelper.PiOver2;
                            } else if (i == size - 64) {
                                rotation = MathHelper.Pi;
                            } else if (k == size - 64) {
                                rotation = -MathHelper.PiOver2;
                            }

                            Main.spriteBatch.Draw(gradientTex,
                                worldBounds.TopLeft() + new Vector2(i, k) + new Vector2(32) - Main.screenPosition,
                                new Rectangle(2, isCorner ? 2 : 68, 64, 64),
                                Color.Red with { A = 0 } * (0.05f + 0.025f * MathF.Sin(Main.GlobalTimeWrappedHourly * 4 + i * 0.01f + k * 0.01f)),
                                rotation,
                                new Vector2(32),
                                1,
                                0,
                                0
                            );
                        }
                    }
                }
                
                if (nearbyChunk.IsUnlockCostCollapsed && nearbyChunk.UnlockCost != null) {
                    var screenBounds = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y,
                        Main.screenWidth, Main.screenHeight);

                    screenBounds.Inflate(-64, -72);

                    var itemIconPos = worldPos + new Vector2(gridWorld.Chunks.CellSize * 16 * 0.5f);
                    //if (_lastHoveredChunk == nearbyChunk && _holdDuration > 0) {
                    //    itemIconPos += MathF.Sin(Main.GlobalTimeWrappedHourly * 24).ToRotationVector2() * (_holdDuration / 10f);
                    //}

                    var isHoveringChunk = worldBounds.Contains(Main.MouseWorld.ToPoint());

                    if (isHoveringChunk) {
                        if (_lastHoveredChunk != nearbyChunk) {
                            _waitForMouseDownBeforeUnlocking = true;
                            _holdDuration = 0;
                        }

                        currentlyHoveredChunk = nearbyChunk;
                        _lastHoveredChunk = nearbyChunk;
                    }

                    var item = nearbyChunk.UnlockCost;
                    var playerHasItem = nearbyChunk.CheckUnlockRequirementsForPlayer(Main.LocalPlayer);
                    var canUnlockChunk = playerHasItem && Main.LocalPlayer.Distance(worldBounds.Center()) < Main.screenHeight * 0.75f;
                    var textColor = playerHasItem ? (canUnlockChunk && isHoveringChunk ? Color.Gold : Color.White) : Color.Lerp(Color.Gray, Color.Red, 0.25f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.125f);

                    if (!screenBounds.Contains(itemIconPos.ToPoint())) {
                        itemIconPos.X = Math.Clamp(itemIconPos.X,
                            MathF.Min(screenBounds.Left, worldBounds.Right - 64),
                            MathF.Max(screenBounds.Right, worldBounds.Left + 64));

                        var heightExt = canUnlockChunk ? 32 : 0;
                        itemIconPos.Y = Math.Clamp(itemIconPos.Y,
                            MathF.Min(screenBounds.Top, worldBounds.Bottom - 76),
                            MathF.Max(screenBounds.Bottom, worldBounds.Top + (32 + heightExt * 2)));
                    }

                    Utils.DrawBorderString(Main.spriteBatch, Lang.GetItemNameValue(nearbyChunk.UnlockCost.type),
                        itemIconPos + new Vector2(0, 24) - Main.screenPosition, textColor, canUnlockChunk && isHoveringChunk ? 1.1f : 1, 0.5f);

                    var slotTex = ModContent.Request<Texture2D>("GridBlock/Assets/UnlockSlot").Value;
                    Main.spriteBatch.Draw(slotTex,
                        itemIconPos - Main.screenPosition,
                        null,
                        (playerHasItem ? Color.Black : Color.Red) * 0.25f,
                        0,
                        new Vector2(21),
                        1,
                        0,
                        0
                    );

                    Main.DrawItemIcon(Main.spriteBatch, item, itemIconPos - Main.screenPosition,
                        playerHasItem ? Color.White : Color.Gray, 32f);

                    //ItemSlot.Draw(Main.spriteBatch, ref item, ItemSlot.Context.CreativeInfinite,
                    //    itemIconPos + new Vector2(-16) - Main.screenPosition, playerHasItem ? Color.White : Color.Gray);

                    var iconOffsetY = 0f;

                    if (canUnlockChunk) {
                        var scale = _lastHoveredChunk == nearbyChunk ? 1f + (_holdDuration / UnlockHoldDuration) * 0.25f : 1f;
                        var text = Language.GetTextValue("Mods.GridBlock.HoldToUnlock");
                        var size = FontAssets.MouseText.Value.MeasureString(text);
                        var pos = itemIconPos + new Vector2(size.X * -0.5f * scale, 76) - Main.screenPosition;
                        Utils.DrawBorderString(Main.spriteBatch, text,
                            pos, Color.DarkGray * (isHoveringChunk ? 1.0f : 0.5f), scale, 0f);

                        if (_holdDuration > 0 && _lastHoveredChunk == nearbyChunk) {
                            var substr = text[..(int)(text.Length * (_holdDuration / UnlockHoldDuration))];
                            Utils.DrawBorderString(Main.spriteBatch, substr, pos, Color.Gold, scale, 0f);

                            for (var i = 0; i < 4; i++) {
                                Utils.DrawBorderString(Main.spriteBatch, substr, pos 
                                    + (i * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly).ToRotationVector2()
                                    * (4), 
                                    Color.Gold with { A = 0 } * (0.15f + 0.1f * (_holdDuration / UnlockHoldDuration)), scale, 0f);
                            }
                        }

                        iconOffsetY += size.Y * scale;
                    }

                    // update cost in real-time and draw special reward indicator
                    if (nearbyChunk.Group == CostGroup.Expensive) {
                        var rewardTex = ModContent.Request<Texture2D>("GridBlock/Assets/RewardIndicator");
                        var rewardPos = itemIconPos + new Vector2(0, 82 + iconOffsetY) - Main.screenPosition;

                        for (var i = 0; i < 4; i++) {
                            Main.spriteBatch.Draw(rewardTex.Value, rewardPos
                                + (i * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly).ToRotationVector2()
                                * (4 + 2 * MathF.Sin(Main.GlobalTimeWrappedHourly * 6)),
                                null, Color.Gold with { A = 0 } * 0.25f, 0, rewardTex.Size() * 0.5f, 1f, 0, 0);
                        }

                        Main.spriteBatch.Draw(rewardTex.Value, rewardPos, null, Color.White, 0, rewardTex.Size() * 0.5f, 1f, 0, 0);
                    }

                    if (canUnlockChunk && isHoveringChunk) {
                        if (_waitForMouseDownBeforeUnlocking && Main.mouseLeftRelease)
                            _waitForMouseDownBeforeUnlocking = false;

                        if (!_waitForMouseDownBeforeUnlocking && Main.mouseLeft) {
                            _holdDuration += 1f;
                            if (_holdDuration < UnlockHoldDuration - 40 && (int)_holdDuration % 5 == 0) {
                                SoundEngine.PlaySound(SoundID.Item1);
                                _itemFlow.Add(new ItemFlowAnimation { 
                                    item = nearbyChunk.UnlockCost,
                                    origin = Main.LocalPlayer.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 16),
                                    target = itemIconPos + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 16),

                                    rotation = Main.rand.NextFloat(6.28f),
                                    rotationSpeed = Main.rand.NextFloat(-0.5f, 0.5f),
                                    arcAmount = Main.rand.NextFloat(-1f, 1f),
                                });
                            }
                        } else _holdDuration = 0;

                        if (_holdDuration >= UnlockHoldDuration) {
                            nearbyChunk.Unlock(Main.LocalPlayer);
                            currentlyHoveredChunk = null;
                            _holdDuration = 0;
                        }
                    }

                    if (nearbyChunk.UnlockCost.stack != 1) {
                        Utils.DrawBorderString(Main.spriteBatch, $"x{nearbyChunk.UnlockCost.stack}",
                            itemIconPos + new Vector2(0, 42 * (isHoveringChunk ? 1f : 1)) - Main.screenPosition, textColor,
                            canUnlockChunk && isHoveringChunk ? 1.1f : 1, 0.5f);
                    }
                }
            }
        }

        if (currentlyHoveredChunk == null || currentlyHoveredChunk != _lastHoveredChunk) {
            _waitForMouseDownBeforeUnlocking = true;
            _holdDuration = 0;
        }

        var shouldClearAnimBuffer = false;
        foreach (var anim in _itemFlow) {
            var progress = anim.timer / 60f;

            Main.GetItemDrawFrame(anim.item.type, out var tex, out var frame);

            var pos = EaseHelper.SampleCubicBezier(anim.origin, 
                Vector2.Lerp(anim.origin, anim.target, 0.5f) + anim.origin.DirectionTo(anim.target).RotatedBy(MathHelper.PiOver2) * anim.arcAmount * 260f,
                anim.target, EaseHelper.SineInOut(progress));

            var scale = (1f - MathF.Abs(progress - 0.5f) / 0.5f);
            Main.spriteBatch.Draw(tex,
                pos - Main.screenPosition,
                frame,
                Color.White * scale,
                anim.rotation,
                frame.Size() * 0.5f,
                /*progress < 0.2f ? progress / 0.2f : (progress >= 0.8f ? 1f - (progress - 0.8f) / 0.2f : 1f)*/
                scale * 1.25f,
                0,
                0
            );

            if (++anim.timer >= 60f) shouldClearAnimBuffer = true;
            anim.rotation += anim.rotationSpeed;
        }

        if (shouldClearAnimBuffer)
        _itemFlow.RemoveAll(i => i.timer >= 60);

        //Main.spriteBatch.End();
    }
}
