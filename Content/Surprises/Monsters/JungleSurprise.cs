using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class JungleSurprise : GridBlockSurprise.ProjectileSpawner<JungleSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 8f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.SuitableForHordeEvents && player.ZoneJungle;
    }
}

public class JungleSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.JungleSlime;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-1, -4);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Cloud, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        if (Main.hardMode) {
            rng.Add(NPCID.JungleCreeper, 10);
            rng.Add(NPCID.MossHornet, 10);
            rng.Add(NPCID.AngryTrapper, 10);
            rng.Add(NPCID.Arapaima, 10);
            rng.Add(NPCID.GiantTortoise, 10);
            rng.Add(NPCID.Moth, 0.5);
        }

        if (!Main.dayTime) rng.Add(NPCID.DoctorBones, 0.015);

        rng.Add(NPCID.JungleSlime);
        rng.Add(NPCID.JungleBat);
        rng.Add(NPCID.ManEater);
        rng.Add(NPCID.Piranha);
        rng.Add(NPCID.SpikedJungleSlime);
        rng.Add(NPCID.Hornet, 0.1);
        rng.Add(NPCID.HornetFatty, 0.1);
        rng.Add(NPCID.HornetLeafy, 0.1);
        rng.Add(NPCID.HornetHoney, 0.1);
        rng.Add(NPCID.HornetSpikey, 0.1);
        rng.Add(NPCID.HornetStingy, 0.1);
        rng.Add(NPCID.BigHornetFatty, 0.1);
        rng.Add(NPCID.BigHornetLeafy, 0.1);
        rng.Add(NPCID.BigHornetHoney, 0.1);
        rng.Add(NPCID.BigHornetSpikey, 0.1);
        rng.Add(NPCID.BigHornetStingy, 0.1);

        return rng.Get();
    }
}