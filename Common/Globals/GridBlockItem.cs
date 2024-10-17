using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Common.Globals;

public class GridBlockItem : GlobalItem {
    public override bool InstancePerEntity => true;

    public bool IsGridBlockReward;

    public override bool OnPickup(Item item, Player player) {
        IsGridBlockReward = false;
        return true;
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {
        if (!IsGridBlockReward)
            return;

        if (item.velocity.Y >= 0) {
            item.velocity.Y = 0;
            gravity *= 0;
        }

        var rect = item.getRect();
        rect.Inflate(10, 10);

        if (Main.rand.NextFloat() < 0.2f) {
            var dust = Dust.NewDustDirect(rect.Location.ToVector2(), rect.Width, rect.Height, DustID.GoldCoin, Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-2, 0));
            dust.noGravity = true;
            dust.fadeIn = 1.1f;
        }
    }
}
