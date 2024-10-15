using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class TravellingMerchantSurprise : GridBlockSurprise {
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.15f;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        for (var i = 0; i < Main.maxNPCs; i++) {
            var npc = Main.npc[i];
            if (npc.active && npc.type == NPCID.TravellingMerchant) {
                npc.StrikeInstantKill();
            }
        }

        NPC.NewNPC(player.GetSource_FromThis(), (int)player.position.X, (int)player.position.Y, NPCID.TravellingMerchant);
    }
}
