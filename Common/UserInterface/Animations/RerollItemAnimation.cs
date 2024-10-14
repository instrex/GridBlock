using GridBlock.Common.UserInterface;
using Microsoft.Xna.Framework;
using Terraria;

namespace GridBlock.Common.UserInterface.Animations;

public class RerollItemAnimation : IAnimation {
    public float Lifetime { get; set; }
    public bool IsExpired => Lifetime > duration;

    public int oldItemType;
    public Vector2 position;
    public float duration;
    public Vector2 velocity;
    public float gravity;
    public float rotation;
    public bool hidden;

    public void Draw() {
        var progress = Lifetime / duration;

        Main.GetItemDrawFrame(oldItemType, out var tex, out var frame);

        var scale = EaseHelper.Fade(progress, 0.1f, 0.9f);

        Main.spriteBatch.Draw(tex,
            position - Main.screenPosition,
            frame,
            (hidden ? Color.Black : Color.White) * scale,
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
