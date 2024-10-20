using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using GridBlock.Common.UserInterface;
using GridBlock.Common.UserInterface.Animations;
using GridBlock.Content.Surprises;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace GridBlock.Common;

public class GridBlockWorld : ModSystem {
    public const string SAVE_VERSION = "test-2";

    public static GridBlockWorld Instance => ModContent.GetInstance<GridBlockWorld>();

    /// <summary>
    /// Amount of rerolls remaining.
    /// </summary>
    public int RerollCount { get; set; }

    /// <summary>
    /// Flags for each downed boss, signaling if rerolls from them have been obtained.
    /// </summary>
    public HashSet<string> BossRerollsObtained { get; set; } = [];

    /// <summary>
    /// Flag signalizing one-time hardmode changes.
    /// </summary>
    public bool AppliedHardmodeChanges { get; set; }

    /// <summary>
    /// Flag signalizing one-time post-plantera changes.
    /// </summary>
    public bool AppliedPlantmodeChanges { get; set; }

    // world-specific variables
    public GridMap2D<GridBlockChunk> Chunks { get; private set; }
    public string WorldVersion { get; private set; }
    public int GridSeed { get; private set; }

    readonly GridBlockUi _uiHelper = new();

    public override void PostWorldGen() {
        GridSeed = WorldGen.genRand.Next();
        WorldVersion = SAVE_VERSION;
        BossRerollsObtained = [];
        RerollCount = 2;
        ResetChunks();
    }

    public override void OnWorldUnload() {
        BossRerollsObtained = [];
        WorldVersion = "";
        GridSeed = 0;
        Chunks = null;
    }

    public void ResetChunks() {
        List<GridBlockChunk> chunksToCollapseNeighboursFor = [];

        var chunkRng = new UnifiedRandom(GridSeed);

        Chunks = new GridMap2D<GridBlockChunk>(40, Main.maxTilesX, Main.maxTilesY);
        Chunks.Fill((map, id) => {
            var chunk = new GridBlockChunk(id);
            var group = ChunkGroupGenerator.CalculateGroup(map, chunkRng, chunk, id, out var unlocked);
            chunk.IsUnlocked  = unlocked;
            chunk.Group = group;

            // save chunks that need their neighbours collapsed
            if (unlocked) chunksToCollapseNeighboursFor.Add(chunk);

            // collapse cost immediately
            if (chunk.Group == CostGroup.PaidReward) 
                chunk.CollapseUnlockCost();

            return chunk;
        });

        chunksToCollapseNeighboursFor.ForEach(c => c.CollapseNeighboursUnlockCost());
    }

    void ApplyHardMode() {
        var chunkRng = new UnifiedRandom(GridSeed);

        // reassign groups
        Chunks.Fill((map, id) => {
            var chunk = map.GetById(id);
            chunk.Group = ChunkGroupGenerator.CalculateGroup(map, chunkRng, chunk, id, out _);

            return chunk;
        });

        // do quickie reroll
        foreach (var chunk in Chunks.GetAll(c => c.IsUnlockCostCollapsed))
            chunk.Reroll();

        var playerChunks = Main.player.Where(p => p.active)
            .SelectMany(p => {
                var playerChunk = Chunks.GetByWorldPos(p.Center);

                var dangerChunks = new List<GridBlockChunk>();

                for (var x = -1; x <= 1; x++) {
                    for (var y = -1; y <= 1; y++) {
                        if (Chunks.GetByChunkCoord(playerChunk.ChunkCoord + new Point(x, y)) is GridBlockChunk ass)
                            dangerChunks.Add(ass);
                    }
                }

                return dangerChunks;
            });

        var closeCandidates = Chunks.GetAll(c => c.IsUnlocked && c.Group != CostGroup.Spawn && !c.ContentAnalysis.HasPylons)
            .Except(playerChunks)
            .ToList();

        var chunksToClose = closeCandidates.Count / 2;

        // now close 50% of the chunks and do a reroll on them
        for (var i = 0; i < chunksToClose; i++) {
            var victim = closeCandidates[Main.rand.Next(closeCandidates.Count)];
            closeCandidates.Remove(victim);

            // do a reroll
            victim.IsUnlocked = false;
            victim.Reroll();
        }

        // done
        AppliedHardmodeChanges = true;
    }

    void ApplyPlantMode() {
        // close all dungeon chunks
        foreach (var chunk in Chunks.GetAll(c => (c.IsUnlocked || c.IsUnlockCostCollapsed) && c.ContentAnalysis.IsDungeon)) {
            chunk.IsUnlocked = false;
            chunk.Reroll();
        }

        // done
        AppliedPlantmodeChanges = true;
    }

