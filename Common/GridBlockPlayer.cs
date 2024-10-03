using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace GridBlock.Common;

public class GridBlockPlayer : ModPlayer {

    /// <summary>
    /// To avoid duplication when giving out rewards.
    /// </summary>
    public HashSet<Item> RichChunkRewards { get; private set; } = [];

    readonly GridBlockChunk[,] _adjChunks = new GridBlockChunk[3, 3];
    GridBlockChunk _lastChunk;

    int _stuckTimer;

    Vector2 _randomTripPortalTrigger, _randomTripPortalDestination;
    bool _hasRandomTripPortalInfo;
    int _randomTripPortalTimer = -1;


    public void PrepareRandomTripPortalTrigger(Vector2 randomTripPosition, Vector2 originPosition) {
        _randomTripPortalTrigger = randomTripPosition;
        _randomTripPortalDestination = originPosition;
        _hasRandomTripPortalInfo = true;
    }

    public void TryCreateRandomTripPortalReturn(Vector2 newPos) {
        if (!_hasRandomTripPortalInfo || _randomTripPortalTrigger != (newPos + new Vector2(10, 42)))
            return;

        _randomTripPortalTimer = 10;
        _hasRandomTripPortalInfo = false;
    }

    public override void SaveData(TagCompound tag) {
        if (RichChunkRewards.Count > 0) tag[nameof(RichChunkRewards)] = RichChunkRewards.ToList();
    }

