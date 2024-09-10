using GridBlock.Common.Surprises;
using Terraria.Utilities;
using Terraria;
using GridBlock.Common;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace GridBlock.Content.Surprises;

public class UnlockNeighbourSurprise : GridBlockSurprise {
    // can trigger if at least 1 neighbour is not unlocked yet
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        for (var i = -1; i < 2; i++)
            for (var k = -1; k < 2; k++) {
                if (i == 0 && k == 0) continue;

                var neighbour = GridBlockWorld.Instance.Chunks.GetByChunkCoord(chunk.ChunkCoord + new Microsoft.Xna.Framework.Point(i, k));
                if (neighbour != null && !neighbour.IsUnlocked) return true;
            }

        return false;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var neighboursToConsider = new WeightedRandom<GridBlockChunk>();
        for (var i = -1; i < 2; i++)
            for (var k = -1; k < 2; k++)  {
                if (i == 0 && k == 0) continue;

                var neighbour = GridBlockWorld.Instance.Chunks.GetByChunkCoord(chunk.ChunkCoord + new Microsoft.Xna.Framework.Point(i, k));
                if (!neighbour.IsUnlocked && neighbour.Group != Common.Costs.CostGroup.Expensive) neighboursToConsider.Add(neighbour);
            }

        var chosenOne = neighboursToConsider.Get();
        chosenOne.Unlock(player);

        var origin = (chunk.WorldCoordTopLeft + new Vector2(GridBlockWorld.Instance.Chunks.CellSize * 16 * 0.5f)).ToPoint();
        CombatText.NewText(new(origin.X - 16, origin.Y + 75, 32, 2), Color.Gold * 0.75f,
            Language.GetTextValue($"Mods.GridBlock.Surprises.UnlockNeighbourSurprise.Tip"),
            true);
    }
}

