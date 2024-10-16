using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

public class SlimeSurprise : GridBlockSurprise.ProjectileSpawner<SlimeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.SuitableForHordeEvents && !player.ZoneSkyHeight && chunk.TileCoord.Y < Main.worldSurface;
    }
}

public class SlimeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 2 : 1)) * SpawnInterval;
        NpcType = NPCID.BlueSlime;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.Item16 with { PitchVariance = 1f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.t_Slime, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        if (CurrentNpcIndex == 1 && Main.rand.NextFloat() < 0.1f) {
            return Main.hardMode ? NPCID.QueenSlimeBoss : NPCID.KingSlime;
        }

        var rng = new WeightedRandom<int>();

        if (Main.hardMode) {
            rng.Add(NPCID.KingSlime);
            rng.Add(NPCID.ShimmerSlime);
            rng.Add(NPCID.ToxicSludge);
            rng.Add(NPCID.CorruptSlime);
            rng.Add(NPCID.Slimer);
            rng.Add(NPCID.Crimslime);
            rng.Add(NPCID.IlluminantSlime);
            rng.Add(NPCID.RainbowSlime);
            rng.Add(NPCID.HoppinJack);
            rng.Add(NPCID.QueenSlimeMinionBlue);
            rng.Add(NPCID.QueenSlimeMinionPink);
            rng.Add(NPCID.QueenSlimeMinionPurple);

            return rng.Get();
        }

        rng.Add(NPCID.BlueSlime);
        rng.Add(NPCID.GreenSlime);
        rng.Add(NPCID.RedSlime, 0.75);
        rng.Add(NPCID.PurpleSlime, 0.75);
        rng.Add(NPCID.YellowSlime, 0.75);
        rng.Add(NPCID.BlackSlime, 0.5);
        rng.Add(NPCID.IceSlime, 0.1);
        rng.Add(NPCID.SandSlime, 0.1);
        rng.Add(NPCID.JungleSlime, 0.1);
        rng.Add(NPCID.SpikedJungleSlime, 0.05);
        rng.Add(NPCID.SpikedIceSlime, 0.05);
        rng.Add(NPCID.LavaSlime, 0.05);
        rng.Add(NPCID.Pinky, 0.01);

        return rng.Get();
    }
}