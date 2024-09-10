using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria;

namespace GridBlock.Content.Surprises;

public class HealingSurprise : GridBlockSurprise.ProjectileSpawner<HealingSurpriseProjectile> { }
public class HealingSurpriseProjectile : ItemShowerSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = Main.rand.Next(10, 20) * 5;
        ItemType = ItemID.Heart;
        SpawnInterval = 5;
    }

    public override void OnItemSpawned(Item item) {
        SoundEngine.PlaySound(SoundID.Item9);
        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(item.position, 32, 32, DustID.GemRuby, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            dust.noGravity = true;
            dust.fadeIn = 1.5f;
        }
    }
}
