using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if (alignmentStr == "left") alignment = HintAnim.Left;
                else if (alignmentStr == "right") alignment = HintAnim.Right;
                else if (alignmentStr == "center") alignment = HintAnim.Center;
                else alignment = HintAnim.None;
            }

            if (target == "self")
            {
                Player player = Player.Get(sender);
                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, hint, duration, alignment);
                response = "Hint broadcasted successfully.";
            }
            else if (target == "all")
            {
                foreach (Player ply in Player.List.Where(p => !p.IsNPC)) FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, hint, duration, alignment);
                response = "Hint broadcasted successfully.";
            }
            else
            {
                Player steamId = Player.Get(target);
                FoundationFortune.Singleton.serverEvents.EnqueueHint(steamId, hint, duration, alignment);
                response = $"Hint broadcasted successfully.";
            }
            return true;
        }
    }
}
