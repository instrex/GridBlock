using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Terraria;

namespace GridBlock.Common;

// chunk data
public class GridBlockChunk(int Id) {
    public bool IsUnlocked { get; set; }

    public CostGroup Group;

    public Item UnlockCost;
}
