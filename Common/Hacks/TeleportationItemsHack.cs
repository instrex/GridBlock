using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common.Hacks; 

public class TeleportationItemsHack : ModSystem {
    public override void Load() {
        On_Player.TeleportationPotion += TeleportationPotion;
        On_Player.MagicConch += MagicConch;
        On_Player.DemonConch += DemonConch;
    }

    public override void Unload() {
        On_Player.TeleportationPotion -= TeleportationPotion;
        On_Player.MagicConch -= MagicConch;
        On_Player.DemonConch -= DemonConch;
    }

    private void DemonConch(On_Player.orig_DemonConch orig, Player self) {
        if (GridBlockWorld.Instance.Chunks is not GridMap2D<GridBlockChunk> chunks) {
            orig(self);
            return;
        }

        var unlockedChunks = chunks.GetAll(c => c.IsUnlocked).ToList();
        while (true) {
            var targetChunk = unlockedChunks.MaxBy(c => c.TileCoord.Y);

            // teleport successfully
            if (targetChunk != null && TryGetRandomPointInChunk(targetChunk, out var tileCoord)) {
                self.Teleport(tileCoord.ToWorldCoordinates(14, 24), TeleportationStyleID.DemonConch);
                break;
            }

            // retry with a different chunk...
            unlockedChunks.Remove(targetChunk);
        }
    }

    private void MagicConch(On_Player.orig_MagicConch orig, Player self) {
        if (GridBlockWorld.Instance.Chunks is not GridMap2D<GridBlockChunk> chunks) {
            orig(self);
            return;
        }

        var unlockedChunks = chunks.GetAll(c => c.IsUnlocked).ToList();
        while (true) {
            var targetChunk = self.position.X / 16 < Main.maxTilesX * 0.5f ? unlockedChunks.MaxBy(c => c.TileCoord.X)
            : unlockedChunks.MinBy(c => c.TileCoord.X);

            // teleport successfully
            if (targetChunk != null && TryGetRandomPointInChunk(targetChunk, out var tileCoord)) {
                self.Teleport(tileCoord.ToWorldCoordinates(14, 24), TeleportationStyleID.MagicConch);
                break;
            }

            // retry with a different chunk...
            unlockedChunks.Remove(targetChunk);
        }
    }

    private void TeleportationPotion(Terraria.On_Player.orig_TeleportationPotion orig, Terraria.Player self) {
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

        // find non-occupied cell
        for (var att = 0; att < 100; att++) {
            var target = buffer[Main.rand.Next(buffer.Count)];
            if (!TryGetRandomPointInChunk(target, out tileCoord))
                continue;
        }

        self.Teleport(tileCoord.ToWorldCoordinates(14, 24), 2);
    }

    bool TryGetRandomPointInChunk(GridBlockChunk chunk, out Point tileCoord) {
        var chunks = GridBlockWorld.Instance.Chunks;
        var spotRng = new WeightedRandom<Point>();
        tileCoord = Point.Zero;

        for (var x = 0; x < chunks.CellSize - 2; x++)
            for (var y = 0; y < chunks.CellSize - 2; y++) {
                var coord = chunk.TileCoord + new Point(x, y);
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
            return false;

        tileCoord = spotRng.Get();

        // attempt to find ground when needed
        for (var k = 1; k < 1000; k++) {
            var checkCoord = tileCoord + new Point(0, k);
            if (WorldGen.SolidOrSlopedTile(checkCoord.X, checkCoord.Y) || WorldGen.SolidOrSlopedTile(checkCoord.X + 1, checkCoord.Y)
                || (chunks.GetByTileCoord(checkCoord)?.IsUnlocked == false)) {
                tileCoord = checkCoord + new Point(0, -4);
                break;
            }
        }

        return true;
    }
}
