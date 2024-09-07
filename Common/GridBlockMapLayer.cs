using GridBlock.Common.Costs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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
            var pos = new Vector2(coord.X * gridBlock.Chunks.CellSize, coord.Y * gridBlock.Chunks.CellSize);

            var chunk = gridBlock.Chunks.GetById(i);

            if (chunk.IsUnlocked)
                continue;

            var color = chunk.Group switch {
                CostGroup.Beginner => Color.Green,
                CostGroup.Common => Color.Yellow,
                CostGroup.Advanced => Color.Orange,
                CostGroup.Hardcore => Color.Red,
                _ => Color.White
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
                context.Draw(TextureAssets.Item[chunk.UnlockCost.type].Value,
                    pos + new Vector2(gridBlock.Chunks.CellSize * 0.5f),
                    Color.White,
                    new SpriteFrame(1, 1, 0, 0),
                    scale * 0.5f, scale * 0.5f,
                    Alignment.Center);
            }
            
        }
    }
}
