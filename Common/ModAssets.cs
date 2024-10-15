using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace GridBlock.Common;

public static class ModAssets {
    public static Texture2D MouseIconTex => ModContent.Request<Texture2D>("GridBlock/Assets/MouseIcons").Value;
    public static Rectangle MouseIconTex_Left => new(0, 0, 32, 32);
    public static Rectangle MouseIconTex_Right => new(32, 0, 32, 32);

    public static Texture2D BorderGradientTex => ModContent.Request<Texture2D>("GridBlock/Assets/BorderGradient").Value;
    public static Rectangle BorderGradient_Corner => new(2, 2, 64, 64);
    public static Rectangle BorderGradient_Side => new(2, 68, 64, 64);

    public static Texture2D StatusIconTex => ModContent.Request<Texture2D>("GridBlock/Assets/StatusIcons").Value;
    public static Rectangle StatusIconTex_Locked => new(0, 0, 32, 32);
    public static Rectangle StatusIconTex_Discounted => new(32, 0, 32, 32);
    public static Rectangle StatusIconTex_Dicey => new(64, 0, 32, 32);
    public static Rectangle StatusIconTex_Mystery => new(96, 0, 32, 32);

    public static Texture2D PixelTex => ModContent.Request<Texture2D>("GridBlock/Assets/Pixel").Value;
    public static Texture2D LightTex => ModContent.Request<Texture2D>("GridBlock/Assets/Light").Value;
    public static Texture2D UnlockSlotTex => ModContent.Request<Texture2D>("GridBlock/Assets/UnlockSlot").Value;
    public static Texture2D RewardIndicatorTex => ModContent.Request<Texture2D>("GridBlock/Assets/RewardIndicator").Value;
    public static Texture2D LockIconTex => ModContent.Request<Texture2D>("GridBlock/Assets/LockIcon").Value;
}
