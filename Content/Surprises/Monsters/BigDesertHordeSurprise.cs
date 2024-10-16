using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class BigDesertHordeSurprise : GridBlockSurprise.ProjectileSpawner<BigDesertHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 2f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && !Main.raining && player.ZoneDesert && chunk.ContentAnalysis.SuitableForHordeEvents;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        Main.raining = true;
        Main.maxRaining = 0.8f;
        Main.maxRain = 140;
        Main.rainTime = 18000;

        base.Trigger(player, chunk);
    }
}

public class BigDesertHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(4, 6) * (Main.rand.NextFloat() < 0.25f ? 2 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override int GetNpcType() {
        if (CurrentNpcIndex == 3)
            return NPCID.SandElemental;

        return base.GetNpcType();
    }
}