using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Content.Surprises;

internal class StarFallSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) => !Main.dayTime;
    public override void Trigger(Player player, GridBlockChunk chunk) {
        Projectile.NewProjectileDirect(player.GetSource_FromThis(), chunk.WorldBounds.Center.ToVector2(), Vector2.Zero,
            ModContent.ProjectileType<StarFallSurpriseProjectile>(), 0, 0, player.whoAmI);
    }
}

public class StarFallSurpriseProjectile : SurpriseProjectile {
    public override void SetDefaults() {
        Projectile.tileCollide = false;
        Projectile.friendly = true;
        Projectile.timeLeft = 90;
    }

    public override void AI() {
        if (Projectile.ai[0] <= 0) {
            var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + new Vector2(Main.rand.NextFloat(-600, 600), Main.rand.NextFloat(-2900, -600)),
                new Vector2(Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(10, 15)), ProjectileID.FallingStar, 100, 2f, Projectile.owner);
            proj.friendly = false;
            proj.hostile = true; 

            Projectile.ai[0] = 0;
        }
    }
}