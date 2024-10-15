using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria;
using Terraria.ModLoader;
using GridBlock.Common;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;
using Terraria.ID;

namespace GridBlock.Content.Items;

internal class ChunkDice : ModItem {
    public override void SetDefaults() {
        Item.height = 30;
        Item.width = 30;
        Item.maxStack = 99;
        Item.rare = ItemRarityID.Purple;
    }

    public override bool ItemSpace(Player player) => true;
    public override bool OnPickup(Player player) {
        SoundEngine.PlaySound(SoundID.Item101);

        // TODO: sync
        GridBlockWorld.Instance.RerollCount += Item.stack;
        PopupText.NewText(new AdvancedPopupRequest {
            Text = Language.GetTextValue("Mods.GridBlock.RerollObtained", GridBlockWorld.Instance.RerollCount),
            DurationInFrames = 60 * 2,
            Color = Color.SkyBlue,
            Velocity = new Vector2(0, -10)
        }, player.Center);

        return false;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed) {
        maxFallSpeed = 0;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
        spriteBatch.Draw(
            ModAssets.LightTex,
            Item.Center - Main.screenPosition,
            null,
            Color.Lerp(Color.Blue, Color.Red, 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2)) with { A = 0 } * 0.5f,
            0,
            ModAssets.LightTex.Size() * 0.5f,
            1f,
            0,
            0
        );

        spriteBatch.Draw(
            TextureAssets.Item[Type].Value,
            Item.Center - Main.screenPosition,
            null,
            Color.Lerp(Color.White, lightColor, 0.25f),
            rotation,
            TextureAssets.Item[Type].Size() * 0.5f,
            scale,
            0,
            0
        );

        for (var i = 0; i < 4; i++) {
            var dir = (MathHelper.PiOver2 * i + Main.GlobalTimeWrappedHourly * 4).ToRotationVector2();
            spriteBatch.Draw(
                TextureAssets.Item[Type].Value,
                Item.Center + dir * new Vector2(6, 3) - Main.screenPosition,
                null,
                Color.Lerp(Color.Red, Color.Blue, (Vector2.Dot(dir, new Vector2(0, -1)) + 1) / 2) with { A = 0 } * 0.25f,
                rotation,
                TextureAssets.Item[Type].Size() * 0.5f,
                scale,
                0,
                0
            );
        }

        return false;
    }
}
