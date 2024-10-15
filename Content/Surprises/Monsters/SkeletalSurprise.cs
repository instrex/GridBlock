using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

public class SkeletalSurprise : GridBlockSurprise.ProjectileSpawner<SkeletalSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.SuitableForHordeEvents && chunk.TileCoord.Y > Main.worldSurface && chunk.TileCoord.Y < Main.UnderworldLayer;
    }
}

public class SkeletalSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit2 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Bone, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        if (Main.hardMode) {
            rng.Add(NPCID.ArmoredSkeleton);
            rng.Add(NPCID.SkeletonArcher);
            rng.Add(NPCID.BlueArmoredBones);
        } else {
            rng.Add(NPCID.Skeleton);
            rng.Add(NPCID.SmallSkeleton);
            rng.Add(NPCID.BigSkeleton);
            rng.Add(NPCID.HeadacheSkeleton);
            rng.Add(NPCID.SmallHeadacheSkeleton);
            rng.Add(NPCID.BigHeadacheSkeleton);
            rng.Add(NPCID.MisassembledSkeleton);
            rng.Add(NPCID.SmallMisassembledSkeleton);
            rng.Add(NPCID.BigMisassembledSkeleton);
            rng.Add(NPCID.PantlessSkeleton);
            rng.Add(NPCID.SmallPantlessSkeleton);
            rng.Add(NPCID.BigPantlessSkeleton);
        }

        rng.Add(NPCID.SkeletonTopHat, 0.1);
        rng.Add(NPCID.SkeletonAstonaut, 0.1);
        rng.Add(NPCID.SkeletonAlien, 0.1);
        rng.Add(NPCID.UndeadMiner, 0.1);
        rng.Add(NPCID.Skeleton, 0.1);

        rng.Add(NPCID.SkeletonMerchant, 0.05);
        rng.Add(NPCID.Tim, 0.05);

        rng.Add(NPCID.DungeonGuardian, 0.001);

        return rng.Get();
    }
}

