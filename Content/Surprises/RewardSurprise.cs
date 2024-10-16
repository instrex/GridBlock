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
    public static readonly HashSet<int> OneTimeRewards = [
        ItemID.HermesBoots, 
        ItemID.IceSkates,
        ItemID.FlurryBoots,
        ItemID.CreativeWings, 
        ItemID.WandofSparking, 
        ItemID.EnchantedSword,
        ItemID.Starfury, 
        ItemID.ShinyRedBalloon,
        ItemID.Aglet,
        ItemID.Radar,
        ItemID.Mace,
        ItemID.EnchantedBoomerang,
        ItemID.ShoeSpikes,
        ItemID.BandofRegeneration,
        ItemID.CloudinaBottle,
        ItemID.Extractinator,
        ItemID.LavaCharm,
        ItemID.AnkletoftheWind
    ];

    public static readonly HashSet<int> AnkhRewards = [
        ItemID.Blindfold,
        ItemID.PocketMirror,
        ItemID.Vitamins,
        ItemID.AdhesiveBandage,
        ItemID.Nazar,
        ItemID.TrifoldMap,
        ItemID.Bezoar,
        ItemID.ArmorPolish,
        ItemID.Megaphone,
        ItemID.FastClock,
    ];

    public static readonly HashSet<int> PhoneRewards = [
        ItemID.DepthMeter,
        ItemID.Compass,
        ItemID.Radar,
        ItemID.TallyCounter,
        ItemID.LifeformAnalyzer,
        ItemID.DPSMeter,
        ItemID.Stopwatch,
        ItemID.MetalDetector,
        ItemID.FishermansGuide,
        ItemID.WeatherRadio,
        ItemID.Sextant,
    ];

    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = (Main.hardMode ? 6 : 5) * SpawnInterval;
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
        rng.Add((ItemID.LifeCrystal, 1), player.statLifeMax < 400 ? 4.5 : 0.0125);
        if (NPC.downedMechBossAny) rng.Add((ItemID.LifeFruit, 1), player.statLifeMax < 500 ? 1 : 0.0125);

        // add mana crystals sometimes
        if (player.statManaMax < 200) rng.Add((ItemID.ManaCrystal, 1), 0.025);

        // add onee-tiem rewards
        foreach (var reward in OneTimeRewards) {
            AddOneTimeLoot(reward, 0.25f);
        }

        if (Main.hardMode) {
            // add onee-tiem rewards ankh
            foreach (var reward in AnkhRewards) {
                AddOneTimeLoot(reward, 0.025f);
            }
        }

        // add cell phone ingredients
        foreach (var reward in PhoneRewards) {
            AddOneTimeLoot(reward, 0.0125f);
        }

        if (Main.hardMode) {
            rng.Add((WorldGen.SavedOreTiers.Cobalt == TileID.Cobalt ? ItemID.CobaltBar : ItemID.PalladiumBar, Main.rand.Next(1, 3) * 3), 0.25);
            rng.Add((WorldGen.SavedOreTiers.Mythril == TileID.Mythril ? ItemID.MythrilBar : ItemID.OrichalcumBar, Main.rand.Next(1, 3) * 2), 0.1);
            rng.Add((WorldGen.SavedOreTiers.Adamantite == TileID.Adamantite ? ItemID.AdamantiteBar : ItemID.TitaniumBar, Main.rand.Next(1, 5) * 1), 0.025);

            rng.Add((ItemID.WoodenCrateHard, 1), 0.15);
            rng.Add((ItemID.IronCrateHard, 1), 0.1);
            rng.Add((ItemID.GoldenCrateHard, 1), 0.05);

            if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) {
                rng.Add((ItemID.ChlorophyteBar, Main.rand.Next(1, 5) * 2), 0.15);
            }
        } else {
            rng.Add((ItemID.WoodenCrate, 1), 0.15);
            rng.Add((ItemID.IronCrate, 1), 0.1);
            rng.Add((ItemID.GoldenCrate, 1), 0.05);
        }

        // add healies
        if (Main.hardMode) rng.Add((NPC.downedPlantBoss ? ItemID.SuperHealingPotion : ItemID.GreaterHealingPotion, Main.rand.Next(1, 5)), 0.5);
        else rng.Add((NPC.downedBoss2 ? ItemID.HealingPotion : ItemID.LesserHealingPotion, Main.rand.Next(1, 5)), 0.5);

        // common rewards
        rng.Add((ItemID.HerbBag, Main.rand.Next(1, 5)), 0.05);
        rng.Add((ItemID.Geode, Main.rand.Next(1, 2)), 0.05);
        rng.Add((ItemID.IronskinPotion, Main.rand.Next(1, 3) * 2), 0.1);
        rng.Add((ItemID.ArcheryPotion, Main.rand.Next(1, 3) * 2), 0.1);
        rng.Add((ItemID.InfernoPotion, Main.rand.Next(1, 3) * 2), 0.1);
        rng.Add((ItemID.EndurancePotion, Main.rand.Next(1, 3) * 2), 0.1);
        rng.Add((ItemID.SpelunkerPotion, Main.rand.Next(1, 3) * 2), 0.1);
        rng.Add((ItemID.WrathPotion, Main.rand.Next(1, 3) * 2), 0.1);

        rng.Add((ItemID.LuckPotion, 1), 0.05);
        rng.Add((ItemID.LuckPotionLesser, 1), 0.025);
        rng.Add((ItemID.LuckPotionGreater, 1), 0.0125);

        return rng.Get();
    }

    public override void OnItemSpawned(Item item) {
        if (OneTimeRewards.Concat(AnkhRewards).Concat(PhoneRewards).Contains(item.type)) {
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
