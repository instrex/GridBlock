using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace GridBlock.Common;

public class GridBlockWorld : ModSystem {
    public const string SAVE_VERSION = "1";

    // world-specific variables
    public GridMap2D<GridBlockChunk> Chunks { get; private set; }
    public string WorldVersion { get; private set; }

    public override void PostWorldGen() {
        WorldVersion = SAVE_VERSION;
        RegenerateChunks();
    }

    void RegenerateChunks() {
        var pool = CostPoolGenerator.GetPool(CostGroup.Common);

        Chunks = new GridMap2D<GridBlockChunk>(40, Main.maxTilesX, Main.maxTilesY);
        Chunks.Fill((map, id) => new(id) { UnlockCost = pool.Get() });
    }

    public override void PostUpdatePlayers() {
        if (PlayerInput.GetPressedKeys().Any(k => k == Microsoft.Xna.Framework.Input.Keys.G))
            RegenerateChunks();
    }
}
