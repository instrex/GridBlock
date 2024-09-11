using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class RegenChunksCommand : ModCommand {
    public override string Command => "gridClear";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        ModContent.GetInstance<GridBlockWorld>().ResetChunks();
    }
}
