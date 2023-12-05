using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;

namespace FoundationFortune.Commands.FortuneAdminCommands.HintCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintEnqueue : ICommand,IUsageProvider
    {
        public string Command { get; } = "ff_enqueuehint";
        public string Description { get; } = "Broadcast a message in the hint system. either for a player or globally.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<self|steamid|all> <text> <duration>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (args.Count < 3)
            {
                response = "Usage: <self|steamid|all> <text> <duration>";
                return false;
            }

            string target = args.At(0).ToLower();
            string hint = args.At(1);

            if (!int.TryParse(args.At(2), out int duration))
            {
                response = "Invalid duration. Please enter a valid number.";
                return false;
            }

            switch (target)
            {
                case "self":
                {
                    Player player = Player.Get(sender);
                    FoundationFortune.Instance.HintSystem.EnqueueHint(player, hint);
                    response = "Hint broadcasted successfully.";
                    break;
                }
                case "all":
                {
                    foreach (Player ply in Player.List.Where(p => !p.IsNPC)) FoundationFortune.Instance.HintSystem.EnqueueHint(ply, hint);
                    response = "Hint broadcasted successfully.";
                    break;
                }
                default:
                {
                    Player steamId = Player.Get(target);
                    FoundationFortune.Instance.HintSystem.EnqueueHint(steamId, hint);
                    response = $"Hint broadcasted successfully.";
                    break;
                }
            }
            return true;
        }
    }
}
