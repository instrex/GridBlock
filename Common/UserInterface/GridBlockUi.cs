using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
using Terraria.GameContent.UI;
using GridBlock.Common.UserInterface.Animations;
using static System.Net.Mime.MediaTypeNames;
using GridBlock.Content.Buffs;

namespace GridBlock.Common.UserInterface;

public class GridBlockUi {
    public static bool IsHoveringChunk { get; private set; }

    GridBlockChunk _lastHoveredChunk;
    float _holdDuration, _rerollShake;
    bool _waitForMouseDownBeforeUnlocking;

    public static readonly GridBlockUiAnimations Animations = new();

    public void Draw() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        IsHoveringChunk = false;

        if (Main.LocalPlayer?.active != true || gridWorld.Chunks is null)
            return;

        var currentChunkCoord = new Point((int)Main.LocalPlayer.Center.X / 16 / gridWorld.Chunks.CellSize,
            (int)Main.LocalPlayer.Center.Y / 16 / gridWorld.Chunks.CellSize);

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

                var bgColor = (nearbyChunk.Group == CostGroup.PaidReward ? Color.Gold * 0.5f : Color.DarkRed)
                    * (0.25f + 0.05f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2 + nearbyChunk.Id * 0.05f));
                Main.spriteBatch.Draw(ModAssets.PixelTex, worldPos - Main.screenPosition,
                    null, bgColor, 0, Vector2.Zero, gridWorld.Chunks.CellSize * 16 / 2, 0, 0);

                {
                    var size = gridWorld.Chunks.CellSize * 16f;
                    for (var i = 0f; i < size; i += 64) {
                        for (var k = 0f; k < size; k += 64) {
                            if (i != 0 && i != size - 64 && k != 0 && k != size - 64)
                                continue;

                            var isCorner = i == 0 && k == 0 || i == size - 64 && k == 0
                                || i == size - 64 && k == size - 64
                                || i == 0 && k == size - 64;

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

                            Main.spriteBatch.Draw(ModAssets.BorderGradientTex,
                                worldBounds.TopLeft() + new Vector2(i, k) + new Vector2(32) - Main.screenPosition,
                                isCorner ? ModAssets.BorderGradient_Corner : ModAssets.BorderGradient_Side,
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
                    var isHoveringChunk = worldBounds.Contains(Main.MouseWorld.ToPoint());

                    if (_rerollShake > 0 && isHoveringChunk && _lastHoveredChunk == nearbyChunk) {
                        itemIconPos.X += MathF.Sin(Main.GlobalTimeWrappedHourly * 36) * 12 * (_rerollShake / 60f);
                        _rerollShake -= 2;
                    }

                    if (isHoveringChunk) {
                        if (_lastHoveredChunk != nearbyChunk) {
                            _waitForMouseDownBeforeUnlocking = true;
                            _holdDuration = 0;
                        }

                        currentlyHoveredChunk = nearbyChunk;
                        _lastHoveredChunk = nearbyChunk;
                    }

                    var eventHideItem = Main.LocalPlayer.HasBuff<MysteryBuff>() || nearbyChunk.Modifier.HasFlag(ChunkModifier.Mystery);

                    var item = nearbyChunk.UnlockCost;
                    var costModifier = nearbyChunk.GetCostModifier(Main.LocalPlayer);
                    var stack = Math.Max(1, Math.Floor(item.stack * costModifier));

                    var eventDiscount = stack != item.stack;
                    var eventNegativeDiscount = stack > item.stack;

                    var playerHasItem = nearbyChunk.CheckUnlockRequirementsForPlayer(Main.LocalPlayer, out var storageContext);
                    var playerInRangeOfUnlock = Main.LocalPlayer.Distance(worldBounds.Center()) < Main.screenHeight * 0.75f && !Main.LocalPlayer.dead;
                    var canUnlockChunk = playerHasItem && playerInRangeOfUnlock;
                    var canRerollChunk = gridWorld.RerollCount > 0 && nearbyChunk.Group != CostGroup.PaidReward && !Main.playerInventory;
                    var textColor = playerHasItem ? canUnlockChunk && isHoveringChunk ? 
                        (eventDiscount && item.stack != stack ? (eventNegativeDiscount ? Color.Red : Color.LightGreen) : Color.Gold) 
                        : ItemRarity.GetColor(item.rare) : Color.Lerp(Color.Gray, Color.Red, 0.25f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 0.125f);

                    if (!screenBounds.Contains(itemIconPos.ToPoint())) {
                        var horizontalMargin = 64;
                        itemIconPos.X = Math.Clamp(itemIconPos.X,
                            MathF.Min(screenBounds.Left, worldBounds.Right - horizontalMargin),
                            MathF.Max(screenBounds.Right, worldBounds.Left + horizontalMargin));

                        var heightExt = canUnlockChunk ? 32 : 0;
                        itemIconPos.Y = Math.Clamp(itemIconPos.Y,
                            MathF.Min(screenBounds.Top, worldBounds.Bottom - 76),
                            MathF.Max(screenBounds.Bottom, worldBounds.Top + (32 + heightExt * 2)));
                    }

                    // trigger click protection
                    if (isHoveringChunk) {
                        if (canUnlockChunk) IsHoveringChunk = true;
                    }

                    Utils.DrawBorderString(Main.spriteBatch, eventHideItem ? "???" : Lang.GetItemNameValue(nearbyChunk.UnlockCost.type),
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
                        eventHideItem ? Color.Black : (playerHasItem ? Color.White : Color.Gray), 32f);

                    // draw storage icon
                    if (storageContext > 0) {
                        var glowTex = ModContent.Request<Texture2D>("GridBlock/Assets/UnlockSlot_Glow").Value;

                        var storageIcon = ContentSamples.ItemsByType[storageContext switch {
                            2 => ItemID.Safe,
                            3 => ItemID.DefendersForge,
                            4 => ItemID.ClosedVoidBag,
                            _ => ItemID.PiggyBank,
                        }];

                        var storageIconPos = itemIconPos + new Vector2(0, -42);

                        var accentColor = storageContext switch {
                            2 => Color.Gray,
                            3 => Color.Cyan,
                            4 => Color.Purple,
                            _ => Color.Magenta,
                        };

                        Main.spriteBatch.Draw(glowTex,
                            itemIconPos + new Vector2(-4) - Main.screenPosition,
                            null,
                            accentColor with { A = 0 } * 0.25f,
                            0,
                            new Vector2(21),
                            1,
                            0,
                            0
                        );

                        Main.spriteBatch.Draw(slotTex,
                            storageIconPos - Main.screenPosition,
                            null,
                            accentColor * 0.25f,
                            0,
                            new Vector2(21),
                            0.75f,
                            0,
                            0
                        );

                        Main.DrawItemIcon(Main.spriteBatch, storageIcon, storageIconPos - Main.screenPosition,
                            playerHasItem ? Color.White : Color.Gray, 24f);
                    }

                    var showStackText = stack > 1 || stack != item.stack;

                    var iconOffsetY = (showStackText ? -12f : -22f) + (isHoveringChunk ? 4f : 0);

                    if (canUnlockChunk) {
                        var scale = _lastHoveredChunk == nearbyChunk ? 1f + EaseHelper.ExpoOut(_holdDuration / UnlockHoldDuration) * 0.25f : 1f;
                        var text = Language.GetTextValue("Mods.GridBlock.HoldToUnlock");
                        var size = FontAssets.MouseText.Value.MeasureString(text);
                        var pos = itemIconPos + new Vector2(size.X * -0.5f * scale, 76 + iconOffsetY) - Main.screenPosition;

                        Utils.DrawBorderString(Main.spriteBatch, text,
                            pos, Color.DarkGray * (isHoveringChunk ? 1.0f : 0.5f), scale, 0f);

                        if (_holdDuration > 0 && _lastHoveredChunk == nearbyChunk) {
                            var substr = text[..(int)(text.Length * (_holdDuration / UnlockHoldDuration))];
                            Utils.DrawBorderString(Main.spriteBatch, substr, pos, Color.Gold, scale, 0f);

                            for (var i = 0; i < 4; i++) {
                                Utils.DrawBorderString(Main.spriteBatch, substr, pos
                                    + (i * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly).ToRotationVector2()
                                    * 4,
                                    Color.Gold with { A = 0 } * (0.15f + 0.1f * (_holdDuration / UnlockHoldDuration)), scale, 0f);
                            }

                            var f = _holdDuration / UnlockHoldDuration;

                            var unlockGlowOpacity = f < 0.2f ? f / 0.2f : 1f;

                            Main.spriteBatch.Draw(ModAssets.LightTex,
                                pos + new Vector2(size.X * f * 1.25f, size.Y * 0.5f),
                                null,
                                Color.Gold with { A = 0 } * 0.25f * unlockGlowOpacity,
                                0,
                                new Vector2(45),
                                new Vector2(0.25f, 1f),
                                0,
                                0
                            );

                            Main.spriteBatch.Draw(ModAssets.LightTex,
                                pos + new Vector2(size.X * f * 0.5f, size.Y * 0.5f),
                                null,
                                Color.Gold with { A = 0 } * 0.25f * unlockGlowOpacity,
                                0,
                                new Vector2(45),
                                new Vector2(1 + f * 2, 0.75f),
                                0,
                                0
                            );
                        }

                        iconOffsetY += size.Y * scale;
                    }

                    if (nearbyChunk.Group != CostGroup.PaidReward && playerInRangeOfUnlock && isHoveringChunk && _holdDuration <= 0) {
                        var text = Language.GetTextValue("Mods.GridBlock.Reroll", gridWorld.RerollCount);
                        var size = FontAssets.MouseText.Value.MeasureString(text) * 0.9f;
                        var pos = itemIconPos + new Vector2(size.X * -0.5f + 16 * 0.65f, 72 + iconOffsetY) - Main.screenPosition;

                        Utils.DrawBorderString(Main.spriteBatch, text,
                            pos, Color.DarkGray * (canRerollChunk && isHoveringChunk ? 1.0f : 0.5f), 0.9f, 0f);

                        for (var i = 0; i < 4; i++) {
                            Main.spriteBatch.Draw(
                                ModAssets.MouseIconTex,
                                pos + new Vector2(-12, size.Y * 0.5f) + (MathHelper.PiOver2 * i).ToRotationVector2() * new Vector2(2 * 0.65f),
                                ModAssets.MouseIconTex_Right,
                                Color.Black * 0.4f,
                                0,
                                ModAssets.MouseIconTex_Left.Size() * 0.5f,
                                0.65f,
                                0,
                                0
                            );
                        }

                        Main.spriteBatch.Draw(
                            ModAssets.MouseIconTex,
                            pos + new Vector2(-12, size.Y * 0.5f),
                            ModAssets.MouseIconTex_Right,
                            canRerollChunk && isHoveringChunk ? Color.White : Color.DarkGray,
                            0,
                            ModAssets.MouseIconTex_Left.Size() * 0.5f,
                            0.65f,
                            0,
                            0
                        );

                        iconOffsetY += size.Y;
                    }

                    var specialIndicatorPos = itemIconPos + new Vector2(0, 92 + iconOffsetY) - Main.screenPosition;

                    // draw special reward indicators
                    if (nearbyChunk.Group == CostGroup.PaidReward) {
                        for (var i = 0; i < 4; i++) {
                            Main.spriteBatch.Draw(ModAssets.RewardIndicatorTex, specialIndicatorPos
                                + (i * MathHelper.PiOver2 + Main.GlobalTimeWrappedHourly).ToRotationVector2()
                                * (4 + 2 * MathF.Sin(Main.GlobalTimeWrappedHourly * 6)),
                                null, Color.Gold with { A = 0 } * 0.25f, 0, ModAssets.RewardIndicatorTex.Size() * 0.5f, 1f, 0, 0);
                        }

                        Main.spriteBatch.Draw(ModAssets.RewardIndicatorTex, specialIndicatorPos, null, Color.White, 0, ModAssets.RewardIndicatorTex.Size() * 0.5f, 1f, 0, 0);
                    } else {
                        var color = (playerHasItem ? Color.Black : Color.Red * 0.5f) * 0.25f;
                        if (_holdDuration > 0 && _lastHoveredChunk == nearbyChunk) {
                            color = Color.Lerp(color, Color.Transparent, _holdDuration / UnlockHoldDuration);
                        }

                        var hasDiscountTag = nearbyChunk.Modifier.HasFlag(ChunkModifier.Discount25)
                            || nearbyChunk.Modifier.HasFlag(ChunkModifier.Discount50)
                            || nearbyChunk.Modifier.HasFlag(ChunkModifier.PriceIncrease25)
                            || nearbyChunk.Modifier.HasFlag(ChunkModifier.PriceIncrease50);

                        var iconCount = (nearbyChunk.Modifier.HasFlag(ChunkModifier.FreeDice) ? 1 : 0)
                            + (hasDiscountTag ? 1 : 0)
                            + (nearbyChunk.Modifier.HasFlag(ChunkModifier.Mystery) ? 1 : 0)
                            + 1;

                        var iconX = (iconCount * 32) * -0.5f + 16;

                        // lock icon
                        Main.spriteBatch.Draw(ModAssets.StatusIconTex, specialIndicatorPos + new Vector2(iconX, 0), ModAssets.StatusIconTex_Locked,
                            color, 0, new Vector2(16), 1, 0, 0 );
                        iconX += 32;

                        if (hasDiscountTag) {
                            // discount icon
                            Main.spriteBatch.Draw(ModAssets.StatusIconTex, specialIndicatorPos + new Vector2(iconX, 0), ModAssets.StatusIconTex_Discounted,
                                color, 0, new Vector2(16), 1, 0, 0);
                            iconX += 32;
                        }
                        
                        if (nearbyChunk.Modifier.HasFlag(ChunkModifier.FreeDice)) {
                            // dice icon
                            Main.spriteBatch.Draw(ModAssets.StatusIconTex, specialIndicatorPos + new Vector2(iconX, 0), ModAssets.StatusIconTex_Dicey,
                                color, 0, new Vector2(16), 1, 0, 0);
                            iconX += 32;
                        }

                        if (nearbyChunk.Modifier.HasFlag(ChunkModifier.Mystery)) {
                            // mystery icon
                            Main.spriteBatch.Draw(ModAssets.StatusIconTex, specialIndicatorPos + new Vector2(iconX, 0), ModAssets.StatusIconTex_Mystery,
                                color, 0, new Vector2(16), 1, 0, 0);
                            iconX += 32;
                        }
                    }

                    if (isHoveringChunk && canRerollChunk && playerInRangeOfUnlock) {
                        if (Main.mouseRight && Main.mouseRightRelease && Main.hasFocus) {
                            nearbyChunk.Reroll();

                            // TODO: sync
                            gridWorld.RerollCount--;

                            _rerollShake = 60f;

                            SoundEngine.PlaySound(SoundID.Item101);

                            for (var i = 0; i < 15; i++) {
                                var dir = (MathHelper.TwoPi / 8 * i).ToRotationVector2().RotatedByRandom(0.5);
                                Animations.Active.Add(new RerollItemAnimation {
                                    duration = Main.rand.Next(45, 90),
                                    velocity = dir * Main.rand.NextFloat(1, 12) * new Vector2(1f, 0.5f),
                                    gravity = Main.rand.NextFloat(0.9f, 0.96f),
                                    rotation = Main.rand.NextFloat(6.28f),
                                    position = itemIconPos + dir * 32,
                                    oldItemType = nearbyChunk.UnlockCost.type,
                                    hidden = eventHideItem
                                });
                            }

                            Animations.Active.Add(new GlowAnimation {
                                Duration = 30f,
                                Position = itemIconPos,
                                FadeIn = 0.15f,
                                Color = Color.White with { A = 0 } * 0.5f,
                                Scale = 1.5f
                            });
                        }
                    }

                    if (canUnlockChunk && isHoveringChunk) {
                        if (_waitForMouseDownBeforeUnlocking && Main.mouseLeftRelease)
                            _waitForMouseDownBeforeUnlocking = false;

                        if (!_waitForMouseDownBeforeUnlocking && Main.mouseLeft && Main.hasFocus) {
                            _holdDuration += 1f;
                            if (_holdDuration < UnlockHoldDuration - 20 && (int)_holdDuration % 5 == 0) {
                                SoundEngine.PlaySound(SoundID.Item1);
                                Animations.Active.Add(new UnlockItemFlowAnimation {
                                    item = nearbyChunk.UnlockCost,
                                    origin = Main.LocalPlayer.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 16),
                                    target = itemIconPos + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 16),
                                    rotation = Main.rand.NextFloat(6.28f),
                                    rotationSpeed = Main.rand.NextFloat(-0.5f, 0.5f),
                                    arcAmount = Main.rand.NextFloat(-1f, 1f),
                                    hidden = eventHideItem
                                });
                            }
                        } else _holdDuration = 0;

                        if (_holdDuration >= UnlockHoldDuration) {
                            nearbyChunk.Unlock(Main.LocalPlayer);
                            currentlyHoveredChunk = null;
                            _holdDuration = 0;

                            Animations.Active.Add(new UnlockAnimation {
                                Chunk = nearbyChunk
                            });

                            if (eventHideItem) {
                                Main.LocalPlayer.ClearBuff(ModContent.BuffType<MysteryBuff>());
                            } else if (eventDiscount) {
                                if (nearbyChunk.Group == CostGroup.PaidReward)
                                    Main.LocalPlayer.ClearBuff(ModContent.BuffType<FomoBuff>());

                                Main.LocalPlayer.ClearBuff(ModContent.BuffType<CostIncreaseBuff>());
                                Main.LocalPlayer.ClearBuff(ModContent.BuffType<SaleBuff>());
                            }
                        }
                    }

                    if (showStackText) {
                        var costOffsetX = 0;
                        var costStack = nearbyChunk.UnlockCost.stack;

                        if (eventDiscount) {
                            var fakePricePos = itemIconPos + new Vector2(-16, 42 * (isHoveringChunk ? 1.05f : 1)) - Main.screenPosition;
                            var fakePriceText = $"x{costStack}";
                            var fakePriceSize = FontAssets.MouseText.Value.MeasureString(fakePriceText);

                            Utils.DrawBorderString(Main.spriteBatch, fakePriceText,
                                fakePricePos, 
                                eventNegativeDiscount ? Color.Green : Color.Red, canUnlockChunk && isHoveringChunk ? 0.9f : 0.85f, 0.5f);

                            var dashLength = MathF.Sqrt(fakePriceSize.X * fakePriceSize.X + fakePriceSize.Y * fakePriceSize.Y) * 0.35f;

                            Main.spriteBatch.Draw(
                                ModAssets.PixelTex,
                                fakePricePos + new Vector2(0, fakePriceSize.Y * 0.5f - 1),
                                null,
                                Color.Black * 0.5f,
                                -MathHelper.PiOver4,
                                ModAssets.PixelTex.Size() * 0.5f,
                                new Vector2(dashLength + 1, 3),
                                0,
                                0
                            );

                            Main.spriteBatch.Draw(
                                ModAssets.PixelTex,
                                fakePricePos + new Vector2(0, fakePriceSize.Y * 0.5f - 1),
                                null,
                                Color.Red,
                                -MathHelper.PiOver4,
                                ModAssets.PixelTex.Size() * 0.5f,
                                new Vector2(dashLength, 1.5f),
                                0,
                                0
                            );

                            costOffsetX += 16;
                        }

                        Utils.DrawBorderString(Main.spriteBatch, $"x{stack}{(eventDiscount ? "!" : "")}",
                            itemIconPos + new Vector2(costOffsetX, 42 * (isHoveringChunk ? 1.05f : 1)) - Main.screenPosition, textColor,
                            (canUnlockChunk && isHoveringChunk ? 0.9f : 0.85f) 
                            * (eventDiscount ? 1.2f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 8) : 1), 0.5f);
                    }
                }
            }
        }

        if (currentlyHoveredChunk == null || currentlyHoveredChunk != _lastHoveredChunk) {
            _waitForMouseDownBeforeUnlocking = true;
            _holdDuration = 0;
        }

        Animations.Draw();
    }

    public void Update() {
        Animations.Update();
    }
}