    public override void PreUpdateDusts() {
        if (GridBlockUi.IsHoveringChunk) {
            Main.LocalPlayer.mouseInterface = true;
            Main.blockMouse = true;
        }

        if (Main.hardMode && !AppliedHardmodeChanges) {
            GridBlockUi.Animations.Active.Add(new SurpriseTextAnimation {
                Title = Language.GetTextValue("Mods.GridBlock.HardmodeMessage.Title"),
                Description = Language.GetTextValue("Mods.GridBlock.HardmodeMessage.Description"),
                TitleColor = Color.Magenta
            });

            ApplyHardMode();
        }

        if (NPC.downedPlantBoss && !AppliedPlantmodeChanges) {
            ApplyPlantMode();
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        if (layers.FindIndex(l => l.Name == "Vanilla: Interface Logic 1") is int i and > -1) {
            layers.Insert(i, new LegacyGameInterfaceLayer("GridBlock: Chunks", () => {
                _uiHelper.Draw();
                return true;
            }));
        }
    }

    public override void UpdateUI(GameTime gameTime) {
        _uiHelper.Update();
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["GridVersion"] = WorldVersion;
        tag["GridSeed"] = GridSeed;
        tag[nameof(RerollCount)] = RerollCount;
        tag[nameof(AppliedHardmodeChanges)] = AppliedHardmodeChanges;
        tag[nameof(AppliedPlantmodeChanges)] = AppliedPlantmodeChanges;

        if (BossRerollsObtained != null) {
            tag[nameof(BossRerollsObtained)] = BossRerollsObtained.ToArray();
        }

        if (Chunks is null) {
            Mod.Logger.Warn("GridBlock Chunk data was null!");
            return;
        }

        List<TagCompound> savedChunks = [];
        for (var id = 0; id < Chunks.Length; id++) {
            var chunk = Chunks.GetById(id);
            if (chunk is null || !chunk.ShouldBeSaved) 
                continue;

            try {
                var chunkTag = new TagCompound { ["ChunkId"] = id };
                savedChunks.Add(chunkTag);

                if (chunk.IsUnlocked) {
                    // if it's unlocked, no other information is required
                    chunkTag[nameof(GridBlockChunk.IsUnlocked)] = true;
                    continue;
                }

                if (chunk.Modifier != ChunkModifier.None) {
                    chunkTag[nameof(GridBlockChunk.Modifier)] = (int)chunk.Modifier;
                }

                if (chunk.IsUnlockCostCollapsed && chunk.Group != CostGroup.PaidReward) {
                    if (chunk.UnlockCost is null) {
                        ModContent.GetInstance<GridBlock>().Logger.Warn("Attempted to save a chunk with collapsed unlock cost but no item instance? Weird...");
                    }

                    chunkTag[nameof(GridBlockChunk.IsUnlockCostCollapsed)] = true;
                    chunkTag[nameof(GridBlockChunk.UnlockCost)] = chunk.UnlockCost;
                }
            } catch (Exception ex) {
                Mod.Logger.Warn($"Failed to save chunk state at {id}!", ex);
            }
            
        }

        tag["GridChunks"] = savedChunks;
    }

    public override void LoadWorldData(TagCompound tag) {
        if (tag.TryGet<string>("GridVersion", out var version))
            WorldVersion = version;

        if (tag.TryGet<int>("GridSeed", out var seed))
            GridSeed = seed;

        RerollCount = tag.TryGet<int>(nameof(RerollCount), out var rerollCount) ? rerollCount : 2;
        BossRerollsObtained = tag.TryGet<string[]>(nameof(BossRerollsObtained), out var bossRerollsObtained) ? new(bossRerollsObtained) : [];
        AppliedHardmodeChanges = tag.TryGet<bool>(nameof(AppliedHardmodeChanges), out var appliedHardmodeChanges) && appliedHardmodeChanges;
        AppliedPlantmodeChanges = tag.TryGet<bool>(nameof(AppliedPlantmodeChanges), out var appliedPlantmodeChanges) && appliedPlantmodeChanges;

        // create determenistic chunks
        ResetChunks();

        if (tag.TryGet<List<TagCompound>>("GridChunks", out var chunks)) {
            foreach (var chunkTag in tag.Get<List<TagCompound>>("GridChunks")) {
                try {
                    var id = chunkTag.Get<int>("ChunkId");
                    if (Chunks.GetById(id) is not GridBlockChunk chunk) {
                        Mod.Logger.Warn($"Attempted to set non-existent chunk during loading. (Id: {id})");
                        continue;
                    }

                    // assign modifiers
                    if (chunkTag.TryGet<int>(nameof(GridBlockChunk.Modifier), out var modifierMask))
                        chunk.Modifier = (ChunkModifier)modifierMask;

                    // unlock the chunk if its set as unlocked
                    if (chunkTag.ContainsKey(nameof(GridBlockChunk.IsUnlocked))) {
                        chunk.IsUnlocked = true;
                        continue;
                    }

                    // set collapsed unlock
                    if (chunkTag.ContainsKey(nameof(GridBlockChunk.IsUnlockCostCollapsed))) {
                        var item = chunkTag.Get<Item>(nameof(GridBlockChunk.UnlockCost));
                        chunk.IsUnlockCostCollapsed = true;
                        chunk.UnlockCost = item;
                        continue;
                    }
                } catch (Exception ex) {
                    Mod.Logger.Error("Failed to load one of GridBlock chunks!", ex);
                }
            }

        } else {
            Mod.Logger.Warn("No GridBlock chunk data found...");
        }
    }
}
