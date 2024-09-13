using GridBlock.Common.Surprises;
using Terraria;
using GridBlock.Common;
using System.Linq;
using Terraria.ID;
using GridBlock.Common.Hacks;

namespace GridBlock.Content.Surprises;

public class RandomChunkTripSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) => GridBlockWorld.Instance.Chunks
        .GetAll(c => !c.IsUnlocked && c.Group != Common.Costs.CostGroup.Expensive)
        .Any();

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var chunks = GridBlockWorld.Instance.Chunks.GetAll(c => !c.IsUnlocked && c.Group != Common.Costs.CostGroup.Expensive)
            .ToList();

        while (true) {
            var targetChunk = chunks[Main.rand.Next(chunks.Count)];
            targetChunk.IsUnlocked = true;

            if (TeleportationItemsHack.TryGetRandomPointInChunk(targetChunk, out var tileCoord)) {
                targetChunk.Unlock(player, false, true);

                player.PotionOfReturnOriginalUsePosition = tileCoord.ToWorldCoordinates(14, 24);
                player.PotionOfReturnHomePosition = player.Bottom;
                NetMessage.SendData(MessageID.PlayerControls, -1, player.whoAmI, null, player.whoAmI, 0f, 0f, 0f, 0, 0, 0);

                break;
            }
        }
    }
}
