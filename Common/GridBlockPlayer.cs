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
    int _adjUpdateTimer;

    public override void PreUpdateMovement() {
        // HorizontalCollisionCheck();
        // VerticalCollisionCheck();

        CheckCollision();
    }

    void CheckCollision() {
        if (GridBlockWorld.Instance.Chunks is not GridMap2D<GridBlockChunk> chunks)
            return;

        // update adj chunks first
        if (_adjUpdateTimer-- <= 0 || Player.oldPosition.Distance(Player.position) > 120) {
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
                Player.Top = Player.Top with { Y = top.WorldBounds.Bottom + 2 };
                Player.gfxOffY = 0;
                collisionOccured = true;
            }
        }

        var bottom = _adjChunks[1, 2];
        if (bottom != null && !bottom.IsUnlocked) {
            if (Player.Bottom.Y > bottom.WorldBounds.Top) {
                Player.Bottom = Player.Bottom with { Y = bottom.WorldBounds.Top + 2 };
                Player.velocity.Y = Player.justJumped ? Player.velocity.Y : 0;
                Player.gfxOffY = 0;
                collisionOccured = true;
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

        // when stuck in a chunk somehow
        var current = _adjChunks[1, 1];
        if (current != null && !current.IsUnlocked) {
            Player.Hurt(new() { Damage = 5, DamageSource = PlayerDeathReason.LegacyDefault() });
        }

        if (collisionOccured) Player.RemoveAllGrapplingHooks();
    }
}
