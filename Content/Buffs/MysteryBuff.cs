using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Content.Buffs;

public class MysteryBuff : ModBuff {
    public override void SetStaticDefaults() {
        BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        Main.persistentBuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;
    }
}
