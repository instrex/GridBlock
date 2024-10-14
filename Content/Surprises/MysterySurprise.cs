using GridBlock.Common;
using GridBlock.Common.Surprises;
using GridBlock.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Content.Surprises;

public class MysterySurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !player.HasBuff<MysteryBuff>();
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.AddBuff(ModContent.BuffType<MysteryBuff>(), 60);
    }
}

