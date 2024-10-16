using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class IceHordeSurprise : GridBlockSurprise.ProjectileSpawner<IceHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 8f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.SuitableForHordeEvents && player.ZoneSnow;
    }
}

public class IceHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(4, 8) * (Main.rand.NextFloat() < 0.25f ? 2 : 1)) * SpawnInterval;
        NpcType = NPCID.IceSlime;
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

        rng.Add(NPCID.IceSlime);
        if (!Main.dayTime) rng.Add(NPCID.ZombieEskimo);

        if (Projectile.position.Y > Main.rockLayer) {
            rng.Add(NPCID.SpikedIceSlime);
            rng.Add(NPCID.SnowFlinx, 0.25);
            rng.Add(NPCID.UndeadViking);
            rng.Add(NPCID.UndeadMiner);
        }

        if (Main.hardMode) {
            if (Projectile.position.Y > Main.rockLayer) rng.Add(NPCID.ArmoredViking, 10);
            rng.Add(NPCID.IceTortoise, 10);
            rng.Add(NPCID.IceElemental, 10);
            rng.Add(NPCID.Wolf, 10);
            rng.Add(NPCID.IcyMerman, 10);
            rng.Add(NPCID.IceMimic, 0.25);
        }

        return rng.Get();
    }
}