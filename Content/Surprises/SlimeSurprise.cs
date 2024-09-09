using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;

namespace GridBlock.Content.Surprises;

public class SlimeSurprise : GridBlockSurprise.ProjectileSpawner<SlimeSurpriseProjectile> { }
public class SlimeSurpriseProjectile : AmbushSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = Main.rand.Next(4, 10) * SpawnInterval;
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
        var rng = new WeightedRandom<int>();
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