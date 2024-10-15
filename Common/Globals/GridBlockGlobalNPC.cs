using GridBlock.Content.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GridBlock.Common.Globals;

internal class GridBlockGlobalNPC : GlobalNPC {
    public override void OnKill(NPC npc) {
        CheckAndSpawnDice(npc, NPCID.EyeofCthulhu, nameof(NPCID.EyeofCthulhu));
        CheckAndSpawnDice(npc, NPCID.KingSlime, nameof(NPCID.KingSlime));
        CheckAndSpawnDice(npc, NPCID.BrainofCthulhu, nameof(NPCID.BrainofCthulhu));
        CheckAndSpawnDice(npc, NPCID.QueenBee, nameof(NPCID.QueenBee));
        CheckAndSpawnDice(npc, NPCID.SkeletronHead, nameof(NPCID.SkeletronHead));
        CheckAndSpawnDice(npc, NPCID.Deerclops, nameof(NPCID.Deerclops));
        CheckAndSpawnDice(npc, NPCID.WallofFlesh, nameof(NPCID.WallofFlesh), 5);
        CheckAndSpawnDice(npc, NPCID.QueenSlimeBoss, nameof(NPCID.QueenSlimeBoss));
        CheckAndSpawnDice(npc, NPCID.Retinazer, nameof(NPCID.Retinazer), 1);
        CheckAndSpawnDice(npc, NPCID.Spazmatism, nameof(NPCID.Spazmatism), 2);
        CheckAndSpawnDice(npc, NPCID.SkeletronPrime, nameof(NPCID.SkeletronPrime));
        CheckAndSpawnDice(npc, NPCID.TheDestroyer, nameof(NPCID.TheDestroyer));
        CheckAndSpawnDice(npc, NPCID.Plantera, nameof(NPCID.Plantera));
        CheckAndSpawnDice(npc, NPCID.Golem, nameof(NPCID.Golem));
        CheckAndSpawnDice(npc, NPCID.DukeFishron, nameof(NPCID.DukeFishron));
        CheckAndSpawnDice(npc, NPCID.HallowBoss, nameof(NPCID.HallowBoss));
        CheckAndSpawnDice(npc, NPCID.CultistBoss, nameof(NPCID.CultistBoss));
        CheckAndSpawnDice(npc, NPCID.MoonLordCore, nameof(NPCID.MoonLordCore), 10);
    }

    static void CheckAndSpawnDice(NPC npc, int wantedType, string flag, int diceAmount = 3) {
        if (npc.type != wantedType)
            return;

        var flags = GridBlockWorld.Instance.BossRerollsObtained;
        if (!flags.Contains(flag)) {
            flags.Add(flag);

            for (var i = 0; i < diceAmount; i++) {
                Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ModContent.ItemType<ChunkDice>());
            }
        }
    }
}
