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

    /// <summary>
    /// History of all the surprises this player encountered.
    /// </summary>
    public List<GridBlockSurprise> SurpriseHistory { get; private set; } = [];

    int _stuckTimer;

    /// <summary>
    /// Adds surprise into the buffer, additionally clearing it.
    /// </summary>
    public void PushSurprise(GridBlockSurprise surprise) {
        SurpriseHistory.Insert(0, surprise);
        if (SurpriseHistory.Count > 5) {
            SurpriseHistory.RemoveAt(5);
        }
    }

    public override void SaveData(TagCompound tag) {
        if (RichChunkRewards.Count > 0) tag[nameof(RichChunkRewards)] = RichChunkRewards.ToList();
        if (SurpriseHistory.Count > 0) tag[nameof(SurpriseHistory)] = SurpriseHistory.Select(s => s.Id).ToList();
    }

    public override void LoadData(TagCompound tag) {
        RichChunkRewards = new(tag.TryGet<List<Item>>("RichChunkRewards", out var rewards) ? rewards : []);
        if (tag.TryGet<List<string>>(nameof(SurpriseHistory), out var history)) {
            var surprises = ModContent.GetContent<GridBlockSurprise>().ToList();
            foreach (var entry in history) {
                var instance = surprises.Find(s => s.Id == entry);
                if (instance is null) {
                    Mod.Logger.Warn($"Couldn't load entry from surprise history! ({entry})");
                    continue;
                }

                SurpriseHistory.Add(instance);
            }
        }
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

    public override void UpdateEquips() {
        Player.pickSpeed *= 0.76f;
    }

    GridBlockChunk[,] _adjChunks = new GridBlockChunk[3, 3];
    GridBlockChunk _lastChunk;
    int _adjUpdateTimer;

    public override void PreUpdateMovement() {
        CheckCollision();
    }

    void CheckCollision() {
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

            _adjUpdateTimer = 5;
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


            //// find distances to the closest edges
            //int[] paths = [
            //    top?.IsUnlocked == true     ? (int)Player.Top.Y - top.WorldBounds.Bottom    : int.MaxValue,
            //    right?.IsUnlocked == true   ? right.WorldBounds.Left - (int)Player.Right.X  : int.MaxValue,
            //    bottom?.IsUnlocked == true  ? bottom.WorldBounds.Top - (int)Player.Bottom.Y : int.MaxValue,
            //    left?.IsUnlocked == true    ? (int)Player.Left.X - left.WorldBounds.Right   : int.MaxValue,
            //];

            //var closestIndex = 0;
            //var closestDist = int.MaxValue;

            //// determine the closest edge
            //for (var i = 0; i < 4; i++) {
            //    if (paths[i] < closestDist) {
            //        closestDist = paths[i];
            //        closestIndex = i;
            //    }
            //}

            //// adjust player pos accordingly
            //switch (closestIndex) {
            //    case 0:
            //        Player.Bottom = Player.Bottom with { Y = top.WorldBounds.Bottom };
            //        break;

            //    case 2:
            //        Player.Top = Player.Top with { Y = bottom.WorldBounds.Bottom };
            //        break;

            //    case 1:
            //        Player.Left = Player.Left with { X = right.WorldBounds.Left };
            //        break;

            //    case 3:
            //        Player.Right = Player.Right with { X = left.WorldBounds.Right };
            //        break;
            //}

            //Main.NewText($"OUTCOCK {closestIndex}");

            //// force update adjChunks
            //_adjUpdateTimer = 0;
        } else _stuckTimer = 0;

        _lastChunk = current;


        if (collisionOccured) {
            // Main.NewText($"Cock check occured: {_adjUpdateTimer}ms");
            Player.RemoveAllGrapplingHooks();
        }
    }
}
