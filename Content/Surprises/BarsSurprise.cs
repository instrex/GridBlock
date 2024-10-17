using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class BarsSurprise : GridBlockSurprise.ProjectileSpawner<BarsSurpriseProjectile> {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.EmptyTileAmount > 100 && chunk.ContentAnalysis.Placeable1x1SpotsCount > 42 
            && (chunk.TileCoord.Y > Main.worldSurface && chunk.TileCoord.Y < Main.UnderworldLayer);
    }
}

public class BarsSurpriseProjectile : TilePlaceSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = 90;
        TileType = TileID.MetalBars;
    }

    public override (int, int) GetTileTypeAndStyle() {
        var rng = new WeightedRandom<int>();
        rng.Add(0, 0.15); // copper
        rng.Add(1, 0.15); // tin
        rng.Add(2, 0.25); // iron
        rng.Add(3, 0.25); // lead
        rng.Add(4, 0.25); // silver
        rng.Add(5, 0.25); // tungsten
        rng.Add(6, 0.15); // gold
        rng.Add(7, 0.15); // platinum

        if (Main.hardMode) {
            rng.Add(11, 3); // cobalt
            rng.Add(12, 3); // palladium

            rng.Add(13, 2); // mythril
            rng.Add(14, 2); // orichalcum

            rng.Add(15, 1); // adamantium
            rng.Add(16, 1); // titanium
        }

        return (TileType, rng.Get());
    }

    public override void OnTilePlaced(Point tileCoord) {
        SoundEngine.PlaySound(SoundID.Item35 with { PitchVariance = 0.5f }, tileCoord.ToWorldCoordinates());
        Lighting.AddLight(tileCoord.ToWorldCoordinates(), Vector3.One * 0.5f);
    }
}
