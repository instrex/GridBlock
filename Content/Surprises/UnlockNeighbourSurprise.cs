using GridBlock.Common.Surprises;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;
using Terraria.Localization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GridBlock.Content.Surprises;

public class UnlockNeighbourSurprise : GridBlockSurprise {
    static bool CheckChunk(Point chunkCoord, out GridBlockChunk chunk) {
        chunk = GridBlockWorld.Instance.Chunks.GetByChunkCoord(chunkCoord);
        return chunk != null && !chunk.IsUnlocked && chunk.Group != Common.Costs.CostGroup.PaidReward;
    }

    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.3f;
    }

    // can trigger if at least 1 neighbour is not unlocked yet
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return CheckChunk(chunk.ChunkCoord + new Point(1, 0), out _) 
            || CheckChunk(chunk.ChunkCoord + new Point(-1, 0), out _)
            || CheckChunk(chunk.ChunkCoord + new Point(0, 1), out _)
            || CheckChunk(chunk.ChunkCoord + new Point(0, -1), out _);
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var neighboursToConsider = new List<GridBlockChunk>();
        if (CheckChunk(chunk.ChunkCoord + new Point(-1, 0), out var left))
            neighboursToConsider.Add(left);

        if (CheckChunk(chunk.ChunkCoord + new Point(1, 0), out var right))
            neighboursToConsider.Add(right);

        if (CheckChunk(chunk.ChunkCoord + new Point(0, 1), out var bottom))
            neighboursToConsider.Add(bottom);

        if (CheckChunk(chunk.ChunkCoord + new Point(0, -1), out var top))
            neighboursToConsider.Add(top);

        if (neighboursToConsider.Count == 0) {
            GridBlockWorld.Instance.Mod.Logger.Warn("UnlockNeigbhourSurprise failed, no neighbouring chunks.");
            return;
        }

        var chosenOne = neighboursToConsider[Main.rand.Next(neighboursToConsider.Count)];
        chosenOne.Unlock(player);
    }
}
