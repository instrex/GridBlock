using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace GridBlock.Common;

public class GridBlockMapLayer : ModMapLayer {
    public override void Draw(ref MapOverlayDrawContext context, ref string text) {
        var gridBlock = ModContent.GetInstance<GridBlockWorld>();

        if (gridBlock.Chunks is null)
            return;

        var scale = Main.mapFullscreen ? Main.mapFullscreenScale : 4f;

        var pixel = ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;

        for (var i = 0; i < gridBlock.Chunks.Bounds.X * gridBlock.Chunks.Bounds.Y; i++) {
            var coord = new Point(i % gridBlock.Chunks.Bounds.X, i / gridBlock.Chunks.Bounds.X);
            if (coord.Y == 0 || coord.Y == gridBlock.Chunks.Bounds.Y - 1 || coord.X == 0 || coord.X == gridBlock.Chunks.Bounds.X - 1)
                continue;

            var pos = new Vector2(coord.X * gridBlock.Chunks.CellSize, coord.Y * gridBlock.Chunks.CellSize);

            var chunk = gridBlock.Chunks.GetById(i);

            if (chunk.IsUnlocked)
                continue;

            var color = chunk.Group switch {
                CostGroup.Expensive => Color.Magenta,
                _ => Color.Red * 0.5f
            };

            if (Main.mapFullscreen) {
                context.Draw(pixel,
                    pos + new Vector2(2),
                    color * 0.5f,
                    new SpriteFrame(1, 1, 0, 0),
                    (gridBlock.Chunks.CellSize - 1f) * scale * 0.5f, (gridBlock.Chunks.CellSize - 1f) * scale * 0.5f,
                    Alignment.TopLeft);
            } else {
                context.Draw(pixel,
                    pos + new Vector2(gridBlock.Chunks.CellSize * 0.5f),
                    color * 0.5f,
                    new SpriteFrame(1, 1, 0, 0),
                    scale * 9f, scale * 9f,
                    Alignment.Center);
            }

            if (chunk.UnlockCost != null) {
                var anim = Main.itemAnimations[chunk.UnlockCost.type];
                var tex = chunk.Group == CostGroup.Expensive ? 
                    ModContent.Request<Texture2D>("GridBlock/Assets/RewardIndicator")
                    : TextureAssets.Item[chunk.UnlockCost.type];

                context.Draw(tex.Value,
                    pos + new Vector2(gridBlock.Chunks.CellSize * 0.5f),
                    Color.White,
                    new SpriteFrame(1, (byte)(anim is null ? 1 : anim.FrameCount), 0, 0),
                    scale * 0.5f, scale * 0.5f,
                    Alignment.Center);
            }
        }
    }
}
