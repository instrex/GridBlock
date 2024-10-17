using GridBlock.Common;
using GridBlock.Common.Globals;
using GridBlock.Common.Surprises;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Utilities;

namespace GridBlock.Content.Surprises;

internal class ItemDropSurprise : GridBlockSurprise {
    public override float GetWeight(Player player, GridBlockChunk chunk) {
        return 0.5f;
    }

    static IEnumerable<Item> GetValidItems(Player player) => player.inventory.Where(i => !i.IsAir && i.pick == 0);
    public override bool CanBeTriggered(Player player, GridBlockChunk chunk) {
        return GetValidItems(player).Count() > 3;
    }

    public override void Trigger(Player player, GridBlockChunk chunk) {
        var items = new WeightedRandom<Item>(GetValidItems(player)
            .Select(i => new Tuple<Item, double>(i, 1.0)).ToArray());

        var amountToDrop = Main.rand.Next(1, items.elements.Count / 3);
        for (var i = 0; i < amountToDrop; i++) {
            var item = items.Get();
            items.elements.RemoveAll(i => i.Item1 == item);
            items.needsRefresh = true;

            var clone = item.Clone();
            item.TurnToAir();

            var k = Item.NewItem(player.GetSource_FromThis(), player.getRect(), clone);
            Main.item[k].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 8);
            Main.item[k].GetGlobalItem<GridBlockItem>().IsGridBlockReward = true;
            Main.item[k].noGrabDelay = Main.rand.Next(60 * 1, 60 * 3);
        }
    }
}
