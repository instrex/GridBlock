using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class EvilHordeSurprise : GridBlockSurprise.ProjectileSpawner<EvilHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 8f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return Main.hardMode && (player.ZoneCorrupt || player.ZoneCrimson) && chunk.ContentAnalysis.SuitableForHordeEvents;
    }
}

public class EvilHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.25f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.VilePowder, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        if (WorldGen.crimson) {
            if (Main.hardMode) {
                rng.Add(NPCID.Herpling);
                rng.Add(NPCID.Crimslime);
                rng.Add(NPCID.BloodJelly);
                rng.Add(NPCID.BloodFeeder);
                rng.Add(NPCID.CrimsonAxe);
                rng.Add(NPCID.IchorSticker);
                rng.Add(NPCID.PigronCrimson);
                rng.Add(NPCID.FloatyGross);
                rng.Add(NPCID.BloodMummy, 0.25);
            } else {
                rng.Add(NPCID.BloodCrawler);
                rng.Add(NPCID.CrimsonGoldfish);
                rng.Add(NPCID.FaceMonster);
                rng.Add(NPCID.Crimera);
            }

        } else {
            if (Main.hardMode) {
                rng.Add(NPCID.Corruptor);
                rng.Add(NPCID.CursedHammer);
                rng.Add(NPCID.Clinger);
                rng.Add(NPCID.PigronCorruption);
                rng.Add(NPCID.CorruptSlime);
                rng.Add(NPCID.Slimer);
                rng.Add(NPCID.DarkMummy, 0.25);
            } else {
                rng.Add(NPCID.EaterofSouls);
                rng.Add(NPCID.CorruptGoldfish);
                rng.Add(NPCID.DevourerHead);
            }
        }

        return rng.Get();
    }
}