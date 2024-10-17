using GridBlock.Common;
using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

public class ReforgeSurprise : GridBlockSurprise {
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.5f;
    }

    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return player.armor.Count(p => p.accessory) >= 3;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        SoundEngine.PlaySound(SoundID.ResearchComplete, player.Center);

        var accs = player.armor.Where(i => i.accessory).ToList();

        var itemsToReforge = accs.Count / 2;

        for (var i = 0; i < itemsToReforge; i++) {
            var acc = accs[Main.rand.Next(accs.Count)];
            accs.Remove(acc);

            var prefix = ItemLoader.ChoosePrefix(acc, Main.rand);
            acc.Prefix(prefix);

            PopupText.NewText(new AdvancedPopupRequest {
                Color = ItemRarity.GetColor(acc.rare),
                Text = acc.HoverName,
                DurationInFrames = 60 * 2,
                Velocity = new Vector2(0, -8)
            }, player.Top);
        }
    }
}