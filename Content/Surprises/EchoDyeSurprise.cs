using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Content.Surprises;

public class EchoDyeSurprise : GridBlockSurprise {
    public override bool IsNegative => true;
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return chunk.ContentAnalysis.FullnessFactor >= 0.5f;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        SoundEngine.PlaySound(SoundID.Item12, player.Center);
        for (var x = 0; x < GridBlockWorld.Instance.Chunks.CellSize; x++) {
            for (var y = 0; y < GridBlockWorld.Instance.Chunks.CellSize; y++) {
                var tileCoord = chunk.TileCoord + new Point(x, y);
                WorldGen.paintCoatTile(tileCoord.X, tileCoord.Y, PaintCoatingID.Echo, true);
                WorldGen.paintCoatWall(tileCoord.X, tileCoord.Y, PaintCoatingID.Echo, true);
            }
        }
    }
}

public class ReforgeSurprise : GridBlockSurprise {
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return player.armor.Any(p => p.accessory);
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        SoundEngine.PlaySound(SoundID.ResearchComplete, player.Center);
        foreach (var item in player.armor) {
            if (!item.accessory)
                continue;

            var prefix = ItemLoader.ChoosePrefix(item, Main.rand);
            item.Prefix(prefix);

            PopupText.NewText(new AdvancedPopupRequest {
                Color = ItemRarity.GetColor(item.rare),
                Text = item.HoverName,
                DurationInFrames = 60 * 2,
                Velocity = new Vector2(0, -8)
            }, player.Top);
        }
    }
}