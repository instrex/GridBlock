using GridBlock.Common.Surprises;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridBlock.Content.Surprises;

public class UnlockNeighbourSurprise : GridBlockSurprise {
    static bool CheckChunk(Point chunkCoord) {
        var chunk = GridBlockWorld.Instance.Chunks.GetByChunkCoord(chunkCoord);
        return chunk != null && !chunk.IsUnlocked && chunk.Group != Common.Costs.CostGroup.Expensive;
    }

    // can trigger if at least 1 neighbour is not unlocked yet
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return CheckChunk(chunk.ChunkCoord + new Point(1, 0)) 
            || CheckChunk(chunk.ChunkCoord + new Point(-1, 0))
            || CheckChunk(chunk.ChunkCoord + new Point(0, 1))
            || CheckChunk(chunk.ChunkCoord + new Point(0, -1));
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var neighboursToConsider = new List<GridBlockChunk>();
        var chunks = GridBlockWorld.Instance.Chunks;
        for (var i = 0; i < 2; i++) {
            var top = chunks.GetByChunkCoord(chunk.ChunkCoord + new Point(i * 2 - 1, 0));
            if (top != null && CheckChunk(top.ChunkCoord)) neighboursToConsider.Add(top);

            var btm = chunks.GetByChunkCoord(chunk.ChunkCoord + new Point(0, i * -2 - 1));
            if (btm != null && CheckChunk(btm.ChunkCoord)) neighboursToConsider.Add(btm);
        }

        var chosenOne = neighboursToConsider[Main.rand.Next(neighboursToConsider.Count)];
        chosenOne.Unlock(player);
    }
}
