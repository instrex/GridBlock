using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.UI;

namespace GridBlock.Common;

public class RerollItemAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > duration;

    public int oldItemType;
    public Vector2 position;
    public float duration;
    public Vector2 velocity;
    public float gravity;
    public float rotation;

    public void Draw() {
        var progress = Lifetime / duration;

        Main.GetItemDrawFrame(oldItemType, out var tex, out var frame);

        var scale = EaseHelper.Fade(progress, 0.1f, 0.9f);

        Main.spriteBatch.Draw(tex,
            position - Main.screenPosition,
            frame,
            Color.White * scale,
            rotation,
            frame.Size() * 0.5f,
            scale * 1.25f,
            0,
            0
        );
    }

    public void Update() {
        velocity *= gravity;
        position += velocity;
        rotation += velocity.Length() * 0.1f;
    }
}

public class UnlockItemFlowAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > 30f;

    public Item item;
    public float rotation, rotationSpeed;
    public float arcAmount;

    public Vector2 origin, target;

    public void Draw() {
        var progress = Lifetime / 30f;

        Main.GetItemDrawFrame(item.type, out var tex, out var frame);

        var pos = EaseHelper.SampleCubicBezier(origin,
            Vector2.Lerp(origin, target, 0.5f) + origin.DirectionTo(target).RotatedBy(MathHelper.PiOver2) * arcAmount * 260f,
            target, EaseHelper.SineInOut(progress));

        var scale = (1f - MathF.Abs(progress - 0.5f) / 0.5f);

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
            Color.White * scale,
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
