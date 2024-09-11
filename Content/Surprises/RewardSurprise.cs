using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;
using System.Linq;
using System.Collections.Generic;

namespace GridBlock.Content.Surprises;

public class RewardSurprise : GridBlockSurprise.ProjectileSpawner<RewardSurpriseProjectile> {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) => false;
}

public class RewardSurpriseProjectile : ItemShowerSurpriseProjectile {
    static readonly HashSet<int> OneTimeRewards = [
        ItemID.HermesBoots, ItemID.IceSkates, ItemID.FlurryBoots, ItemID.CreativeWings, ItemID.WandofSparking, ItemID.EnchantedSword,
        ItemID.Starfury, ItemID.ShinyRedBalloon,
        ItemID.Aglet,
        ItemID.Radar,
        ItemID.Mace,
        ItemID.EnchantedBoomerang,
        ItemID.ShoeSpikes,
        ItemID.BandofRegeneration,
        ItemID.CloudinaBottle,
        ItemID.Extractinator];

    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.hardMode ? 3 : 5) * SpawnInterval;
        SoundEngine.PlaySound(SoundID.AchievementComplete with { PitchVariance = 0.2f });
    }

    public override (int, int) GetItemTypeAndStack() {
        var player = Main.player[Projectile.owner];
        var rng = new WeightedRandom<(int, int)>();
        void AddOneTimeLoot(int itemId, float weight = 1.0f, int stack = 1) {
            if (player.GetModPlayer<GridBlockPlayer>().RichChunkRewards.Any(i => i.type == itemId))
                return;

            rng.Add((itemId, stack), weight);
        }

        // add life crystals often
        if (player.statLifeMax < 400) rng.Add((ItemID.LifeCrystal, 1), 0.5);
        else if (NPC.downedMechBossAny) rng.Add((ItemID.LifeFruit, 1), 0.5);

        // add mana crystals sometimes
        if (player.statManaMax < 200) rng.Add((ItemID.ManaCrystal, 1), 0.025);

        // add onee-tiem rewards
        foreach (var reward in OneTimeRewards) {
            AddOneTimeLoot(reward, 0.25f);
        }

        // TODO: add hardmode rewards

        if (Main.hardMode) {
            rng.Add((WorldGen.SavedOreTiers.Cobalt == TileID.Cobalt ? ItemID.CobaltBar : ItemID.PalladiumBar, Main.rand.Next(1, 5) * 5));
            rng.Add((WorldGen.SavedOreTiers.Mythril == TileID.Mythril ? ItemID.MythrilBar : ItemID.OrichalcumBar, Main.rand.Next(1, 5) * 4), 0.5);
            rng.Add((WorldGen.SavedOreTiers.Adamantite == TileID.Adamantite ? ItemID.AdamantiteBar : ItemID.TitaniumBar, Main.rand.Next(1, 5) * 3), 0.25);

            if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) {
                rng.Add((ItemID.ChlorophyteBar, Main.rand.Next(1, 5) * 4), 0.5);
            }
        }

        // common rewards
        rng.Add((ItemID.HerbBag, Main.rand.Next(1, 5)), 0.25);

        // coin rewards are rarer
        if (!Main.hardMode) {
            rng.Add((ItemID.SilverCoin, 10), 0.25);
            rng.Add((ItemID.SilverCoin, 50), 0.25);
        }
        
        rng.Add((ItemID.GoldCoin, Main.hardMode ? 50 : 1), 0.15);
        rng.Add((ItemID.GoldCoin, Main.hardMode ? 100 : 5), 0.05);

        return rng.Get();
    }

    public override void OnItemSpawned(Item item) {
        if (OneTimeRewards.Contains(item.type)) {
            Main.player[Projectile.owner].GetModPlayer<GridBlockPlayer>().RichChunkRewards.Add(item.Clone());
        }

        SoundEngine.PlaySound(SoundID.Item158 with { PitchVariance = 0.5f });
        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(item.position, 32, 32, DustID.GoldCoin, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            dust.noGravity = true;
            dust.fadeIn = 1.5f;
        }
    }
}
