using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;

namespace GridBlock.Content.Surprises;

public class RewardSurprise : GridBlockSurprise.ProjectileSpawner<RewardSurpriseProjectile> { }
public class RewardSurpriseProjectile : ItemShowerSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = Main.rand.Next(3, 8) * SpawnInterval;
    }

    public override (int, int) GetItemTypeAndStack() {
        var rng = new WeightedRandom<(int, int)>();
        rng.Add((ItemID.LifeCrystal, 1), 2);
        rng.Add((ItemID.ManaCrystal, 1), 0.2);
        rng.Add((ItemID.SilverCoin, 10));
        rng.Add((ItemID.SilverCoin, 50));
        rng.Add((ItemID.GoldCoin, 1), 0.25);
        rng.Add((ItemID.GoldCoin, 5), 0.1);
        return rng.Get();
    }

    public override void OnItemSpawned(Item item) {
        SoundEngine.PlaySound(SoundID.Item9);
        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(item.position, 32, 32, DustID.GoldCoin, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            dust.noGravity = true;
            dust.fadeIn = 1.5f;
        }
    }
}
