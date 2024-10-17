using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class CallSurpriseCommand : ModCommand {
    public override string Command => "surprise";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 1) {
            Main.NewText("Expected string surpriseId");
            return;
        }

        var chunk = GridBlockWorld.Instance.Chunks.GetByWorldPos(caller.Player.Center);

        var surprise = ModContent.GetContent<GridBlockSurprise>()
            .FirstOrDefault(s => s.Id.StartsWith(args[0], System.StringComparison.InvariantCultureIgnoreCase));

        if (surprise is null || !surprise.CanBeTriggered(caller.Player, chunk)) {
            caller.Reply($"Can't trigger {surprise} right now.", Color.Red);
            return;
        }

        surprise.Trigger(caller.Player, chunk);
    }
}
