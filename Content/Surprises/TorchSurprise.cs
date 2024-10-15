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

public class TorchSurprise : GridBlockSurprise.ProjectileSpawner<TorchSurpriseProjectile> {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.EmptyTileAmount > 100 && (chunk.ContentAnalysis.WallSpotsCount > 42 || chunk.ContentAnalysis.Placeable1x1SpotsCount > 42) 
            && ((chunk.TileCoord.Y > Main.worldSurface && chunk.TileCoord.Y < Main.UnderworldLayer) || !Main.dayTime);
    }
}

public class TorchSurpriseProjectile : TilePlaceSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = 60;
        TileType = TileID.Torches;
    }
}
