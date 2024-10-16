using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class HallowHordeSurprise : GridBlockSurprise.ProjectileSpawner<HallowHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 8f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && player.ZoneHallow && chunk.ContentAnalysis.SuitableForHordeEvents;
    }
}

public class HallowHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.25f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.Item9 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.RainbowRod, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        if (CurrentNpcIndex == 0 && Main.rand.NextFloat() < 0.05f) {
            return NPCID.BigMimicHallow;
        }

        var rng = new WeightedRandom<int>();

        rng.Add(NPCID.IlluminantSlime);
        rng.Add(NPCID.IlluminantBat);
        rng.Add(NPCID.ChaosElemental);
        rng.Add(NPCID.EnchantedSword);

        rng.Add(NPCID.Pixie);
        rng.Add(NPCID.Unicorn);
        rng.Add(NPCID.RainbowSlime);
        rng.Add(NPCID.LightMummy, 1);

        if (!Main.dayTime) rng.Add(NPCID.Gastropod);

        return rng.Get();
    }
}