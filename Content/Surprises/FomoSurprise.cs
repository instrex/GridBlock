using GridBlock.Common;
using GridBlock.Common.Surprises;
using GridBlock.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Content.Surprises;

public class FomoSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.HasBuff<FomoBuff>();
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.AddBuff(ModContent.BuffType<FomoBuff>(), 60 * 30);
    }
}

