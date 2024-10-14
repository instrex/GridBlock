using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace GridBlock.Common.UserInterface.Animations;

internal class GlowAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > Duration;

    public float Duration { get; set; }
    public Color Color { get; set; } = Color.White;
    public float FadeIn { get; set; } = 0.5f;
    public float FadeOut { get; set; } = 0.5f;
    public float Scale { get; set; } = 1.0f;
    public Vector2 Position { get; set; }

    public void Update() { }
    public void Draw() {
        Main.spriteBatch.Draw(
            ModAssets.LightTex,
            Position - Main.screenPosition,
            null,
            Color * EaseHelper.Fade(Lifetime / Duration, FadeIn, FadeOut),
            0,
            ModAssets.LightTex.Size() * 0.5f,
            Scale,
            0,
            0
        );
    }

}
