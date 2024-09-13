using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common.Costs;

public static class CostPoolGenerator {
    public const int RewardChunkBasePrice = 5;
    public const int RewardChunkBasePriceHardmode = 15;

    /// <summary>
    /// Reward chunk price will increase based on this value for each unique item obtained.
    /// </summary>
    public const float RewardChunkIncrease = 0.1f;

    public static WeightedRandom<Item> GetPool(CostGroup group, GridBlockChunk chunk, int? seed = default) {
        var rng = new WeightedRandom<Item>(seed ?? Main.rand.Next());
        
        switch (group) {
            default:
                ModContent.GetInstance<GridBlock>().Logger.Warn($"Cost Group {group} isn't defined.");
                break;

            case CostGroup.Expensive:
                rng.Add(new(ItemID.GoldCoin, 50));
                break;

            case CostGroup.Beginner:
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronOre : ItemID.LeadOre, 5));
                rng.Add(new(ItemID.Mushroom), 0.15);
                rng.Add(new(ItemID.Gel, 5));
                rng.Add(new(ItemID.Gel, 10), 0.5);
                rng.Add(new(ItemID.Gel, 15), 0.25);
                rng.Add(new(ItemID.Acorn, 2));
                rng.Add(new(ItemID.Wood, 15));
                rng.Add(new(ItemID.Wood, 30), 0.5);
                rng.Add(new(ItemID.StoneBlock, 15));
                rng.Add(new(ItemID.StoneBlock, 30), 0.5);
                rng.Add(new(ItemID.SilverCoin, 10));
                break;

            case CostGroup.Common:
                rng.Add(new(ItemID.Wood, 15));
                rng.Add(new(ItemID.Torch, 20));
                rng.Add(new(ItemID.Torch, 40), 0.25);
                rng.Add(new(ItemID.Daybloom, 3));
                rng.Add(new(ItemID.Sunflower, 1), 0.5);
                rng.Add(new(ItemID.Mushroom, 3));
                rng.Add(new(ItemID.StoneBlock, 25));
                rng.Add(new(ItemID.StoneBlock, 50), 0.25);
                rng.Add(new(ItemID.MudBlock, 25));
                rng.Add(new(ItemID.MudBlock, 50), 0.25);
                rng.Add(new(ItemID.SandBlock, 25));
                rng.Add(new(ItemID.IceBlock, 25), 0.5);
                rng.Add(new(ItemID.SandBlock, 50), 0.25);
                rng.Add(new(ItemID.SnowBlock, 25));
                rng.Add(new(ItemID.SnowBlock, 50), 0.25);
                rng.Add(new(ItemID.ClayBlock, 25));
                rng.Add(new(ItemID.ClayBlock, 50), 0.25);
                rng.Add(new(ItemID.Cobweb, 20));
                rng.Add(new(ItemID.Cactus, 15));
                rng.Add(new(ItemID.FallenStar), 0.5);
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 3));
                rng.Add(new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 3));
                rng.Add(new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 3));
                rng.Add(new(ItemID.GoldCoin, 1));
                rng.Add(new(ItemID.GoldCoin, 3), 0.25);
                rng.Add(new(ItemID.Lens, 3));
                break;

            case CostGroup.Advanced:
                rng.Add(new(ItemID.JungleSpores, 3), 0.15);
                rng.Add(new(ItemID.Vine, 3), 0.15);
                rng.Add(new(ItemID.RichMahogany, 15), 0.5);
                rng.Add(new(ItemID.PalmWood, 15), 0.5);
                rng.Add(new(ItemID.AntlionMandible, 3));
                rng.Add(new(ItemID.Stinger, 10), 0.15);
                rng.Add(new(ItemID.Amethyst, 3));
                rng.Add(new(ItemID.Sapphire, 3));
                rng.Add(new(ItemID.Emerald, 3));
                rng.Add(new(ItemID.Ruby, 3));
                rng.Add(new(ItemID.Diamond, 1), 0.5);
                rng.Add(new(ItemID.Amber, 1), 0.5);
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 5));
                rng.Add(new(ItemID.Cloud, 15));
                rng.Add(new(ItemID.Bone, 10));
                rng.Add(new(ItemID.FossilOre, 3));
                rng.Add(new(ItemID.FlinxFur, 3));
                rng.Add(new(ItemID.SpikyBall, 15));
                rng.Add(new(ItemID.GlowingMushroom, 15));
                rng.Add(new(ItemID.Granite, 25));
                rng.Add(new(ItemID.Marble, 25));
                rng.Add(new(ItemID.Obsidian, 10));
                rng.Add(new(ItemID.AshBlock, 25), 0.5);
                rng.Add(new(ItemID.FallenStar, 5), 0.5);
                rng.Add(new(ItemID.TatteredCloth, 3), 0.5);
                rng.Add(new(ItemID.HardenedSand, 25));
                rng.Add(new(WorldGen.WorldGenParam_Evil == 0 ? ItemID.EbonstoneBlock : ItemID.CrimstoneBlock, 15));
                rng.Add(new(WorldGen.WorldGenParam_Evil == 0 ? ItemID.ShadowScale : ItemID.TissueSample, 5));
                rng.Add(new(ItemID.GoldCoin, 5));
                rng.Add(new(ItemID.GoldCoin, 10), 0.25);
                break;

            case CostGroup.Adventure:
                rng.Add(new(ItemID.Bunny));
                rng.Add(new(ItemID.Toilet));
                rng.Add(new(ItemID.Burger));
                rng.Add(new(ItemID.WaterBucket));
                rng.Add(new(ItemID.Abeemination));
                rng.Add(new(ItemID.BladeofGrass));
                rng.Add(new(ItemID.NightsEdge));
                rng.Add(new(ItemID.GuideVoodooDoll));
                rng.Add(new(ItemID.EoCShield));
                rng.Add(new(ItemID.CobaltShield));
                rng.Add(new(ItemID.WaterCandle));
                rng.Add(new(ItemID.SuspiciousLookingEye));
                rng.Add(new(ItemID.MeteoriteBar, 10));
                rng.Add(new(WorldGen.WorldGenParam_Evil == 0 ? ItemID.DemoniteBar : ItemID.CrimtaneBar, 10));
                break;

        }

        if (chunk.TileCoord.Y > Main.UnderworldLayer) {
            rng.Add(new(ItemID.AshBlock, 50), 2);
            rng.Add(new(ItemID.Hellstone, 10), 1);
            rng.Add(new(ItemID.MeteoriteBar, 5), 1);
            rng.Add(new(ItemID.DemonTorch, 5), 1);
        }

        return rng;
    }

}
