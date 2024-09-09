using GridBlock.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace GridBlock.Common.Surprises;

public abstract class AmbushSurpriseProjectile : SurpriseProjectile {
    public int NpcType { get; set; }
    public int SpawnInterval { get; set; } = 10;

    public virtual int GetNpcType() => NpcType;

    public virtual bool CanSpawnNpcAtLocation(Point tileCoord, Vector2 worldPos, int npcType) {
        var size = new Point(ContentSamples.NpcsByNetId[npcType].width / 16, ContentSamples.NpcsByNetId[npcType].height / 16);

        // check the rect for free tiles
        for (var x = tileCoord.X; x < tileCoord.X + size.X; x++)
            for (var y = tileCoord.Y; y < tileCoord.Y + size.Y; y++) {
                if (WorldGen.SolidTile(x, y)) return false;
            }

        return true;
    }

    public virtual void OnNpcSpawned(NPC npc) { }

    public override void AI() {
        if (Projectile.ai[1]++ > 5) {
            Projectile.ai[1] = 0;

            for (var i = 0; i < 1000; i++) {
                var tileCoord = Chunk.TileCoord + new Point(Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize), Main.rand.Next(GridBlockWorld.Instance.Chunks.CellSize));
                var npcType = GetNpcType();
                if (CanSpawnNpcAtLocation(tileCoord, tileCoord.ToWorldCoordinates(), npcType)) {
                    var npc = NPC.NewNPCDirect(Projectile.GetSource_FromThis(), tileCoord.ToWorldCoordinates(), npcType);
                    npc.netUpdate = true;
                    OnNpcSpawned(npc);

                    break;
                }
            }
        }
    }
}