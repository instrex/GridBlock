using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class BuffUpSurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        var rng = new WeightedRandom<(int, int)>();
        rng.Add((BuffID.AmmoReservation, 3600 * 2));
        rng.Add((BuffID.Archery, 3600 * 2));
        rng.Add((BuffID.Battle, 3600 * 2));
        rng.Add((BuffID.Builder, 3600 * 12));
        rng.Add((BuffID.BiomeSight, 3600 * 4));
        rng.Add((BuffID.Calm, 3600 * 2));
        rng.Add((BuffID.Dangersense, 3600 * 4));
        rng.Add((BuffID.Endurance, 3600 * 2));
        rng.Add((BuffID.Featherfall, 3600 * 1));
        rng.Add((BuffID.Heartreach, 3600 * 4));
        rng.Add((BuffID.Inferno, 3600 * 1));
        rng.Add((BuffID.Ironskin, 3600 * 3));
        rng.Add((BuffID.Lifeforce, 3600 * 2));
        rng.Add((BuffID.MagicPower, 3600 * 2));
        rng.Add((BuffID.ManaRegeneration, 3600 * 2));
        rng.Add((BuffID.Mining, 3600 * 3));
        rng.Add((BuffID.ObsidianSkin, 3600 * 2));
        rng.Add((BuffID.Rage, 3600 * 4));
        rng.Add((BuffID.Shine, 3600 * 2));
        rng.Add((BuffID.Summoning, 3600 * 3));
        rng.Add((BuffID.Swiftness, 3600 * 3));
        rng.Add((BuffID.Wrath, 3600 * 2));

        var buffCount = (int)(Main.rand.Next(3, 6) * (Main.hardMode ? 1.5f : 1));
        for (var i = 0; i < buffCount; i++) {
            var (buff, duration) = rng.Get();
            player.AddBuff(buff, duration);
        }
    }
}
