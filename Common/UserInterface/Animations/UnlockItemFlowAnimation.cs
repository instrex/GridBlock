using GridBlock.Common.UserInterface;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.UI;

namespace GridBlock.Common.UserInterface.Animations;

public class UnlockItemFlowAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > 30f;

    public Item item;
    public float rotation, rotationSpeed;
    public float arcAmount;
    public bool hidden;
    public Vector2 origin, target;

    public void Draw() {
        var progress = Lifetime / 30f;

        Main.GetItemDrawFrame(item.type, out var tex, out var frame);

        var pos = EaseHelper.SampleCubicBezier(origin,
            Vector2.Lerp(origin, target, 0.5f) + origin.DirectionTo(target).RotatedBy(MathHelper.PiOver2) * arcAmount * 260f,
            target, EaseHelper.SineInOut(progress));

        var scale = 1f - MathF.Abs(progress - 0.5f) / 0.5f;

        Main.spriteBatch.Draw(ModAssets.LightTex,
            pos - Main.screenPosition,
            null,
            ItemRarity.GetColor(item.rare) with { A = 0 } * scale * 0.2f,
            rotation,
            new Vector2(45),
            scale,
            0,
            0
        );

        Main.spriteBatch.Draw(tex,
            pos - Main.screenPosition,
            frame,
            (hidden ? Color.Black : Color.White) * scale,
            rotation,
            frame.Size() * 0.5f,
            /*progress < 0.2f ? progress / 0.2f : (progress >= 0.8f ? 1f - (progress - 0.8f) / 0.2f : 1f)*/
            scale * 1.25f,
            0,
            0
        );
    }

    public void Update() {
        rotation += rotationSpeed;
    }
}
