using GridBlock.Common;
using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Content.Surprises;

internal class DoubleLockSurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        chunk.IsUnlocked = false;
        chunk.Reroll();
    }
}
