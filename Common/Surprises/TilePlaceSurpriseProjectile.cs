using GridBlock.Common;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace GridBlock.Common.Surprises;

public abstract class TilePlaceSurpriseProjectile : SurpriseProjectile {
    public int TileType { get; set; }

    public virtual bool CanPlaceTile(Point tileCoord, bool checkForBottomTileOrWall = true) =>
        (!Main.tile[tileCoord].HasTile || TileID.Sets.BreakableWhenPlacing[Main.tile[tileCoord].TileType])
        && (!checkForBottomTileOrWall || WorldGen.SolidTile(tileCoord.X, tileCoord.Y + 1) || Main.tile[tileCoord].WallType != 0)
        && !Main.chest.Any(c => c != null && new Rectangle(tileCoord.X, tileCoord.Y, 2, 2).Contains(c.x, c.y));

    public virtual void OnTilePlaced(Point tileCoord) { }

    public override void AI() {
        if (Projectile.ai[1]++ > 5) {
            Projectile.ai[1] = 0;

            for (var i = 0; i < 1000; i++) {
                var tileCoord = Chunk.TileCoord + new Point(Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize), Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize));
                if (CanPlaceTile(tileCoord)) {
                    if (WorldGen.PlaceTile(tileCoord.X, tileCoord.Y, TileType)) {
                        OnTilePlaced(tileCoord);
                        break;
                    }

                }
            }
        }
    }
}
