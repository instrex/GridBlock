using GridBlock.Common;
using GridBlock.Common.Surprises;
using System.Linq;
using Terraria;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class SomeoneDiesSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.npc.Any(n => n.active && n.townNPC);
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var rng = new WeightedRandom<NPC>();
        foreach (var npc in Main.npc.Where(n => n.active && n.townNPC))
            rng.Add(npc);

        // KILL
        rng.Get().StrikeInstantKill();
    }
}