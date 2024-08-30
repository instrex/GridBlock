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

        var scale = Main.mapFullscreen ? Main.mapFullscreenScale : 0f;

        var pixel = ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;

        for (var i = 0; i < gridBlock.Chunks.Bounds.X * gridBlock.Chunks.Bounds.Y; i++) {
            var coord = new Point(i % gridBlock.Chunks.Bounds.X, i / gridBlock.Chunks.Bounds.X);
            var pos = new Vector2(coord.X * gridBlock.Chunks.ChunkSize, coord.Y * gridBlock.Chunks.ChunkSize);

            var chunk = gridBlock.Chunks.GetById(i);

            context.Draw(pixel,
                pos + new Vector2(2), 
                chunk.Color * 0.25f,
                new SpriteFrame(1, 1, 0, 0), 
                (gridBlock.Chunks.ChunkSize - 1f) * scale * 0.5f, (gridBlock.Chunks.ChunkSize - 1f) * scale * 0.5f, 
                Alignment.TopLeft);

            context.Draw(TextureAssets.Item[chunk.UnlockCost.type].Value,
                pos + new Vector2(gridBlock.Chunks.ChunkSize * 0.5f),
                Color.White,
                new SpriteFrame(1, 1, 0, 0),
                scale * 0.5f, scale * 0.5f,
                Alignment.Center);

            Utils.DrawBorderString(Main.spriteBatch, chunk.UnlockCost.stack.ToString(),  Main.mapFullscreenPos, Color.White, 1f);
        }
    }
}
