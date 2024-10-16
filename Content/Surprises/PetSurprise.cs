using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class PetSurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        for (var i = 0; i < 2; i++) {
            var equip = player.miscEquips[i];
            if (!equip.IsAir) {
                var clone = equip.Clone();
                equip.TurnToAir();

                var k = Item.NewItem(player.GetSource_FromThis(), player.getRect(), clone);
                Main.item[k].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 6);
                Main.item[k].noGrabDelay = 60 * 2;
            }
        }

        var rng = new WeightedRandom<int>();
        rng.Add(BuffID.BabyDinosaur);
        rng.Add(BuffID.BabyEater);
        rng.Add(BuffID.BabyFaceMonster);
        rng.Add(BuffID.BabyGrinch);
        rng.Add(BuffID.BabyHornet);
        rng.Add(BuffID.BabyImp);
        rng.Add(BuffID.BabyPenguin);
        rng.Add(BuffID.BabyRedPanda);
        rng.Add(BuffID.BabySkeletronHead);
        rng.Add(BuffID.BabySnowman);
        rng.Add(BuffID.BabyTruffle);
        rng.Add(BuffID.BabyWerewolf);
        rng.Add(BuffID.BerniePet);
        rng.Add(BuffID.BlackCat);
        rng.Add(BuffID.BlueChickenPet);
        rng.Add(BuffID.PetBunny);
        rng.Add(BuffID.CavelingGardener);
        rng.Add(BuffID.ChesterPet);
        rng.Add(BuffID.CompanionCube);
        rng.Add(BuffID.CursedSapling);
        rng.Add(BuffID.DirtiestBlock);
        rng.Add(BuffID.DynamiteKitten);
        rng.Add(BuffID.EyeballSpring);
        rng.Add(BuffID.FennecFox);
        rng.Add(BuffID.GlitteryButterfly);
        rng.Add(BuffID.GlommerPet);
        rng.Add(BuffID.JunimoPet);
        rng.Add(BuffID.LilHarpy);
        rng.Add(BuffID.PetLizard);
        rng.Add(BuffID.MiniMinotaur);
        rng.Add(BuffID.PetParrot);
        rng.Add(BuffID.PigPet);
        rng.Add(BuffID.Plantero);
        rng.Add(BuffID.Puppy);
        rng.Add(BuffID.PetSapling);
        rng.Add(BuffID.PetSpider);
        rng.Add(BuffID.SharkPup);
        rng.Add(BuffID.Spiffo);
        rng.Add(BuffID.Squashling);
        rng.Add(BuffID.SugarGlider);
        rng.Add(BuffID.TikiSpirit);
        rng.Add(BuffID.PetTurtle);
        rng.Add(BuffID.VoltBunny);
        rng.Add(BuffID.ZephyrFish);

        player.AddBuff(rng.Get(), 3600);
    }
}