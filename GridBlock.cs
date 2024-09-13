using GridBlock.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock;

public class GridBlock : Mod {
    public override void Load() {
        On_Player.TeleportationPotion += TeleportationPotionWorkaround;
    }

    public override void Unload() {
        On_Player.TeleportationPotion -= TeleportationPotionWorkaround;
    }

    private void TeleportationPotionWorkaround(Terraria.On_Player.orig_TeleportationPotion orig, Terraria.Player self) {
        if (GridBlockWorld.Instance.Chunks is not GridMap2D<GridBlockChunk> chunks) {
            orig(self);
            return;
        }

        var buffer = new List<GridBlockChunk>();

        // get all unlocked chunks
        for (var i = 0; i < chunks.Length; i++) {
            var chunk = chunks.GetById(i);
            if (chunk.IsUnlocked) {
                buffer.Add(chunk);
            }
        }

        if (buffer.Count <= 0)
            return;

        var tileCoord = self.Center.ToTileCoordinates();
        var spotRng = new WeightedRandom<Point>();
        
        // find non-occupied cell
        for (var att = 0; att < 100; att++) {
            var target = buffer[Main.rand.Next(buffer.Count)];
            for (var x = 0; x < chunks.CellSize - 2; x++)
                for (var y = 0; y < chunks.CellSize - 2; y++) {
                    var chunkCoord = chunks.ToChunkCoord(target.Id);
                    var coord = new Point(chunkCoord.X * chunks.CellSize + x, chunkCoord.Y * chunks.CellSize + y);
                    if (!WorldGen.SolidOrSlopedTile(coord.X, coord.Y) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y + 1) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y + 2) &&
                        !WorldGen.SolidOrSlopedTile(coord.X, coord.Y + 1) &&
                        !WorldGen.SolidOrSlopedTile(coord.X, coord.Y + 2)) {

                        // fun
                        spotRng.Add(coord);
                        break;
                    }
                }

            if (spotRng.elements.Count <= 0) 
                continue;

            tileCoord = spotRng.Get();
        }

        // attempt to find ground when needed
        for (var k = 1; k < 1000; k++) {
            var checkCoord = tileCoord + new Point(0, k);
            if (WorldGen.SolidOrSlopedTile(checkCoord.X, checkCoord.Y) || WorldGen.SolidOrSlopedTile(checkCoord.X + 1, checkCoord.Y)
                || (chunks.GetByTileCoord(checkCoord)?.IsUnlocked == false)) {
                tileCoord = checkCoord + new Point(0, -4);
                break;
            }
        }

        self.Teleport(tileCoord.ToWorldCoordinates(14, 24), 2);
    }
}
