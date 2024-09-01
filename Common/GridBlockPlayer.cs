using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common;

public class GridBlockPlayer : ModPlayer {
    public override void PreUpdateMovement() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        if (gridWorld.Chunks is null)
            return;

        var playerRect = Player.getRect();
        var playerPos = new Vector2(Player.velocity.X > 0 ? playerRect.Right : playerRect.Left,
            Player.velocity.Y > 0 ? playerRect.Bottom : playerRect.Top);

        var currentChunkCoord = new Point((int)playerPos.X / 16 / gridWorld.Chunks.ChunkSize,
            (int)playerPos.Y / 16 / gridWorld.Chunks.ChunkSize);

        if (gridWorld.Chunks.GetByChunkCoord(currentChunkCoord) is not GridBlockChunk chunk || chunk.IsUnlocked)
            return;

        var tileBounds = new Rectangle(
            currentChunkCoord.X * gridWorld.Chunks.ChunkSize,
            currentChunkCoord.Y * gridWorld.Chunks.ChunkSize,
            gridWorld.Chunks.ChunkSize, gridWorld.Chunks.ChunkSize
        );

        var worldBounds = new Rectangle(tileBounds.X * 16, tileBounds.Y * 16, tileBounds.Width * 16, tileBounds.Height * 16);

        // push out players vertically
        if (worldBounds.Intersects(playerRect)) {
            var worldY = tileBounds.Center.Y * 16 + 8;
            var pushDir = Player.position.Y > worldY ? 1 : -1;
            Player.position.Y = pushDir == 1 ? tileBounds.Bottom * 16 - 4 : tileBounds.Top * 16 - Player.height + 2;
            Player.velocity.Y = pushDir == 1 ? Player.velocity.Y : 0;
            Player.gfxOffY = 0;
            Main.NewText($"PASHOL VON Y {Player.velocity} {Player.gfxOffY}");
        }
    }
}
