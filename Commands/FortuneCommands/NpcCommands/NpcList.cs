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
            AppendBotList(sb, "Buying Bots", FoundationFortune.Singleton.BuyingBots);
            AppendBotList(sb, "Selling Bots", FoundationFortune.Singleton.SellingBots);
            AppendBotList(sb, "Music Bots", FoundationFortune.Singleton.MusicBots);
            response = sb.ToString();
            return true;
        }

        private void AppendBotList(StringBuilder sb, string title, Dictionary<string, (Npc? bot, int indexation)> botIndexation)
        {
            sb.AppendLine(title + ":");
            foreach (var botInfo in botIndexation)
            {
                sb.AppendLine($"Name: {botInfo.Value.bot?.Nickname ?? "Null Nickname"}, Indexation: {botInfo.Value.indexation}");
            }
        }
    }
}
