using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class ZombieSurprise : GridBlockSurprise.ProjectileSpawner<ZombieSurpriseProjectile> {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.EmptyTileAmount > 100 && (!Main.dayTime || chunk.TileCoord.Y > Main.worldSurface);
    }
}

public class ZombieSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.rand.Next(2, 4) * (Main.rand.NextFloat() < 0.1f ? 3 : 1)) * SpawnInterval;
        NpcType = NPCID.Skeleton;
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

        rng.Add(NPCID.Zombie);
        rng.Add(NPCID.SmallZombie);
        rng.Add(NPCID.BigZombie);

        rng.Add(NPCID.BaldZombie);
        rng.Add(NPCID.SmallBaldZombie);
        rng.Add(NPCID.BigBaldZombie);

        rng.Add(NPCID.PincushionZombie);
        rng.Add(NPCID.SmallPincushionZombie);
        rng.Add(NPCID.BigPincushionZombie);

        rng.Add(NPCID.SlimedZombie);
        rng.Add(NPCID.SmallSlimedZombie);
        rng.Add(NPCID.BigSlimedZombie);

        rng.Add(NPCID.SwampZombie);
        rng.Add(NPCID.SmallSwampZombie);
        rng.Add(NPCID.BigSwampZombie);

        rng.Add(NPCID.FemaleZombie);
        rng.Add(NPCID.SmallFemaleZombie);
        rng.Add(NPCID.BigFemaleZombie);

        rng.Add(NPCID.TwiggyZombie);
        rng.Add(NPCID.SmallTwiggyZombie);
        rng.Add(NPCID.BigTwiggyZombie);

        rng.Add(NPCID.TorchZombie, 0.5);

        return rng.Get();
    }
}