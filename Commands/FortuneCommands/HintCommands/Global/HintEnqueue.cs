using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationFortune.API.Models;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Global
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintEnqueue : ICommand,IUsageProvider
    {
        public string Command { get; } = "ff_enqueuehint";
        public string Description { get; } = "Broadcast a message in the hint system. either for a player or globally.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<self|steamid|all> <text> <duration> <left|right|center>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (args.Count < 3)
            {
                response = "Usage: <self|steamid|all> <text> <duration> [left|right|center]";
                return false;
            }

            string target = args.At(0).ToLower();
            string hint = args.At(1);

            if (!float.TryParse(args.At(2), out float duration))
            {
                response = "Invalid duration. Please enter a valid number.";
                return false;
            }

            HintAnim alignment = HintAnim.None;
            if (args.Count > 3)
            {
                string alignmentStr = args.At(3).ToLower();
                alignment = alignmentStr switch
                {
                    "left" => HintAnim.Left,
                    "right" => HintAnim.Right,
                    "center" => HintAnim.Center,
                    _ => HintAnim.None
                };
            }

            switch (target)
            {
                case "self":
                {
                    Player player = Player.Get(sender);
                    FoundationFortune.Singleton.ServerEvents.EnqueueHint(player, hint, duration, alignment);
                    response = "Hint broadcasted successfully.";
                    break;
                }
                case "all":
                {
                    foreach (Player ply in Player.List.Where(p => !p.IsNPC)) FoundationFortune.Singleton.ServerEvents.EnqueueHint(ply, hint, duration, alignment);
                    response = "Hint broadcasted successfully.";
                    break;
                }
                default:
                {
                    Player steamId = Player.Get(target);
                    FoundationFortune.Singleton.ServerEvents.EnqueueHint(steamId, hint, duration, alignment);
                    response = $"Hint broadcasted successfully.";
                    break;
                }
            }
            return true;
        }
    }
}
