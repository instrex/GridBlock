using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class IceHordeSurprise : GridBlockSurprise.ProjectileSpawner<IceHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return player.ZoneSnow;
    }
}

public class IceHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
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
            if (Projectile.position.Y > Main.rockLayer) rng.Add(NPCID.ArmoredViking);
            rng.Add(NPCID.IceTortoise);
            rng.Add(NPCID.IceElemental);
            rng.Add(NPCID.Wolf);
            rng.Add(NPCID.IcyMerman);
            rng.Add(NPCID.IceMimic, 0.25);
            rng.Add(NPCID.IceGolem, 0.1);
        }

        return rng.Get();
    }
}