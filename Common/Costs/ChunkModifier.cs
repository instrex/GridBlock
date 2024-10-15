using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridBlock.Common.Costs;

[Flags]
public enum ChunkModifier {
    None = 0,

    /// <summary>
    /// Unlocking this chunk will grant a dice.
    /// </summary>
    FreeDice = 1,

    /// <summary>
    /// Unlock cost is reduced by 25%
    /// </summary>
    Discount25 = 2,

    /// <summary>
    /// Unlock cost is reduced by 50%
    /// </summary>
    Discount50 = 4,

    /// <summary>
    /// Price is increased by 25%
    /// </summary>
    PriceIncrease25 = 8,

    /// <summary>
    /// Price is increased by 50%
    /// </summary>
    PriceIncrease50 = 16,

    /// <summary>
    /// Chunk price will be hidden...
    /// </summary>
    Mystery = 32,
}
