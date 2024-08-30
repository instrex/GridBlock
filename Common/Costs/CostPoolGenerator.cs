using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common.Costs;

public static class CostPoolGenerator {
    public static WeightedRandom<Item> GetPool(CostGroup group) {
        var rng = new WeightedRandom<Item>();

        switch (group) {
            default:
                ModContent.GetInstance<GridBlock>().Logger.Warn($"Cost Group {group} isn't defined.");
                break;

            case CostGroup.Beginner:
                rng.Add(new(ItemID.Mushroom, 5));
                rng.Add(new(ItemID.Mushroom, 10), 0.5f);
                rng.Add(new(ItemID.Gel, 5));
                rng.Add(new(ItemID.Gel, 10), 0.5);
                rng.Add(new(ItemID.Gel, 15), 0.25);
                rng.Add(new(ItemID.Wood, 15));
                rng.Add(new(ItemID.Wood, 30), 0.5);
                rng.Add(new(ItemID.StoneBlock, 15));
                rng.Add(new(ItemID.StoneBlock, 30), 0.5);

                break;

            case CostGroup.Common:
                rng.Add(new(ItemID.Torch, 20));
                rng.Add(new(ItemID.Torch, 40), 0.5);
                rng.Add(new(ItemID.Daybloom, 5));
                rng.Add(new(ItemID.Sunflower, 1));
                rng.Add(new(ItemID.Mushroom, 15));
                rng.Add(new(ItemID.StoneBlock, 25));
                rng.Add(new(ItemID.StoneBlock, 50), 0.5);
                rng.Add(new(ItemID.ClayBlock, 25));
                rng.Add(new(ItemID.ClayBlock, 25), 0.5);
                rng.Add(new(ItemID.Cactus, 15));
                rng.Add(new(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar, 5));
                rng.Add(new(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar, 5));
                rng.Add(new(ItemID.AntlionMandible, 3));

                break;
        }

        return rng;
    }

}
