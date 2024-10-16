using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

public class MartianSurprise : GridBlockSurprise.ProjectileSpawner<MartianSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && chunk.ContentAnalysis.SuitableForHordeEvents;
    }
}

public class MartianSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 6) * (Main.rand.NextFloat() < 0.25f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.BlueSlime;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.Item16 with { PitchVariance = 1f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        rng.Add(NPCID.Scutlix);
        rng.Add(NPCID.MartianWalker);
        rng.Add(NPCID.MartianDrone);
        rng.Add(NPCID.MartianTurret);
        rng.Add(NPCID.GigaZapper);
        rng.Add(NPCID.MartianEngineer);
        rng.Add(NPCID.MartianOfficer);
        rng.Add(NPCID.RayGunner);
        rng.Add(NPCID.GrayGrunt);
        rng.Add(NPCID.BrainScrambler);

        return rng.Get();
    }
}