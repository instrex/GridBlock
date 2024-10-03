using GridBlock.Common.Costs;
using GridBlock.Common.Surprises;
using GridBlock.Content.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Common;

// chunk data
public class GridBlockChunk(int Id) {
    public int Id { get; init; } = Id;

    // state variables (should be saved)

    /// <summary>
    /// Checks if the chunk is safe to traverse.
    /// </summary>
    public bool IsUnlocked { get; set; }

    /// <summary>
    /// Checks if chunk has its reward decided.
    /// </summary>
    public bool IsUnlockCostCollapsed { get; set; }

    /// <summary>
    /// Item cost of this chunk to unlock.
    /// </summary>
    public Item UnlockCost { get; set; }

    // determenistic variables

    int? _emptyTileAmount;

    public int EmptyTileAmount {
        get {
            if (_emptyTileAmount is not int emptyTileAmount) {
                emptyTileAmount = 0;

                // check all nearby tiles
                for (var x = TileCoord.X; x <= TileCoord.X + GridBlockWorld.Instance.Chunks.CellSize; x++) {
                    for (var y = TileCoord.Y; y <= TileCoord.Y + GridBlockWorld.Instance.Chunks.CellSize; y++) {
                        if (!Framing.GetTileSafely(x, y).HasTile)
                            emptyTileAmount++;
                    }
                }

                // cache the result
                _emptyTileAmount = emptyTileAmount;
            }

            return emptyTileAmount;
        }
    }

    /// <summary>
    /// Cost group this chunk is assigned to.
    /// </summary>
    public CostGroup Group { get; set; }

    /// <summary>
    /// Only chunks with altered state should be saved.
    /// Currently, it's for following reasons:
    /// <list type="bullet">
    /// <item> The chunk is unlocked, meaning this progress has to persist. </item>
    /// <item> <see cref="UnlockCost"/> is decided. </item>
    /// <item> Surprise flags or other ratios are adjusted, as this process is done only during worldgen. </item>
    /// </list>
    /// </summary>
    public bool ShouldBeSaved => IsUnlocked || IsUnlockCostCollapsed;

    /// <summary>
    /// Collapse unlock cost for this chunk.
    /// </summary>
    public void CollapseUnlockCost() {
        if (IsUnlocked || IsUnlockCostCollapsed)
            return;

        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        var pool = CostPoolGenerator.GetPool(Group, this, gridWorld.GridSeed + Id);

        List<GridBlockChunk> neighbours = [];
        for (var i = -1; i < 2; i++) {
            for(var k = -1; k < 2; k++) {
                if (i == 0 && k == 0) 
                    continue;

                var coord = gridWorld.Chunks.ToChunkCoord(Id);
                neighbours.Add(gridWorld.Chunks.GetByChunkCoord(coord + new Point(i, k)));
            }
        }

        // adjust weights so that items are duplicated less
        foreach (var neighbour in neighbours.Where(n => n != null && n.IsUnlockCostCollapsed && n.UnlockCost != null)) {
            if (pool.elements.Count <= 1)
                break;

            pool.elements.RemoveAll(p => p.Item1.type == neighbour.UnlockCost.type);
        }

        // decide the item
        IsUnlockCostCollapsed = true;
        UnlockCost = pool.Get();
    }

    /// <summary>
    /// Collapse neighbours unlock costs.
    /// </summary>
    public void CollapseNeighboursUnlockCost() {
        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        for (var i = -1; i < 2; i++) {
            for (var k = -1; k < 2; k++) {
                if (i == 0 && k == 0)
                    continue;

                var coord = gridWorld.Chunks.ToChunkCoord(Id);
                gridWorld.Chunks.GetByChunkCoord(coord + new Point(i, k))?.CollapseUnlockCost();
            }
        }
    }

