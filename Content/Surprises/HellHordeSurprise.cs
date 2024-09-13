using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class HellHordeSurprise : GridBlockSurprise.ProjectileSpawner<HellHordeSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.TileCoord.Y >= Main.UnderworldLayer;
    }
}

public class HellHordeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
    }

    public override void OnNpcSpawned(NPC npc) {
        SoundEngine.PlaySound(SoundID.NPCHit1 with { PitchVariance = 0.25f });
        npc.velocity.Y = Main.rand.NextFloat(-4, -8);

        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.LavaMoss, 2f, 2f);
            dust.velocity.Y = Main.rand.NextFloat(-2, -6);
        }
    }

    public override int GetNpcType() {
        var rng = new WeightedRandom<int>();

        rng.Add(NPCID.Hellbat);
        rng.Add(NPCID.LavaSlime);
        rng.Add(NPCID.Demon);
        rng.Add(NPCID.VoodooDemon, 0.05);
        rng.Add(NPCID.FireImp, 0.5f);

        return rng.Get();
    }
}