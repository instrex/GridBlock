using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class DungeonGuardianSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) => 0.1f;
    public override void Trigger(Player player, GridBlockChunk chunk) {
        NPC.NewNPC(player.GetSource_FromThis(), chunk.WorldBounds.Center.X, chunk.WorldBounds.Center.Y, NPCID.DungeonGuardian);
    }
}
