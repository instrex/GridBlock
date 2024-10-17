using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Common.UserInterface.Animations;

internal class UnlockAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > 60;

    public GridBlockChunk Chunk { get; set; }

    public void Update() { }

    public void Draw() {
        var f = Lifetime / 60f;
        for (var x = 0; x < GridBlockWorld.Instance.Chunks.CellSize; x += 2) {
            for (var y = 0; y < GridBlockWorld.Instance.Chunks.CellSize; y += 2) {
                var pos = (Chunk.TileCoord + new Point(x, y)).ToWorldCoordinates(16, 16);
                var distToPlayer = Main.LocalPlayer.Distance(pos);
                var dirToPlayer = Main.LocalPlayer.DirectionTo(pos);

                var startFrom = distToPlayer * 0.0005f;

                var cf = f < startFrom ? 0 : MathHelper.Clamp((f - startFrom) / 0.1f, 0, 1);

                Main.spriteBatch.Draw(
                    ModAssets.PixelTex,
                    pos - Main.screenPosition,
                    null,
                    Color.Red * 0.35f,
                    0,
                    ModAssets.PixelTex.Size() * 0.5f,
                    16f * (1f - cf),
                    0,
                    0
                );

            }
        }
    }

}
