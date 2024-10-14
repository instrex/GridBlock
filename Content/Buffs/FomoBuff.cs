using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Content.Buffs;

public class FomoBuff : ModBuff {
    public override void SetStaticDefaults() {
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        Main.persistentBuff[Type] = true;
        Main.debuff[Type] = true;
    }
}