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
        var numberOfSolidsOrWalls = 0;
        for (var i = 0; i < GridBlockWorld.Instance.Chunks.CellSize; i++) 
            for (var k = 0; k < GridBlockWorld.Instance.Chunks.CellSize; k++) {
                var coord = new Point(chunk.TileCoord.X + i, chunk.TileCoord.Y + k);
                if (WorldGen.SolidOrSlopedTile(coord.X, coord.Y) || Main.tile[coord].WallType != 0)
                    numberOfSolidsOrWalls++;
            }
        
        // only happen in places where there's enough tiles for torches
        if (numberOfSolidsOrWalls < 25) 
            return false;
        
        return chunk.EmptyTileAmount > 100 && (chunk.TileCoord.Y > Main.worldSurface || !Main.dayTime);
    }
}

public class TorchSurpriseProjectile : TilePlaceSurpriseProjectile {
    public override void SetDefaults() {
        base.SetDefaults();

        Projectile.timeLeft = 60;
        TileType = TileID.Torches;
    }
}
