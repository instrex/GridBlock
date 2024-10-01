using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GridBlock.Common.Costs;
using GridBlock.Content.Surprises;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace GridBlock.Common;

public class GridBlockWorld : ModSystem {
    public const string SAVE_VERSION = "test-1";

    /// <summary>
    /// Tiles that contribute to "no surprise" status of the chunk.
    /// </summary>
    public static readonly HashSet<int> SafetyTileExclusionSet = [
        TileID.BlueDungeonBrick,
        TileID.GreenDungeonBrick,
        TileID.PinkDungeonBrick,
        TileID.LihzahrdBrick,
        TileID.LihzahrdAltar
    ];

    public static GridBlockWorld Instance => ModContent.GetInstance<GridBlockWorld>();

    // world-specific variables
    public GridMap2D<GridBlockChunk> Chunks { get; private set; }
    public string WorldVersion { get; private set; }
    public int GridSeed { get; private set; }

    readonly GridBlockUi _uiHelper = new();

    public override void PostWorldGen() {
        GridSeed = WorldGen.genRand.Next();
        WorldVersion = SAVE_VERSION;
        ResetChunks();
    }

    public override void OnWorldUnload() {
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
            var group = GridBlockChunk.CalculateGroup(map, chunkRng, chunk, id, out var unlocked);
            chunk.IsUnlocked  = unlocked;
            chunk.Group = group;

            // save chunks that need their neighbours collapsed
            if (unlocked) chunksToCollapseNeighboursFor.Add(chunk);

            // collapse cost immediately
            if (chunk.Group == CostGroup.Expensive) 
                chunk.CollapseUnlockCost();

            return chunk;
        });

        chunksToCollapseNeighboursFor.ForEach(c => c.CollapseNeighboursUnlockCost());
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        if (layers.FindIndex(l => l.Name == "Vanilla: Interface Logic 1") is int i and > -1) {
            layers.Insert(i, new LegacyGameInterfaceLayer("GridBlock: Chunks", () => {
                _uiHelper.Draw();
                return true;
            }));
        }
    }

    public override void SaveWorldData(TagCompound tag) {
        tag["GridVersion"] = WorldVersion;
        tag["GridSeed"] = GridSeed;

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

                if (chunk.IsUnlockCostCollapsed && chunk.Group != CostGroup.Expensive) {
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