    /// <summary>
    /// Called when any player is near the chunk.
    /// </summary>
    public void Update(Player player) {
        if (Group == CostGroup.Expensive && UnlockCost != null) {
            var plr = player.GetModPlayer<GridBlockPlayer>();
            UnlockCost.stack = (int)((Main.hardMode ? CostPoolGenerator.RewardChunkBasePriceHardmode :  CostPoolGenerator.RewardChunkBasePrice) 
                * (1f + plr.RichChunkRewards.Count(i => RewardSurpriseProjectile.OneTimeRewards.Contains(i.type)) * CostPoolGenerator.RewardChunkIncrease));
        }
    }

    bool TryCheckInventory(Item[] items, int limit, ref int count) {
        foreach (var item in items) {
            if (item.type == UnlockCost.type) {
                if ((count += item.stack) >= limit) {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckUnlockRequirementsForPlayer(Player player, out int storageContext) {
        storageContext = -1;

        if (UnlockCost is null)
            return false;

        if (UnlockCost.IsACoin) {
            storageContext = 0;
            return player.CanAfford(UnlockCost.value);
        }

        var itemCount = 0;

        // check inventory
        if (TryCheckInventory(player.inventory, UnlockCost.stack, ref itemCount)) {
            storageContext = 0;
            return true;
        }

        // check piggy bank
        if (TryCheckInventory(player.bank.item, UnlockCost.stack, ref itemCount)) {
            storageContext = 1;
            return true;
        }

        // check safe
        if (TryCheckInventory(player.bank2.item, UnlockCost.stack, ref itemCount)) {
            storageContext = 2;
            return true;
        }

        // check defender's forge
        if (TryCheckInventory(player.bank3.item, UnlockCost.stack, ref itemCount)) {
            storageContext = 3;
            return true;
        }

        // check void vault
        if (TryCheckInventory(player.bank4.item, UnlockCost.stack, ref itemCount)) {
            storageContext = 4;
            return true;
        }

        return false;
    }

    public void ConsumeUnlockRequirements(Player player) {
        if (UnlockCost is null)
            return;

        if (UnlockCost.IsACoin) {
            player.PayCurrency(UnlockCost.value);
            return;
        }

        var banks = player.inventory.Concat(player.bank.item)
            .Concat(player.bank2.item)
            .Concat(player.bank3.item)
            .Concat(player.bank4.item)
            .ToArray();

        for (var i = 0; i < UnlockCost.stack; i++) {
            var index = Array.FindIndex(banks, x => x.type == UnlockCost.type);

            if (index == -1) {
                GridBlockWorld.Instance.Mod.Logger.Warn($"Attempted to consume item '{UnlockCost.Name}', which the player didn't have...");
                break;
            }

            var item = banks[index];
            item.stack--;

            if (item.stack <= 0)
                item.TurnToAir();
        }
    }

    /// <summary>
    /// Returns this chunk's location in tile-space.
    /// </summary>
    public Point TileCoord {
        get {
            var chunkCoord = GridBlockWorld.Instance.Chunks.ToChunkCoord(Id);
            var size = GridBlockWorld.Instance.Chunks.CellSize;
            return new(chunkCoord.X * size, chunkCoord.Y * size);
        }
    }

    public Point ChunkCoord => GridBlockWorld.Instance.Chunks.ToChunkCoord(Id);
    public Vector2 WorldCoordTopLeft => TileCoord.ToWorldCoordinates(0, 0);

    public Rectangle WorldBounds => new((int)WorldCoordTopLeft.X, (int)WorldCoordTopLeft.Y, GridBlockWorld.Instance.Chunks.CellSize * 16, GridBlockWorld.Instance.Chunks.CellSize * 16);

    /// <summary>
    /// Calculate CostGroup for a given chunk.
    /// </summary>
    public static CostGroup CalculateGroup(GridMap2D<GridBlockChunk> map, UnifiedRandom gridRng, GridBlockChunk chunk, int id, out bool unlocked) {
        var chunkCoord = map.ToChunkCoord(id);
        var normalizedCoord = new Vector2(chunkCoord.X / (float)map.Bounds.X, chunkCoord.Y / (float)map.Bounds.Y);
        var dist = MathF.Abs((normalizedCoord.X - 0.5f) / 0.5f);
        unlocked = false;

        // unlock small area
        var spawnChunkCoord = new Point(Main.spawnTileX / map.CellSize, Main.spawnTileY / map.CellSize);

        var spawnDistX = Math.Abs(spawnChunkCoord.X - chunkCoord.X);
        var spawnDistY = Math.Abs(spawnChunkCoord.Y - chunkCoord.Y);

        // clear 3x3 area around spawn point
        if (spawnDistX <= 1 && spawnDistY <= 1) {
            unlocked = true;
            return CostGroup.Beginner;
        }

        // make sure dungeon chunks are accessible at all times (i dont think this is needed anymore)
        // if (!CheckRegionSafety(new(chunkCoord.X * map.CellSize, chunkCoord.Y * map.CellSize, map.CellSize, map.CellSize))) {
        //     return CostGroup.Common;
        // }

        // make 12x6 area around spawn substantially easier to unlock
        if (spawnDistX <= 6 && spawnDistY <= 3) {
            return CostGroup.Beginner;
        }

        // make 20x12 area around spawn slightly easier to unlock
        if (spawnDistX <= 10 && spawnDistY <= 8) {
            return CostGroup.Common;
        }

        // check for empty tiles first
        if (gridRng.NextFloat() < 0.05f && chunk.EmptyTileAmount > 200) {
            return CostGroup.Expensive;
        }

        if (dist < 0.75f) {
            return normalizedCoord.Y switch {
                > 0.8f => gridRng.NextFloat() < 0.025f ? CostGroup.Adventure : CostGroup.Advanced,
                > 0.25f => CostGroup.Advanced,
                _ => CostGroup.Common
            };
        }

        return gridRng.NextFloat() < 0.03f ? CostGroup.Adventure : CostGroup.Advanced;
    }

    /// <summary>
    /// Attempts to unlock this chunk.
    /// </summary>
    public void Unlock(Player player, bool triggerSurprises = true, bool noCost = false) {
        // consume unlock ingredients
        if (!noCost && player != null && UnlockCost != null) 
            ConsumeUnlockRequirements(player);
        
        SoundEngine.PlaySound(SoundID.Unlock);

        // set the flag
        CollapseNeighboursUnlockCost();
        IsUnlocked = true;

        if (Group == CostGroup.Expensive) {
            // trigger reward surprise
            ModContent.GetInstance<RewardSurprise>().Trigger(player, this);
            return;
        }

        if (!triggerSurprises || Main.rand.NextFloat() > 0.5f)
            return;

        var surprises = ModContent.GetContent<GridBlockSurprise>()
            .Where(s => s.CanBeTriggered(player, this));

        var surpriseHistory = player.GetModPlayer<GridBlockPlayer>().SurpriseHistory;
        var eventRng = new WeightedRandom<GridBlockSurprise>(GridBlockWorld.Instance.GridSeed + Id * 2);
        foreach (var surprise in surprises) {
            var weight = surprise.GetWeight(player, this);

            // scale weight based on how recently the event triggered
            for (var i = 0; i < surpriseHistory.Count; i++) {
                if (surpriseHistory[i] == surprise) {
                    weight *= 1f - i / 5f;
                    continue;
                }
            }

            if (weight <= 0) continue;

            eventRng.Add(surprise, weight * (surprise.IsNegative ? 0.75f : 1.0f));
        }

        if (eventRng.elements.Count > 0) {
            var surprise = eventRng.Get();
            surprise.Trigger(player, this);

            // save surprise to history
            player.GetModPlayer<GridBlockPlayer>().PushSurprise(surprise);

            var text = string.Concat(Language.GetTextValue($"Mods.GridBlock.Surprises.{surprise.GetType().Name}"), surprise.IsNegative ? "..." : "!");

            // display funny text
            var origin = (WorldCoordTopLeft + new Vector2(GridBlockWorld.Instance.Chunks.CellSize * 16 * 0.5f)).ToPoint();
            CombatText.NewText(new(origin.X - 16, origin.Y + 16, 32, 32), surprise.IsNegative ? Color.Red : Color.Gold, text, true);
            Main.NewText(text, surprise.IsNegative ? Color.Red : Color.Gold);
        }
    }


}
