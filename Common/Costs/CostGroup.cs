namespace GridBlock.Common.Costs;

public enum CostGroup {
    Spawn,

    /// <summary>
    /// Cost of chunks around the spawn area. Items should be accessible right from the start.
    /// </summary>
    Beginner,

    /// <summary>
    /// Cost of chunks on surface. 
    /// </summary>
    Common,

    /// <summary>
    /// Cost of chunks in caves or specific tiles on surface.
    /// </summary>
    Advanced,

    /// <summary>
    /// Cost of world border chunks.
    /// </summary>
    Adventure,

    /// <summary>
    /// Special cost for chunks with reward event.
    /// </summary>
    PaidReward,
}
