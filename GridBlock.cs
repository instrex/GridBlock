using GridBlock.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock;

public class GridBlock : Mod {
    public override void Load() {
        On_Player.TeleportationPotion += TeleportationPotionWorkaround;
    }

    public override void Unload() {
        On_Player.TeleportationPotion -= TeleportationPotionWorkaround;
    }

    private void TeleportationPotionWorkaround(Terraria.On_Player.orig_TeleportationPotion orig, Terraria.Player self) {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();

        if (gridWorld.Chunks == null) {
            orig(self);
            return;
        }

        var buffer = new List<GridBlockChunk>();
        for (var i = 0; i < gridWorld.Chunks.Length; i++) {
            var chunk = gridWorld.Chunks.GetById(i);
            if (chunk.IsUnlocked) {
                buffer.Add(chunk);
            }
        }

        if (buffer.Count <= 0)
            return;

        var position = self.Center;

        for (var att = 0; att < 100; att++) {
            var target = buffer[Main.rand.Next(buffer.Count)];
            for (var x = 0; x < gridWorld.Chunks.CellSize; x++)
                for (var y = 0; y < gridWorld.Chunks.CellSize; y++) {
                    var chunkCoord = gridWorld.Chunks.ToChunkCoord(target.Id);
                    var coord = new Point(chunkCoord.X * gridWorld.Chunks.CellSize + x, chunkCoord.Y * gridWorld.Chunks.CellSize + y);
                    if (!WorldGen.SolidOrSlopedTile(coord.X, coord.Y) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y + 1) &&
                        !WorldGen.SolidOrSlopedTile(coord.X + 1, coord.Y + 2) &&
                        !WorldGen.SolidOrSlopedTile(coord.X, coord.Y + 1) &&
                        !WorldGen.SolidOrSlopedTile(coord.X, coord.Y + 2)) {

                        // fun
                        position = coord.ToWorldCoordinates(16, 16);
                    }
                }
        }

        self.Teleport(position, 2);
    }
}
