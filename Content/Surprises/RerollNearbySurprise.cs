using GridBlock.Common;
using GridBlock.Common.Surprises;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace GridBlock.Content.Surprises;

public class RerollNearbySurprise : GridBlockSurprise {
    public override void Trigger(Player player, GridBlockChunk chunk) {
        for (var x = -3; x <= 3; x++) {
            for (var y = -3; y <= 3; y++) {
                if (GridBlockWorld.Instance.Chunks.GetByChunkCoord(chunk.ChunkCoord + new Microsoft.Xna.Framework.Point(x, y)) is not GridBlockChunk nearbyChunk)
                    continue;

                if (nearbyChunk.IsUnlocked || !nearbyChunk.IsUnlockCostCollapsed)
                    continue;

                nearbyChunk.Reroll();
            }
        }

        SoundEngine.PlaySound(SoundID.Item101);
    }
}