    public override void LoadData(TagCompound tag) {
        RichChunkRewards = new(tag.TryGet<List<Item>>("RichChunkRewards", out var rewards) ? rewards : []);
    }

    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) {
        yield return new(ItemID.Rope, 999);
    }

    public override bool CanUseItem(Item item) {
        // allow usage of RoD and pickaxes only inside unlocked chunks
        if (ModContent.GetInstance<GridBlockWorld>().Chunks?.GetByWorldPos(Main.MouseWorld) is GridBlockChunk chunk &&
            (item.type is ItemID.RodofDiscord or ItemID.RodOfHarmony || (item.pick > 0 && !Main.SmartCursorShowing) || item.createTile != -1)) {
            return chunk.IsUnlocked;
        }

        return true;
    }

    // increase mining speed passively
    public override void UpdateEquips() {
        Player.pickSpeed *= 0.76f;

        if (_randomTripPortalTimer > 0) {
            _randomTripPortalTimer--;
            if (_randomTripPortalTimer <= 0) {
                Player.PotionOfReturnOriginalUsePosition = _randomTripPortalDestination;
                Player.PotionOfReturnHomePosition = _randomTripPortalTrigger;
                NetMessage.SendData(MessageID.PlayerControls, -1, Player.whoAmI, null, Player.whoAmI, 0f, 0f, 0f, 0, 0, 0);
            }
        }
    }

    public override void PreUpdateMovement() {
        if (GridBlockWorld.Instance.Chunks is not GridMap2D<GridBlockChunk> chunks)
            return;

        var current = chunks.GetByWorldPos(Player.Center);

        // update adj chunks first
        if (_lastChunk != current && current.IsUnlocked /*(_adjUpdateTimer-- <= 0 || Player.oldPosition.Distance(Player.position) > 120)*/) {
            var center = chunks.GetByWorldPos(Player.Center).ChunkCoord;
            for (var i = 0; i < 3; i++) {
                for (var k = 0; k < 3; k++) {
                    _adjChunks[i, k] = chunks.GetByChunkCoord(center + new Point(i - 1, k - 1));
                }
            }
        }

        var collisionOccured = false;

        var top = _adjChunks[1, 0];
        if (top != null && !top.IsUnlocked) {
            if (Player.Top.Y < top.WorldBounds.Bottom) {
                if (Player.gravDir > 0) {
                    Player.Top = Player.Top with { Y = top.WorldBounds.Bottom + 2 };
                    Player.gfxOffY = 0;
                } else {
                    Player.Top = Player.Top with { Y = top.WorldBounds.Bottom - 2 };
                    Player.velocity.Y = Player.justJumped ? Player.velocity.Y : 0;
                    Player.gfxOffY = 0;
                }

                collisionOccured = true;
            }
        }

        var bottom = _adjChunks[1, 2];
        if (bottom != null && !bottom.IsUnlocked) {
            if (Player.Bottom.Y > bottom.WorldBounds.Top) {
                if (Player.gravDir > 0) {
                    Player.Bottom = Player.Bottom with { Y = bottom.WorldBounds.Top + 2 };
                    Player.velocity.Y = Player.justJumped ? Player.velocity.Y : 0;
                    Player.gfxOffY = 0;
                } else {
                    Player.Bottom = Player.Bottom with { Y = bottom.WorldBounds.Top + 2 };
                    Player.gfxOffY = 0;
                }
            }
        }

        var left = _adjChunks[0, 1];
        if (left != null && !left.IsUnlocked) {
            if (Player.Left.X < left.WorldBounds.Right) {
                Player.Left = Player.Left with { X = left.WorldBounds.Right };
                Player.velocity.X *= -1;
                collisionOccured = true;

                if (MathF.Abs(Player.velocity.X) > 2f)
                    SoundEngine.PlaySound(SoundID.Item56 with { PitchVariance = 1 }, Player.Center);
            }
        }

        var right = _adjChunks[2, 1];
        if (right != null && !right.IsUnlocked) {
            if (Player.Right.X > right.WorldBounds.Left) {
                Player.Right = Player.Right with { X = right.WorldBounds.Left };
                Player.velocity.X *= -1;
                collisionOccured = true;

                if (MathF.Abs(Player.velocity.X) > 2f)
                    SoundEngine.PlaySound(SoundID.Item56 with { PitchVariance = 1 }, Player.Center);
            }
        }

        // corners
        var cornerCheckInset = 4;

        var topLeft = _adjChunks[0, 0];
        if (topLeft != null && !topLeft.IsUnlocked) {
            var btmRightCorner = topLeft.WorldBounds.BottomRight();
            if (Player.position.X < btmRightCorner.X - cornerCheckInset && Player.position.Y < btmRightCorner.Y - cornerCheckInset) {
                Player.position = btmRightCorner;
                // Player.velocity = Vector2.Reflect(Player.velocity * 0.5f, MathHelper.PiOver4.ToRotationVector2());
                collisionOccured = true;
            }
        }

        var topRight = _adjChunks[2, 0];
        if (topRight != null && !topRight.IsUnlocked) {
            var btmLeftCorner = topRight.WorldBounds.BottomLeft();
            if (Player.Right.X > btmLeftCorner.X + cornerCheckInset && Player.position.Y < btmLeftCorner.Y - cornerCheckInset) {
                Player.position = btmLeftCorner + new Vector2(-Player.width, 0);
                // Player.velocity = Vector2.Reflect(Player.velocity * 0.5f, (MathHelper.PiOver2 + MathHelper.PiOver4).ToRotationVector2());
                collisionOccured = true;
            }
        }

        var bottomLeft = _adjChunks[0, 2];
        if (bottomLeft != null && !bottomLeft.IsUnlocked) {
            var topRightCorner = bottomLeft.WorldBounds.TopRight();
            if (Player.Left.X < topRightCorner.X - cornerCheckInset && Player.Bottom.Y > topRightCorner.Y + cornerCheckInset) {
                Player.position = topRightCorner + new Vector2(0, -Player.height);
                Player.velocity = Vector2.Reflect(Player.velocity * 0.5f, (-MathHelper.PiOver4).ToRotationVector2());
                collisionOccured = true;
            }
        }

        var bottomRight = _adjChunks[2, 2];
        if (bottomRight != null && !bottomRight.IsUnlocked) {
            var topLeftCorner = bottomRight.WorldBounds.TopLeft();
            if (Player.Right.X > topLeftCorner.X + cornerCheckInset && Player.Bottom.Y > topLeftCorner.Y + cornerCheckInset) {
                Player.position = topLeftCorner + new Vector2(-Player.width, -Player.height);
                Player.velocity = Vector2.Reflect(Player.velocity * 0.5f, (-MathHelper.PiOver2 - MathHelper.PiOver4).ToRotationVector2());
                collisionOccured = true;
            }
        }

        // when stuck in a chunk somehow
        if (current != null && !current.IsUnlocked) {
            if (_stuckTimer++ >= 2) {

                // fiasco
                Player.Hurt(new() { Damage = 5, DamageSource = PlayerDeathReason.LegacyDefault() });
                Player.RemoveAllGrapplingHooks();
            }

        } else _stuckTimer = 0;

        _lastChunk = current;

        if (collisionOccured) {
            // Main.NewText($"Cock check occured: {_adjUpdateTimer}ms");
            Player.RemoveAllGrapplingHooks();
            if (Player.HasBuff(BuffID.TheTongue)) {
                Player.Hurt(new() { Damage = 5, DamageSource = PlayerDeathReason.LegacyDefault() });
            }
        }
    }
}
