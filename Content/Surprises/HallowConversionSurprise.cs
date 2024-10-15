using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class HallowConversionSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && chunk.ContentAnalysis.FullnessFactor >= 0.5f;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        SoundEngine.PlaySound(SoundID.Item9, player.Center);
        for (var x = 0; x < GridBlockWorld.Instance.Chunks.CellSize; x++) {
            for (var y = 0; y < GridBlockWorld.Instance.Chunks.CellSize; y++) {
                var tileCoord = chunk.TileCoord + new Point(x, y);
                WorldGen.Convert(tileCoord.X, tileCoord.Y, BiomeConversionID.Hallow, 2);
            }
        }
    }
}
