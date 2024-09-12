using GridBlock.Common;
using GridBlock.Common.Globals;
using Microsoft.Xna.Framework;
using Terraria;

namespace GridBlock.Common.Surprises;

public abstract class ItemShowerSurpriseProjectile : SurpriseProjectile {
    public int ItemType { get; set; }
    public int SpawnInterval { get; set; } = 10;

    public virtual (int, int) GetItemTypeAndStack() => (ItemType, 1);

    public virtual bool CanSpawnItemAtLocation(Vector2 worldPos, Point tileCoord) => !Main.tile[tileCoord].HasTile;

    public virtual void OnItemSpawned(Item item) { }

    public override void AI() {
        if (Projectile.ai[1]++ > 5) {
            Projectile.ai[1] = 0;

            for (var i = 0; i < 1000; i++) {
                var tileCoord = Chunk.TileCoord + new Point(Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize), Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize));
                if (CanSpawnItemAtLocation(tileCoord.ToWorldCoordinates(), tileCoord)) {
                    var (type, stack) = GetItemTypeAndStack();
                    var item = Item.NewItem(Projectile.GetSource_FromThis(), tileCoord.ToWorldCoordinates(), type, stack);
                    Main.item[item].velocity.Y = -4;
                    Main.item[item].GetGlobalItem<GridBlockItem>().IsGridBlockReward = true;
                    OnItemSpawned(Main.item[item]);
                    break;
                }
            }
        }
    }
}
