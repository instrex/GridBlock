using GridBlock.Common.Surprises;
using Terraria.ID;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

public class GenderSwapSurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.Male = !player.Male;
        for (var i = 0; i < 32; i++) {
            var dust = Dust.NewDustDirect(player.position, 32, 32, Main.rand.NextBool() ? DustID.PinkCrystalShard : DustID.BlueCrystalShard, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            dust.noGravity = true;
            dust.fadeIn = 1.5f;
        }
    }
}