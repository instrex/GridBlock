using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common.Costs;

public static class CostPoolGenerator {
    public static WeightedRandom<Item> GetPool(CostGroup group, int? seed = default) {
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
                rng.Add(new(ItemID.Mushroom, 5), 0.15);
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
                rng.Add(new(ItemID.RichMahogany, 15), 0.5);
                rng.Add(new(ItemID.PalmWood, 15), 0.5);
                rng.Add(new(ItemID.Torch, 20));
                rng.Add(new(ItemID.Torch, 40), 0.25);
                rng.Add(new(ItemID.Daybloom, 5));
                rng.Add(new(ItemID.Sunflower, 1), 0.5);
                rng.Add(new(ItemID.Mushroom, 15));
                rng.Add(new(ItemID.StoneBlock, 25));
                rng.Add(new(ItemID.StoneBlock, 50), 0.25);
                rng.Add(new(ItemID.MudBlock, 25));
                rng.Add(new(ItemID.MudBlock, 50), 0.25);
                rng.Add(new(ItemID.SandBlock, 25));
                rng.Add(new(ItemID.IceBlock, 25));
                rng.Add(new(ItemID.SandBlock, 50), 0.25);
                rng.Add(new(ItemID.SnowBlock, 25));
                rng.Add(new(ItemID.SnowBlock, 50), 0.25);
                rng.Add(new(ItemID.ClayBlock, 25));
                rng.Add(new(ItemID.ClayBlock, 50), 0.25);
                rng.Add(new(ItemID.Cobweb, 20));
                rng.Add(new(ItemID.Cactus, 15));
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 5));
                rng.Add(new(ItemID.AntlionMandible, 3));
                rng.Add(new(ItemID.GoldCoin, 5));
                rng.Add(new(ItemID.GoldCoin, 15), 0.25);
                rng.Add(new(ItemID.Lens, 5));
                break;

            case CostGroup.Advanced:
                rng.Add(new(ItemID.JungleSpores, 5), 0.5);
                rng.Add(new(ItemID.Vine, 5), 0.5);
                rng.Add(new(ItemID.Stinger, 10), 0.5);
                rng.Add(new(ItemID.Amethyst, 5));
                rng.Add(new(ItemID.Sapphire, 5));
                rng.Add(new(ItemID.Emerald, 5));
                rng.Add(new(ItemID.Ruby, 5));
                rng.Add(new(ItemID.Diamond, 1), 0.5);
                rng.Add(new(ItemID.Amber, 1), 0.5);
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 5));
                rng.Add(new(ItemID.Cloud, 15));
                rng.Add(new(ItemID.GlowingMushroom, 15));
                rng.Add(new(ItemID.Granite, 25));
                rng.Add(new(ItemID.Marble, 25));
                rng.Add(new(ItemID.HardenedSand, 25));
                rng.Add(new(WorldGen.WorldGenParam_Evil == 0 ? ItemID.EbonstoneBlock : ItemID.CrimstoneBlock, 25));
                rng.Add(new(ItemID.GoldCoin, 10));
                rng.Add(new(ItemID.GoldCoin, 30), 0.25);
                break;

            case CostGroup.Hardcore:
                //rng.Add(new(ItemID.IronBar, 5));
                //rng.Add(new(ItemID.LeadBar, 5));
                //rng.Add(new(ItemID.SilverBar, 5));
                //rng.Add(new(ItemID.TungstenBar, 5));
                //rng.Add(new(ItemID.GoldBar, 5));
                //rng.Add(new(ItemID.PlatinumBar, 5));
                rng.Add(new(ItemID.LunarBar, 999));
                rng.Add(new(ItemID.Meowmere, 999));
                rng.Add(new(ItemID.Zenith, 999));
                break;

        }

        return rng;
    }

}
