using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class RainbowDyeSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.FullnessFactor >= 0.5f;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        SoundEngine.PlaySound(SoundID.Item29, player.Center);
        for (var x = 0; x < GridBlockWorld.Instance.Chunks.CellSize; x++) {
            for (var y = 0; y < GridBlockWorld.Instance.Chunks.CellSize; y++) {
                var tileCoord = chunk.TileCoord + new Point(x, y);
                WorldGen.paintTile(tileCoord.X, tileCoord.Y, (byte)Main.rand.Next(30), true);
                WorldGen.paintWall(tileCoord.X, tileCoord.Y, (byte)Main.rand.Next(30), true);
            }
        }
    }
}
