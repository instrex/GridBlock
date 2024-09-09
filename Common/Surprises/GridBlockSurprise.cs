using GridBlock.Content.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Surprises;

public abstract class GridBlockSurprise : ILoadable {
    /// <summary>
    /// Simple wrapper for suprises that just spawn projectiles (majority of them?)
    /// </summary>
    public class ProjectileSpawner<T> : GridBlockSurprise where T : SurpriseProjectile {
        public override void Trigger(Player player, GridBlockChunk chunk) => SurpriseProjectile.Spawn<T>(player, chunk);
    }

    public void Load(Mod mod) { }
    public void Unload() { }

    /// <summary>
    /// Triggers a funny surprise!
    /// </summary>
    public abstract void Trigger(Player player, GridBlockChunk chunk);
}
