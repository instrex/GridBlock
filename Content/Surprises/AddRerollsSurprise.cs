using GridBlock.Common;
using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Content.Surprises;

internal class AddRerollsSurprise : GridBlockSurprise {
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 1.0f;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        GridBlockWorld.Instance.RerollCount += 2;
    }
}
