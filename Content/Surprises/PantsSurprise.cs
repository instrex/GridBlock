using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;

namespace GridBlock.Content.Surprises;

public class PantsSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.armor[2].IsAir;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var clone = player.armor[2].Clone();
        player.armor[2].TurnToAir();

        var k = Item.NewItem(player.GetSource_FromThis(), player.getRect(), clone);
        Main.item[k].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 6);
        Main.item[k].noGrabDelay = 60 * 2;
    }
}
