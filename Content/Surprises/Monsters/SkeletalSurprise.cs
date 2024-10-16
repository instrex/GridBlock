using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

public class SkeletalSurprise : GridBlockSurprise.ProjectileSpawner<SkeletalSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return player.ZoneDungeon ? 10f : 5f;
    }
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
            rng.Add(NPCID.ArmoredSkeleton, 4);
            rng.Add(NPCID.SkeletonArcher, 4);

            rng.Add(NPCID.BlueArmoredBones, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.BlueArmoredBonesMace, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.BlueArmoredBonesNoPants, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.BlueArmoredBonesSword, (NPC.downedPlantBoss ? 4 : 0.5) / 4);

            rng.Add(NPCID.RustyArmoredBonesAxe, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.RustyArmoredBonesSword, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.RustyArmoredBonesSwordNoArmor, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.RustyArmoredBonesFlail, (NPC.downedPlantBoss ? 4 : 0.5) / 4);

            rng.Add(NPCID.HellArmoredBones, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.HellArmoredBonesMace, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.HellArmoredBonesSpikeShield, (NPC.downedPlantBoss ? 4 : 0.5) / 4);
            rng.Add(NPCID.HellArmoredBonesSword, (NPC.downedPlantBoss ? 4 : 0.5) / 4);

            if (NPC.downedPlantBoss) {
                rng.Add(NPCID.Paladin, 1);
                rng.Add(NPCID.Necromancer, 0.5);
                rng.Add(NPCID.NecromancerArmored, 0.5);
                rng.Add(NPCID.RaggedCaster, 0.5);
                rng.Add(NPCID.RaggedCasterOpenCoat, 0.5);
                rng.Add(NPCID.DiabolistRed, 0.5);
                rng.Add(NPCID.DiabolistWhite, 0.5);
                rng.Add(NPCID.SkeletonCommando, 1);
                rng.Add(NPCID.SkeletonSniper, 1);
                rng.Add(NPCID.TacticalSkeleton, 1);
                rng.Add(NPCID.GiantCursedSkull, 1);
                rng.Add(NPCID.BoneLee, 1);
                rng.Add(NPCID.DungeonSpirit, 1);
            }

        } else {
            rng.Add(NPCID.Skeleton, 1.0 / 12);
            rng.Add(NPCID.SmallSkeleton, 1.0 / 12);
            rng.Add(NPCID.BigSkeleton, 1.0 / 12);
            rng.Add(NPCID.HeadacheSkeleton, 1.0 / 12);
            rng.Add(NPCID.SmallHeadacheSkeleton, 1.0 / 12);
            rng.Add(NPCID.BigHeadacheSkeleton, 1.0 / 12);
            rng.Add(NPCID.MisassembledSkeleton, 1.0 / 12);
            rng.Add(NPCID.SmallMisassembledSkeleton, 1.0 / 12);
            rng.Add(NPCID.BigMisassembledSkeleton, 1.0 / 12);
            rng.Add(NPCID.PantlessSkeleton, 1.0 / 12);
            rng.Add(NPCID.SmallPantlessSkeleton, 1.0 / 12);
            rng.Add(NPCID.BigPantlessSkeleton, 1.0 / 12);

            if (NPC.downedBoss3) {
                rng.Add(NPCID.AngryBones, 4);
                rng.Add(NPCID.DarkCaster, 4);
                rng.Add(NPCID.CursedSkull, 4);
            }
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

