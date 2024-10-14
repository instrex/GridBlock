using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using System.Linq;
using Terraria.ID;
using GridBlock.Common.Hacks;

namespace GridBlock.Content.Surprises;

public class RandomChunkTripSurprise : GridBlockSurprise {
    static bool ValidChunkCheck(GridBlockChunk chunk) {
        var chunks = GridBlockWorld.Instance.Chunks;
        return !chunk.IsUnlocked && chunk.Group != Common.Costs.CostGroup.PaidReward &&
            chunk.ChunkCoord.X != 0 && chunk.ChunkCoord.X != chunks.Bounds.X - 1 &&
            chunk.ChunkCoord.Y != 0 && chunk.ChunkCoord.Y != chunks.Bounds.Y - 1;
    }

    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.4f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) => GridBlockWorld.Instance.Chunks
        .GetAll(ValidChunkCheck)
        .Any();

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var chunks = GridBlockWorld.Instance.Chunks.GetAll(ValidChunkCheck)
            .ToList();

        for (var i = 0; i < 10000; i++) {
            var targetChunk = chunks[Main.rand.Next(chunks.Count)];
            targetChunk.IsUnlocked = true;

            if (TeleportationItemsHack.TryGetRandomPointInChunk(targetChunk, out var tileCoord)) {
                targetChunk.Unlock(player, false, true);

                player.PotionOfReturnOriginalUsePosition = tileCoord.ToWorldCoordinates(14, 24);
                player.PotionOfReturnHomePosition = player.Bottom;
                NetMessage.SendData(MessageID.PlayerControls, -1, player.whoAmI, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);

                // player.QuickSpawnItem(player.GetSource_FromThis(), ItemID.RecallPotion);

                player.GetModPlayer<GridBlockPlayer>().PrepareRandomTripPortalTrigger(tileCoord.ToWorldCoordinates(14, 24), player.Bottom);

                return;
            }

            targetChunk.IsUnlocked = false;
        }

        GridBlockWorld.Instance.Mod.Logger.Warn("Couldn't find a place for 20-minute adventure!");
    }
}
