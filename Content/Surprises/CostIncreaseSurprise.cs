using GridBlock.Common;
using GridBlock.Common.Surprises;
using GridBlock.Content.Buffs;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Content.Surprises;

public class CostIncreaseSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.HasBuff<CostIncreaseBuff>() && !player.HasBuff<SaleBuff>();
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.AddBuff(ModContent.BuffType<CostIncreaseBuff>(), 60);
    }
}

