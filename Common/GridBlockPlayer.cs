using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace GridBlock.Common;

public class GridBlockPlayer : ModPlayer {

    /// <summary>
    /// To avoid duplication when giving out rewards.
    /// </summary>
    public List<Item> RichChunkRewards { get; private set; } = [];

    /// <summary>
    /// History of all the surprises this player encountered.
    /// </summary>
    public List<GridBlockSurprise> SurpriseHistory { get; private set; } = [];

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
        if (RichChunkRewards.Count > 0) tag[nameof(RichChunkRewards)] = RichChunkRewards;
        if (SurpriseHistory.Count > 0) tag[nameof(SurpriseHistory)] = SurpriseHistory.Select(s => s.Id).ToList();
    }

    public override void LoadData(TagCompound tag) {
        RichChunkRewards = tag.TryGet<List<Item>>("RichChunkRewards", out var rewards) ? rewards : [];
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

    public override void PreUpdateMovement() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (gridWorld.Chunks is null)
            return;

        HorizontalCollisionCheck();

        var playerRect = Player.getRect();
        var playerPos = new Vector2(Player.velocity.X > 0 ? playerRect.Right : playerRect.Left,
            Player.velocity.Y > 0 ? playerRect.Bottom : playerRect.Top);

        var currentChunkCoord = new Point((int)playerPos.X / 16 / gridWorld.Chunks.CellSize,
            (int)playerPos.Y / 16 / gridWorld.Chunks.CellSize);

        // vertical check
        if (gridWorld.Chunks.GetByChunkCoord(currentChunkCoord) is GridBlockChunk chunk && !chunk.IsUnlocked) {
            var tileBounds = new Rectangle(
                currentChunkCoord.X * gridWorld.Chunks.CellSize,
                currentChunkCoord.Y * gridWorld.Chunks.CellSize,
                gridWorld.Chunks.CellSize, gridWorld.Chunks.CellSize
            );

            var worldBounds = new Rectangle(tileBounds.X * 16, tileBounds.Y * 16, tileBounds.Width * 16, tileBounds.Height * 16);

            // push out players vertically
            if (worldBounds.Intersects(playerRect)) {
                var worldY = tileBounds.Center.Y * 16 + 8;
                var pushDir = Player.position.Y > worldY ? 1 : -1;
                Player.position.Y = pushDir == 1 ? tileBounds.Bottom * 16 - 4 : tileBounds.Top * 16 - Player.height + 2;
                Player.velocity.Y = pushDir == 1 ? Player.velocity.Y : 0;
                Player.gfxOffY = 0;

                // Main.NewText($"PASHOL VON Y {Player.velocity} {Player.gfxOffY}");
            }
        }
    }

    void HorizontalCollisionCheck() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (gridWorld.Chunks is null)
            return;

        var playerRect = Player.getRect();
        var playerPos = new Vector2(Player.velocity.X > 0 ? playerRect.Right : playerRect.Left, playerRect.Center.Y);

        var currentChunkCoord = new Point((int)playerPos.X / 16 / gridWorld.Chunks.CellSize,
            (int)playerPos.Y / 16 / gridWorld.Chunks.CellSize);

        if (gridWorld.Chunks.GetByChunkCoord(currentChunkCoord) is not GridBlockChunk chunk || chunk.IsUnlocked)
            return;

        var tileBounds = new Rectangle(
            currentChunkCoord.X * gridWorld.Chunks.CellSize,
            currentChunkCoord.Y * gridWorld.Chunks.CellSize,
            gridWorld.Chunks.CellSize, gridWorld.Chunks.CellSize
        );

        var worldBounds = new Rectangle(tileBounds.X * 16, tileBounds.Y * 16, tileBounds.Width * 16, tileBounds.Height * 16);

        var xDir = MathF.Sign(worldBounds.Center.X - Player.Center.X);

        if (xDir == 1 && playerRect.Right > worldBounds.Left) {
            Player.position.X = worldBounds.Left - Player.width;
            Player.velocity.X *= -1;

            if (MathF.Abs(Player.velocity.X) > 2f)
                SoundEngine.PlaySound(SoundID.Item56 with { PitchVariance = 1 }, Player.Center);

            // Main.NewText("X RIGHT");
        }


        else if (xDir == -1 && playerRect.Left < worldBounds.Right) {
            Player.position.X = worldBounds.Right;
            Player.velocity.X *= -1;

            if (MathF.Abs(Player.velocity.X) > 2f) 
                SoundEngine.PlaySound(SoundID.Item56 with { PitchVariance = 1 }, Player.Center);
            
            

            // Main.NewText("X LEFT");
        }
    }
}
