using Microsoft.Xna.Framework;
using Terraria;

namespace GridBlock.Common;

// chunk data
public class GridBlockChunk(int Id) {
    public Color Color = new Color[] { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Cyan }[Main.rand.Next(5)];
    public Item UnlockCost;
}
