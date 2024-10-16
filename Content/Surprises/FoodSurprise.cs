using GridBlock.Common.Surprises;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using GridBlock.Common;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class FoodSurprise : GridBlockSurprise.ProjectileSpawner<FoodSurpriseProjectile> {
    
}

public class FoodSurpriseProjectile : ItemShowerSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = Main.rand.Next(2, 6) * 5;
        ItemType = ItemID.Heart;
        SpawnInterval = 5;
    }

    public override (int, int) GetItemTypeAndStack() {
        var rng = new WeightedRandom<int>();

        rng.Add(ItemID.Marshmallow);
        rng.Add(ItemID.JojaCola);
        rng.Add(ItemID.Apple);
        rng.Add(ItemID.Apricot);
        rng.Add(ItemID.Banana);
        rng.Add(ItemID.BlackCurrant);
        rng.Add(ItemID.BloodOrange);
        rng.Add(ItemID.Cherry);
        rng.Add(ItemID.Coconut);
        rng.Add(ItemID.Elderberry);
        rng.Add(ItemID.Grapefruit);
        rng.Add(ItemID.Lemon);
        rng.Add(ItemID.Mango);
        rng.Add(ItemID.Peach);
        rng.Add(ItemID.Pineapple);
        rng.Add(ItemID.Plum);
        rng.Add(ItemID.Pomegranate);
        rng.Add(ItemID.Rambutan);
        rng.Add(ItemID.SpicyPepper);
        rng.Add(ItemID.Teacup);
        rng.Add(ItemID.CookedFish);
        rng.Add(ItemID.AppleJuice);
        rng.Add(ItemID.BunnyStew);
        rng.Add(ItemID.CookedMarshmallow);
        rng.Add(ItemID.GrilledSquirrel);
        rng.Add(ItemID.Lemonade);
        rng.Add(ItemID.PeachSangria);
        rng.Add(ItemID.RoastedBird);
        rng.Add(ItemID.SauteedFrogLegs);
        rng.Add(ItemID.ShuckedOyster);
        rng.Add(ItemID.FruitJuice);
        rng.Add(ItemID.BloodyMoscato);
        rng.Add(ItemID.MilkCarton);
        rng.Add(ItemID.PinaColada);
        rng.Add(ItemID.SmoothieofDarkness);
        rng.Add(ItemID.TropicalSmoothie);
        rng.Add(ItemID.FruitSalad);
        rng.Add(ItemID.PotatoChips);

        rng.Add(ItemID.Dragonfruit, 0.25);
        rng.Add(ItemID.Starfruit, 0.25);
        rng.Add(ItemID.FroggleBunwich, 0.25);
        rng.Add(ItemID.BowlofSoup, 0.25);
        rng.Add(ItemID.MonsterLasagna, 0.25);
        rng.Add(ItemID.PadThai, 0.25);
        rng.Add(ItemID.PumpkinPie, 0.25);
        rng.Add(ItemID.Sashimi, 0.25);
        rng.Add(ItemID.CoffeeCup, 0.25);
        rng.Add(ItemID.CookedShrimp, 0.25);
        rng.Add(ItemID.Escargot, 0.25);
        rng.Add(ItemID.Fries, 0.25);
        rng.Add(ItemID.LobsterTail, 0.25);
        rng.Add(ItemID.RoastedDuck, 0.25);
        rng.Add(ItemID.ChickenNugget, 0.25);
        rng.Add(ItemID.FriedEgg, 0.25);
        rng.Add(ItemID.IceCream, 0.25);
        rng.Add(ItemID.SeafoodDinner, 0.25);
        rng.Add(ItemID.CreamSoda, 0.25);

        rng.Add(ItemID.Burger, 0.1);
        rng.Add(ItemID.Pizza, 0.1);
        rng.Add(ItemID.Spaghetti, 0.1);
        rng.Add(ItemID.Steak, 0.1);
        rng.Add(ItemID.Bacon, 0.1);
        rng.Add(ItemID.BBQRibs, 0.1);

        return (rng.Get(), 1);
    }

    public override void OnItemSpawned(Item item) {
        SoundEngine.PlaySound(SoundID.Item2);
        for (var i = 0; i < 7; i++) {
            var dust = Dust.NewDustDirect(item.position, 32, 32, DustID.GemRuby, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            dust.noGravity = true;
            dust.fadeIn = 1.5f;
        }
    }
}
