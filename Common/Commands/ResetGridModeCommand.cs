using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class ResetGridModeCommand : ModCommand {
    public override string Command => "resetGridMode";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        GridBlockWorld.Instance.AppliedHardmodeChanges = false;
    }
}