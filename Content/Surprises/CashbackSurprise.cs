using GridBlock.Common;
using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Content.Surprises;

internal class CashbackSurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        player.QuickSpawnItem(player.GetSource_FromThis(), chunk.UnlockCost, Math.Max(1, chunk.UnlockCost.stack / 2));
    }
}
