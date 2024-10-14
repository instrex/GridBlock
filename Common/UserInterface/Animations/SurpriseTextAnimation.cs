using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;

namespace GridBlock.Common.UserInterface.Animations;

internal class SurpriseTextAnimation : IAnimation {
    float Duration => 60 * 6;

    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > Duration;

    public string Title { get; set; }
    public Color TitleColor { get; set; }
    public string Description { get; set; }

    public void Update() { }

    public void Draw() {
        var origin = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.75f + MathF.Sin(Main.GlobalTimeWrappedHourly * 4) * 10);
        var progress = Lifetime / Duration;

        var pos = origin + new Vector2(0, 128 * (1f - EaseHelper.ExpoOut(MathHelper.Clamp(progress / 0.2f, 0, 1))));
        var f = EaseHelper.Fade(progress, 0.05f, 0.25f);

        var titleSize = FontAssets.MouseText.Value.MeasureString(Title);
        var titleScale = (0.7f + 0.05f * f) * EaseHelper.Fade(progress, 0f, 0.05f);

        Main.spriteBatch.Draw(
            ModAssets.LightTex,
            pos + new Vector2(0, titleSize.Y * -1 * titleScale + MathF.Sin(Main.GlobalTimeWrappedHourly * 3) * 4),
            null,
            TitleColor with { A = 0 } * f * 0.25f,
            0,
            ModAssets.LightTex.Size() * 0.5f,
            titleSize * new Vector2(0.1f, 0.05f) * titleScale * (1f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 12)),
            0,
            0
        );

        if (!string.IsNullOrEmpty(Description)) {
            Utils.DrawBorderStringBig(Main.spriteBatch, Description, pos + new Vector2(0, -12), Color.White * f,
                (0.5f + 0.05f * f) * EaseHelper.Fade(progress, 0f, 0.05f), 0.5f, 0);
        }

        Utils.DrawBorderStringBig(Main.spriteBatch, Title, pos, TitleColor * f, 
            titleScale, 0.5f, 1f);
    }

}
