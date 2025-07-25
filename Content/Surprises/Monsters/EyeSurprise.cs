﻿using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class EyeSurprise : GridBlockSurprise.ProjectileSpawner<EyeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return !Main.hardMode && chunk.ContentAnalysis.SuitableForHordeEvents && !Main.dayTime && chunk.TileCoord.Y < Main.worldSurface;
    }
}

public class EyeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.DemonEye;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        rng.Add(NPCID.DemonEye);
        rng.Add(NPCID.DemonEye2);
        rng.Add(NPCID.CataractEye);
        rng.Add(NPCID.CataractEye2);
        rng.Add(NPCID.SleepyEye);
        rng.Add(NPCID.SleepyEye2);
        rng.Add(NPCID.GreenEye);
        rng.Add(NPCID.GreenEye2);
        rng.Add(NPCID.PurpleEye);
        rng.Add(NPCID.PurpleEye2);

        if (Main.hardMode) {
            rng.Add(NPCID.WanderingEye, 4);
        }

        return rng.Get();
    }
}