using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;

namespace GridBlock.Content.Surprises;

public class PantsSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.5f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.armor[2].IsAir || !player.armor[12].IsAir;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        foreach (var i in new int[] { 2, 12 }) {
            if (player.armor[i].IsAir)
                continue;

            var clone = player.armor[i].Clone();
            player.armor[i].TurnToAir();

            var k = Item.NewItem(player.GetSource_FromThis(), player.getRect(), clone);
            Main.item[k].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 6);
            Main.item[k].noGrabDelay = 60 * 2;
        }
        
    }
}
