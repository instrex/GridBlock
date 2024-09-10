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

    /// <summary>
    /// Internal id used for saving/loading.
    /// </summary>
    public string Id => GetType().Name;

    public void Load(Mod mod) { }
    public void Unload() { }

    /// <summary>
    /// Is this surprise bad?
    /// </summary>
    public virtual bool IsNegative => false;

    /// <summary>
    /// Dynamically calculate the likelyhood for this event to happen.
    /// </summary>
    public virtual float GetWeight(Player player, GridBlockChunk chunk) => 1.0f;

    /// <summary>
    /// Checks conditions before adding this to event selector.
    /// </summary>
    public virtual bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return true;
    }

    /// <summary>
    /// Triggers a funny surprise!
    /// </summary>
    public abstract void Trigger(Player player, GridBlockChunk chunk);
}
