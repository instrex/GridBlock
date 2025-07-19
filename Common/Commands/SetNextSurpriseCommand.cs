using GridBlock.Common.Surprises;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace GridBlock.Common.Commands;

internal class SetNextSurpriseCommand : ModCommand {
    static GridBlockSurprise PendingSurprise { get; set; }

    static internal bool TryGetPendingSurprise(out GridBlockSurprise surprise) {
        surprise = PendingSurprise;
        PendingSurprise = null;
        return surprise != null;
    }

    public override string Command => "surpriseNext";
    public override CommandType Type => CommandType.Chat;

    public override void Action(CommandCaller caller, string input, string[] args) {
        if (args.Length < 1) {
            Main.NewText("Expected string surpriseId");
            return;
        }

        var surprise = ModContent.GetContent<GridBlockSurprise>()
            .FirstOrDefault(s => s.Id.StartsWith(args[0], System.StringComparison.InvariantCultureIgnoreCase));

        if (surprise is null) {
            caller.Reply($"'{surprise}' is invalid.", Color.Red);
            return;
        }

        PendingSurprise = surprise;
    }
}