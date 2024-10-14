using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common.Costs;

public record struct CostPoolItem(int Type, int Stack = 1, double Weight = 1.0, bool ExcludeFromHardmodeScaling = false) {
    public Item ToItem() => new(Type, Main.hardMode && !ExcludeFromHardmodeScaling ? Stack * 2 : Stack);
}


public static class CostPoolGenerator {
    public const int RewardChunkBasePrice = 5;
    public const int RewardChunkBasePriceHardmode = 15;

    /// <summary>
    /// Reward chunk price will increase based on this value for each unique item obtained.
    /// </summary>
    public const float RewardChunkIncrease = 0.1f;

    public static WeightedRandom<CostPoolItem> GetPool(CostGroup group, GridBlockChunk chunk, int? seed = default) {
        if (chunk.Group == CostGroup.PaidReward) {
            var assRng = new WeightedRandom<CostPoolItem>(seed ?? Main.rand.Next());
            assRng.Add(new CostPoolItem(ItemID.GoldCoin, 1));

            // technically no rng is needed, but guh!
            return assRng;
        }

        List<CostPoolItem> candidates = [];

        List<CostPoolItem> commonGroup = [
            new(ItemID.Wood, 15),
            new(ItemID.Torch, 20),
            new(ItemID.Torch, 40, 0.25),
            new(ItemID.Daybloom, 3),
            new(ItemID.Sunflower, 1, 0.5),
            new(ItemID.Mushroom, 3),
            new(ItemID.StoneBlock, 25),
            new(ItemID.StoneBlock, 50, 0.25),
            new(ItemID.MudBlock, 25),
            new(ItemID.MudBlock, 50, 0.25),
            new(ItemID.SandBlock, 25),
            new(ItemID.SandBlock, 50, 0.25),
            new(ItemID.IceBlock, 25, 0.5),
            new(ItemID.SnowBlock, 25),
            new(ItemID.SnowBlock, 50, 0.25),
            new(ItemID.ClayBlock, 25),
            new(ItemID.ClayBlock, 50, 0.25),
            new(ItemID.Cobweb, 20),
            new(ItemID.Cactus, 15),
            new(ItemID.FallenStar, 1, 0.5),
            new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 3),
            new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 3),
            new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 3),
            new(ItemID.GoldCoin, 1),
            new(ItemID.GoldCoin, 3, 0.25),
            new(ItemID.Lens, 3),
        ];

        List<CostPoolItem> advancedGroup = [
            new(ItemID.JungleSpores, 3, 0.15),
            new(ItemID.Vine, 3, 0.15),
            new(ItemID.RichMahogany, 15, 0.5),
            new(ItemID.PalmWood, 15, 0.5),
            new(ItemID.AntlionMandible, 3),
            new(ItemID.Stinger, 10, 0.15),
            new(ItemID.Amethyst, 3),
            new(ItemID.Sapphire, 3),
            new(ItemID.Emerald, 3),
            new(ItemID.Ruby, 3),
            new(ItemID.Diamond, 1, 0.5),
            new(ItemID.Amber, 1, 0.5),
            new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 5),
            new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 5),
            new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 5),
            new(WorldGen.crimson ? ItemID.CrimtaneBar : ItemID.DemoniteBar, 3, 0.25),
            new(ItemID.HellstoneBar, 3, 0.15),
            new(ItemID.MeteoriteBar, 3, 0.25),
            new(ItemID.Cloud, 15),
            new(ItemID.Bone, 10),
            new(ItemID.FossilOre, 3),
            new(ItemID.FlinxFur, 3, 0.2),
            new(ItemID.SpikyBall, 15),
            new(ItemID.GlowingMushroom, 15),
            new(ItemID.Granite, 25),
            new(ItemID.Marble, 25),
            new(ItemID.Obsidian, 10),
            new(ItemID.AshBlock, 25, 0.5),
            new(ItemID.FallenStar, 5, 0.5),
            new(ItemID.TatteredCloth, 3, 0.5),
            new(ItemID.HardenedSand, 25),
            new(WorldGen.crimson ? ItemID.CrimstoneBlock : ItemID.EbonstoneBlock, 15),
            new(WorldGen.crimson ? ItemID.TissueSample : ItemID.ShadowScale, 5),
            new(ItemID.GoldCoin, 5),
            new(ItemID.GoldCoin, 10, 0.25),
            new(ItemID.WaterCandle),
            new(ItemID.ManaCrystal, 1, 0.5),
            new(ItemID.ManaCrystal, 3, 0.25),
            new(Main.hardMode ? ItemID.WoodenCrateHard : ItemID.WoodenCrate, 1, 0.25),
            new(Main.hardMode ? ItemID.IronCrateHard : ItemID.IronCrate, 1, 0.25),
            new(Main.hardMode ? ItemID.GoldenCrateHard : ItemID.GoldenCrate, 1, 0.25),
        ];

        if (Main.hardMode) {
            advancedGroup.AddRange([
                new(ItemID.LifeCrystal, 1),
                new(ItemID.LifeCrystal, 2, 0.25),
                new(WorldGen.SavedOreTiers.Cobalt == TileID.Cobalt ? ItemID.CobaltBar : ItemID.PalladiumBar, 2),
                new(WorldGen.SavedOreTiers.Mythril == TileID.Mythril ? ItemID.MythrilBar : ItemID.OrichalcumBar, 2),
                new(WorldGen.SavedOreTiers.Adamantite == TileID.Adamantite ? ItemID.AdamantiteBar : ItemID.TitaniumBar, 2),
                new(ItemID.ChlorophyteBar, 1, 0.5),
                new(ItemID.ChlorophyteBar, 2, 0.15),
                new(ItemID.SoulofFlight, 6),
                new(ItemID.SoulofLight, 6),
                new(ItemID.SoulofNight, 6),
                new(ItemID.TurtleShell, 1, 0.25),
            ]);

            if (NPC.downedMechBoss1) advancedGroup.Add(new(ItemID.SoulofMight, 4));
            if (NPC.downedMechBoss2) advancedGroup.Add(new(ItemID.SoulofSight, 4));
            if (NPC.downedMechBoss3) advancedGroup.Add(new(ItemID.SoulofFright, 4));
            if (NPC.downedGolemBoss) advancedGroup.Add(new(ItemID.BeetleHusk, 4));
            if (NPC.downedPlantBoss) advancedGroup.Add(new(ItemID.ShroomiteBar, 3));
        }

        List<CostPoolItem> adventureGroup = [
            new(ItemID.Bunny),
            new(ItemID.Toilet),
            new(ItemID.Burger),
            new(ItemID.WaterBucket),
            new(ItemID.Abeemination),
            new(ItemID.BladeofGrass),
            new(ItemID.NightsEdge),
            new(ItemID.GuideVoodooDoll),
            new(ItemID.CobaltShield),
            new(ItemID.SuspiciousLookingEye),
            new(ItemID.DemonScythe),
            new(ItemID.WaterBolt),
            new(ItemID.HoneyBucket),
            new(ItemID.HermesBoots),
            new(ItemID.LuckyHorseshoe),
            new(ItemID.ShinyRedBalloon),
        ];

        var rewardRng = seed is int seedForUnifiedRandom ? new UnifiedRandom(seedForUnifiedRandom) : Main.rand;

        switch (group) {
            default:
                ModContent.GetInstance<GridBlock>().Logger.Warn($"Cost Group {group} isn't defined.");
                break;

            case CostGroup.Beginner:
                candidates.AddRange([
                    new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronOre : ItemID.LeadOre, 6),
                    new(ItemID.Mushroom, Weight: 0.15),
                    new(ItemID.Gel, 5),
                    new(ItemID.Gel, 15, 0.25),
                    new(ItemID.Gel, 10, 0.5),
                    new(ItemID.Acorn, 2),
                    new(ItemID.Wood, 15),
                    new(ItemID.Wood, 30, 0.5),
                    new(ItemID.StoneBlock, 15),
                    new(ItemID.StoneBlock, 30, 0.5),
                    new(ItemID.SilverCoin, 10),
                ]);

                break;

            case CostGroup.Common:
                candidates.AddRange(commonGroup);
                break;

            case CostGroup.Advanced:
                candidates.AddRange(advancedGroup);
                break;

            case CostGroup.Adventure:
                candidates.AddRange(adventureGroup.Select(a => a with { ExcludeFromHardmodeScaling = true }));
                if (Main.expertMode) candidates.Add(new(ItemID.EoCShield, ExcludeFromHardmodeScaling: true));
                break;
        }

        if (chunk.TileCoord.Y > Main.UnderworldLayer) {
            candidates.AddRange([
                new(ItemID.AshBlock, 50, 2),
                new(ItemID.AshBlock, 10),
                new(ItemID.Hellstone, 10),
                new(ItemID.MeteoriteBar, 5),
                new(ItemID.DemonTorch, 5),
                new(ItemID.ObsidianBrick, 10, 2),
            ]);
        }

        var rng = new WeightedRandom<CostPoolItem>(seed is int newSeed ? newSeed : Main.rand.Next());
        foreach (var item in candidates) rng.Add(item, item.Weight);

        return rng;
    }

}
