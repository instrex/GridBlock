using Terraria.ModLoader;

namespace GridBlock.Common.Globals;

public class GridBlockGlobalTile : GlobalTile {
    public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged) {
        if (ModContent.GetInstance<GridBlockWorld>().Chunks?.GetByTileCoord(new(i, j)) is GridBlockChunk chunk) {
            return chunk.IsUnlocked;
        }

        return true;
    }

    public override bool CanPlace(int i, int j, int type) {
        if (ModContent.GetInstance<GridBlockWorld>().Chunks?.GetByTileCoord(new(i, j)) is GridBlockChunk chunk) {
            return chunk.IsUnlocked;
        }

        return true;
    }
}
