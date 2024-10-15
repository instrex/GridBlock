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

public class GemSurprise : GridBlockSurprise.ProjectileSpawner<GemSurpriseProjectile> {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.EmptyTileAmount > 100 && chunk.ContentAnalysis.Placeable1x1SpotsCount > 42 
            && (chunk.TileCoord.Y > Main.worldSurface && chunk.TileCoord.Y < Main.UnderworldLayer);
    }
}

public class GemSurpriseProjectile : TilePlaceSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = 60;
        TileType = TileID.ExposedGems;
    }

    public override (int, int) GetTileTypeAndStyle() {
        return (TileType, Main.rand.Next(7));
    }

    public override void OnTilePlaced(Point tileCoord) {
        SoundEngine.PlaySound(SoundID.Item35 with { PitchVariance = 0.5f }, tileCoord.ToWorldCoordinates());
        Lighting.AddLight(tileCoord.ToWorldCoordinates(), Vector3.One);
    }
}
