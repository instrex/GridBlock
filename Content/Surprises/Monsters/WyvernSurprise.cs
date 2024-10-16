using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class WyvernSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 2f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return player.ZoneSkyHeight && Main.hardMode;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var npc = NPC.NewNPCDirect(player.GetSource_FromThis(), chunk.WorldBounds.Center.ToVector2(), NPCID.WyvernHead);
        npc.netUpdate = true;
    }
}