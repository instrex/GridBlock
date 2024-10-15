using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class PlayerBuffSpeedSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.HasBuff(BuffID.Swiftness) && !player.HasBuff(BuffID.SugarRush) && !player.HasBuff(BuffID.Panic);
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.AddBuff(BuffID.Swiftness, 60 * 30);
        player.AddBuff(BuffID.SugarRush, 60 * 30);
        player.AddBuff(BuffID.Panic, 60 * 30);
    }
}
