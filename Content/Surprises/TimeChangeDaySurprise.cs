using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;

namespace GridBlock.Content.Surprises;

public class TimeChangeDaySurprise : GridBlockSurprise {
    public override float GetWeight(Player player, GridBlockChunk chunk) => 0.25f;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !Main.dayTime && !Main.fastForwardTimeToDawn && chunk.TileCoord.Y < Main.worldSurface;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        Main.fastForwardTimeToDawn = true;
    }
}
