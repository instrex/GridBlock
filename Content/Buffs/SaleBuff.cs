using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Content.Buffs;

public class SaleBuff : ModBuff {
    public override void SetStaticDefaults() {
        BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        Main.persistentBuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;
    }
}
