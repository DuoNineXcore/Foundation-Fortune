using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.NpcCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NpcList : ICommand
    {
        public string Command { get; } = "ff_npclist";
        public string Description { get; } = "List all NPCs in the game.";
        public string[] Aliases { get; } = new string[] { };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.list"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (FoundationFortune.Singleton == null)
            {
                response = "FoundationFortune.Singleton is null.";
                return false;
            }

            StringBuilder sb = new();
            sb.AppendLine("Foundation Fortune NPCs:");
            AppendBotList(sb, "Selling Bots", FoundationFortune.Singleton.SellingBots.Values);
            AppendBotList(sb, "Buying Bots", FoundationFortune.Singleton.BuyingBots.Values);
            response = sb.ToString();
            return true;
        }

        private static void AppendBotList(StringBuilder sb, string title, IEnumerable<(Npc bot, int indexation)> collection)
        {
            sb.AppendLine(title + ":");
            foreach (var (bot, indexation) in collection) sb.AppendLine(bot != null ? $"{bot.Nickname} {indexation} - Position: {bot.Position} - Room: {bot.CurrentRoom.Type}" : "Invalid bot format.");
        }
    }
}
