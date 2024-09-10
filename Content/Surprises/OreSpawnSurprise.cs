using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;

namespace GridBlock.Content.Surprises;

public class OreSpawnSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) => false; //TODO: implement
    public override void Trigger(Player player, GridBlockChunk chunk) {
        SurpriseProjectile.Spawn<OreSpawnSurpriseProjectile>(player, chunk);
    }
}

public class OreSpawnSurpriseProjectile : SurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();
    }
}