using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class DesertHordeSurprise : GridBlockSurprise.ProjectileSpawner<DesertHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && player.ZoneDesert && chunk.ContentAnalysis.SuitableForHordeEvents;
    }
}

public class DesertHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.25f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Sand, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        rng.Add(NPCID.Vulture);
        rng.Add(NPCID.Antlion);
        rng.Add(NPCID.WalkingAntlion);
        rng.Add(NPCID.FlyingAntlion);
        rng.Add(NPCID.GiantFlyingAntlion);
        rng.Add(NPCID.GiantWalkingAntlion);
        rng.Add(NPCID.SandSlime);

        if (Main.hardMode && (Projectile.position.Y > Main.worldSurface || Main.raining)) {
            rng.Add(532);
            rng.Add(NPCID.DesertScorpionWalk);
            rng.Add(NPCID.DesertLamiaDark);
            rng.Add(NPCID.DesertLamiaLight);
            rng.Add(NPCID.DesertGhoul);
            rng.Add(NPCID.DuneSplicerHead);
        }

        if (Main.hardMode) {
            rng.Add(NPCID.Mummy);
        }

        return rng.Get();
    }
}