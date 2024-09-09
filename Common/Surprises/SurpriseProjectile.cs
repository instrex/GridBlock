using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Surprises;

public abstract class SurpriseProjectile : ModProjectile {
    public override string Texture => "Terraria/Images/Projectile_1";

    /// <summary>
    /// If set to true, chunk will be decided based on current projectile position.
    /// </summary>
    public bool UseContinousChunkDetection { get; set; }

    public override void SetDefaults() {
        Projectile.friendly = true;
        Projectile.hide = true;
        Projectile.tileCollide = false;
    }

    public GridBlockChunk Chunk => UseContinousChunkDetection ?
        GridBlockWorld.Instance.Chunks.GetByWorldPos(Projectile.Center) :
        GridBlockWorld.Instance.Chunks.GetById((int)Projectile.ai[0]);

    public static Projectile Spawn<T>(Player player, GridBlockChunk chunk) where T : SurpriseProjectile {
        var proj = Projectile.NewProjectileDirect(null, player.Center, Vector2.Zero, ModContent.ProjectileType<T>(), 0, 0, player.whoAmI, chunk.Id);
        proj.ai[0] = chunk.Id;
        proj.netUpdate = true;
        return proj;
    }
}