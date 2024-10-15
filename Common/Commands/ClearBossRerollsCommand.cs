using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class ClearBossRerollsCommand : ModCommand {
    public override string Command => "clearBossRerolls";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        GridBlockWorld.Instance.BossRerollsObtained.Clear();
    }
}