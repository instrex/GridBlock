using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;

namespace GridBlock.Content.Surprises;

internal class GoldAnimalsSurprise : GridBlockSurprise.ProjectileSpawner<GoldAnimalsSurpriseProjectile> {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.SuitableForHordeEvents && chunk.TileCoord.Y < Main.UnderworldLayer;
    }
}

public class GoldAnimalsSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(1, 3) * (Main.rand.NextFloat() < 0.1f ? 2 : 1)) * SpawnInterval;
        NpcType = NPCID.GoldBunny;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.Item109 with { PitchVariance = 1f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldCritter, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();
        rng.Add(NPCID.GoldBunny);
        rng.Add(NPCID.GoldBird);
        rng.Add(NPCID.GoldButterfly);
        rng.Add(NPCID.GoldDragonfly);
        rng.Add(NPCID.GoldFrog);
        rng.Add(NPCID.GoldGoldfish);
        rng.Add(NPCID.GoldGrasshopper);
        rng.Add(NPCID.GoldLadyBug);
        rng.Add(NPCID.GoldMouse);
        rng.Add(NPCID.GoldSeahorse);
        rng.Add(NPCID.GoldWorm);

        return rng.Get();
    }
}