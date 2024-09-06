using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Globals;

public class GridBlockGlobalProjectile : GlobalProjectile {
    public override bool? CanCutTiles(Projectile projectile) {
        if (ModContent.GetInstance<GridBlockWorld>().Chunks?.GetByWorldPos(projectile.Center) is { IsUnlocked: false })
            return false;

        return null;
    }
}
