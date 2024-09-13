using GridBlock.Common;
using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Content.Surprises;

public class TimeChangeNightSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.dayTime && !Main.fastForwardTimeToDusk && chunk.TileCoord.Y < Main.worldSurface;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        Main.fastForwardTimeToDusk = true;
    }
}
