using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class GrantRerollsCommand : ModCommand {
    public override string Command => "addRerolls";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 1 || !int.TryParse(args[0], out var rerollsToAdd)) {
            Main.NewText("Expected int rerollsToAdd");
            return;
        }

        ModContent.GetInstance<GridBlockWorld>().RerollCount += rerollsToAdd;
    }
}
