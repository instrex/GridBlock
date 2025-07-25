﻿using GridBlock.Common.Commands;
using GridBlock.Common.Costs;
using GridBlock.Common.Surprises;
using GridBlock.Common.UserInterface;
using GridBlock.Common.UserInterface.Animations;
using GridBlock.Content.Buffs;
using GridBlock.Content.Items;
using GridBlock.Content.Surprises;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    // struct used for storing content data about this chunk
    public record struct ChunkTileData(int EmptyTilesCount, int Placeable1x1SpotsCount, int WallSpotsCount, int LiquidCount, bool IsDungeon, bool IsGolemDungeon, bool HasPylons,
        float FullnessFactor) {

        /// <summary>
        /// Checks if horde events could be spawned in this chunk.
        /// </summary>
        public readonly bool SuitableForHordeEvents => EmptyTilesCount > 200 && LiquidCount < 100;
    }

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

    /// <summary>
    /// Special modifier applied to a chunk.
    /// </summary>
    public ChunkModifier Modifier { get; set; }

    // determenistic variables

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
    public bool ShouldBeSaved => IsUnlocked || IsUnlockCostCollapsed || Modifier != ChunkModifier.None;

    #region Content Analysis 

    ChunkTileData? _analysisData;

    ChunkTileData RunTileAnalysis() {
        var data = new ChunkTileData();

        var tileNum = 0;

        // check all nearby tiles
        for (var x = TileCoord.X; x <= TileCoord.X + GridBlockWorld.Instance.Chunks.CellSize; x++) {
            for (var y = TileCoord.Y; y <= TileCoord.Y + GridBlockWorld.Instance.Chunks.CellSize; y++) {
                var tile = Framing.GetTileSafely(x, y);
                if (tile.HasTile)
                    tileNum++;

                // counts empty spots
                if (!tile.HasTile && tile.LiquidType == 0)
                    data.EmptyTilesCount++;

                // counts liquid tiles
                if (tile.LiquidType != 0 && tile.LiquidAmount > 50)
                    data.LiquidCount++;

                // count spots with walls
                if (tile.WallType != 0)
                    data.WallSpotsCount++;

                // count spots for placeable torches/1x1 tiles
                if (!tile.HasTile && Framing.GetTileSafely(x, y + 1).HasTile)
                    data.Placeable1x1SpotsCount++;

                if (tile.HasTile) {
                    // check if chunk contains dungeon bricks
                    if (tile.TileType is TileID.BlueDungeonBrick or TileID.GreenDungeonBrick or TileID.PinkDungeonBrick)
                        data.IsDungeon = true;

                    // check if golem bricks are there
                    if (tile.TileType == TileID.LihzahrdBrick)
                        data.IsGolemDungeon = true;

                    // check for pylons
                    if (tile.TileType == TileID.TeleportationPylon)
                        data.HasPylons = true;
                }
            }
        }

        // calculate fullness factor
        data.FullnessFactor = tileNum / (float)(GridBlockWorld.Instance.Chunks.CellSize * GridBlockWorld.Instance.Chunks.CellSize);

        // cache the results
        return data;
    }

    public ChunkTileData ContentAnalysis {
        get {
            if (_analysisData is not ChunkTileData data) {
                _analysisData = data = RunTileAnalysis();
            }

            return data;
        }
    }

    public int EmptyTileAmount => ContentAnalysis.EmptyTilesCount;

    #endregion

    /// <summary>
    /// Collapse unlock cost for this chunk.
    /// </summary>
    public void CollapseUnlockCost(bool useSeededRandom = true) {
        if (IsUnlocked || IsUnlockCostCollapsed)
            return;

        var gridWorld = ModContent.GetInstance<GridBlockWorld>();
        var pool = CostPoolGenerator.GetPool(Group, this, useSeededRandom ? gridWorld.GridSeed + Id : null);

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

            pool.elements.RemoveAll(p => p.Item1.Type == neighbour.UnlockCost.type);
        }

        var modifierRng = useSeededRandom ? new UnifiedRandom(gridWorld.GridSeed + Id * 3) : Main.rand;

        // decide on modifiers
        Modifier = ChunkModifier.None;
        if (Group != CostGroup.PaidReward) {
            // 5% to make chunk mysterious
            if (modifierRng.NextFloat() <= 0.05f) {
                Modifier |= ChunkModifier.Mystery;
            }

            // 7.5% to gain a dice
            if (modifierRng.NextFloat() <= 0.075f) {
                Modifier |= ChunkModifier.FreeDice;
            }

            // 10% chance to get a discount, or price hike
            if (modifierRng.NextFloat() <= 0.1f) {
                var epic = Main.rand.NextFloat() < 0.5f;
                Modifier |= Main.rand.NextBool() ?
                    (epic ? ChunkModifier.Discount50 : ChunkModifier.Discount25) :
                    (epic ? ChunkModifier.PriceIncrease50 : ChunkModifier.PriceIncrease25);
            }
        }

        // decide the item
        IsUnlockCostCollapsed = true;
        UnlockCost = pool.Get()
            .ToItem();
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
    /// Gets adjusted unlock cost.
    /// </summary>
    public float GetCostModifier(Player player) {
        var mod = 1.0f;

        if (player.HasBuff<CostIncreaseBuff>())
            mod += 1.0f;

        if (player.HasBuff<SaleBuff>())
            mod -= 0.5f;

        if (player.HasBuff<FomoBuff>() && Group == CostGroup.PaidReward)
            mod -= 0.75f;

        if (Modifier.HasFlag(ChunkModifier.Discount50))
            mod -= 0.5f;

        if (Modifier.HasFlag(ChunkModifier.Discount25))
            mod -= 0.25f;

        if (Modifier.HasFlag(ChunkModifier.PriceIncrease25))
            mod += 0.25f;

        if (Modifier.HasFlag(ChunkModifier.PriceIncrease50))
            mod += 0.5f;

        return Math.Clamp(mod, 0, 5);
    }

    /// <summary>
    /// Called when any player is near the chunk.
    /// </summary>
    public void Update(Player player) {
        if (UnlockCost is null)
            return;

        if (Group == CostGroup.PaidReward) {
            UnlockCost.type = ItemID.GoldCoin;
            UnlockCost.stack = Main.hardMode ? 35 : 15;
            UnlockCost.value = Item.buyPrice(gold: UnlockCost.stack);

            return;
        }

        if (UnlockCost.IsACoin) {
            var stack = UnlockCost.stack;
            UnlockCost.value = stack * (UnlockCost.type switch {
                ItemID.SilverCoin => 100,
                ItemID.GoldCoin => 100 * 100,
                ItemID.PlatinumCoin => 100 * 100 * 100,
                _ => 1
            });
        }
    }

    bool TryCheckInventory(Item[] items, int limit, ref int count, bool ignoreIndex58 = false) {
        for (int i = 0; i < items.Length; i++) {
            if (ignoreIndex58 && i == 58)
                continue;

            Item item = items[i];
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

        var mod = GetCostModifier(player);

        if (UnlockCost.IsACoin) {
            storageContext = 0;
            return player.CanAfford((int)(UnlockCost.value * mod));
        }

        var itemCount = 0;

        var requiredStack = (int)Math.Max(1, Math.Floor(UnlockCost.stack * mod));

        // check inventory
        if (TryCheckInventory(player.inventory, requiredStack, ref itemCount, true)) {
            storageContext = 0;
            return true;
        }

        // check piggy bank
        if (TryCheckInventory(player.bank.item, requiredStack, ref itemCount)) {
            storageContext = 1;
            return true;
        }

        // check safe
        if (TryCheckInventory(player.bank2.item, requiredStack, ref itemCount)) {
            storageContext = 2;
            return true;
        }

        // check defender's forge
        if (TryCheckInventory(player.bank3.item, requiredStack, ref itemCount)) {
            storageContext = 3;
            return true;
        }

        // check void vault
        if (TryCheckInventory(player.bank4.item, requiredStack, ref itemCount)) {
            storageContext = 4;
            return true;
        }

        return false;
    }

    public void ConsumeUnlockRequirements(Player player) {
        if (UnlockCost is null)
            return;

        var mod = GetCostModifier(player);

        if (UnlockCost.IsACoin) {
            player.PayCurrency((int)(UnlockCost.value * mod));
            return;
        }

        var banks = player.inventory.Concat(player.bank.item)
            .Concat(player.bank2.item)
            .Concat(player.bank3.item)
            .Concat(player.bank4.item)
            .ToArray();

        var cost = Math.Max(1, Math.Floor(UnlockCost.stack * mod));
        for (var i = 0; i < cost; i++) {
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
    /// Attempts to reroll this chunk's unlock requirement.
    /// </summary>
    public void Reroll() {
        var oldCostType = UnlockCost?.type ?? -1;
        for (var i = 0; i < 10; i++) {
            IsUnlockCostCollapsed = false;
            CollapseUnlockCost(useSeededRandom: false);

            // attempt to get a unique cost
            if (UnlockCost.type != oldCostType)
                break;
        }
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

        if (Group == CostGroup.PaidReward) {
            // trigger reward surprise
            ModContent.GetInstance<RewardSurprise>().Trigger(player, this);
            return;
        }

        if (Modifier.HasFlag(ChunkModifier.FreeDice)) {
            player.QuickSpawnItem(player.GetSource_FromThis(), ModContent.ItemType<ChunkDice>());
        }

        if (SetNextSurpriseCommand.TryGetPendingSurprise(out var debugSurprise)) {
            TriggerSurpriseWithFx(debugSurprise, player);
            return;
        }

        if (!triggerSurprises || Main.rand.NextFloat() > 0.5f)
            return;

        var surprises = ModContent.GetContent<GridBlockSurprise>()
            .Where(s => s.CanBeTriggered(player, this));

        var eventRng = new WeightedRandom<GridBlockSurprise>(GridBlockWorld.Instance.GridSeed + Id * 2);
        foreach (var surprise in surprises) {
            var weight = surprise.GetWeight(player, this);

            // avoid repetition
            if (Array.FindIndex(player.GetModPlayer<GridBlockPlayer>().surpriseHistory, i => i == surprise.Id) != -1) {
                weight *= 0;
            }

            if (weight <= 0) continue;

            eventRng.Add(surprise, weight * (surprise.IsNegative ? 0.85f : 1.0f));
        }

        if (eventRng.elements.Count > 0) {
            TriggerSurpriseWithFx(eventRng.Get(), player);
        }
    }

    void TriggerSurpriseWithFx(GridBlockSurprise surprise, Player player) { 
        surprise.Trigger(player, this);

        // save to history stack
        player.GetModPlayer<GridBlockPlayer>().PushSurpriseHistory(surprise.Id);

        // hide other animations
        foreach (var anim in GridBlockUi.Animations.Active.OfType<SurpriseTextAnimation>()) {
            anim.Lifetime = MathF.Max(anim.Lifetime, 60 * 5);
        }

        var localizationKey = $"Mods.GridBlock.Surprises.{surprise.GetType().Name}";
        var messageLocalizationKey = $"{localizationKey}.Message";
        var text = Language.GetTextValue(localizationKey);

        // display funny text
        var origin = (WorldCoordTopLeft + new Vector2(GridBlockWorld.Instance.Chunks.CellSize * 16 * 0.5f)).ToPoint();

        var message = Language.GetTextValue(messageLocalizationKey);

        var textColor = surprise.IsNegative ? Color.Red : Color.Gold;

        GridBlockUi.Animations.Active.Add(new SurpriseTextAnimation {
            Title = string.Concat(text, surprise.IsNegative ? "..." : "!"),
            TitleColor = textColor,
            Description = message == messageLocalizationKey ? null : message
        });

        var chatMessage = Language.GetTextValue("Mods.GridBlock.SurpriseTrigger", player.name, $"[c/{textColor.Hex3()}:{text}]");
        Main.NewText(chatMessage, Color.White);
    }
}
