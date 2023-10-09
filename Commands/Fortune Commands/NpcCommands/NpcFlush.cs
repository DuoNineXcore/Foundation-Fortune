using CommandSystem;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.NpcCommands
{
    internal class NpcFlush : ICommand
    {
        public string Command { get; } = "flush";
        public string Description { get; } = "Flush all NPCs from the game.";
        public string[] Aliases { get; } = new string[] { "flushnpcs", "removenpcs" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.flush"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            var buyingBots = FoundationFortune.Singleton.BuyingBotIndexation.Values.ToList();

            foreach (var botInfo in buyingBots)
            {
                BuyingBot.RemoveBuyingBot(botInfo.indexation);
            }

            response = $"Flushed {buyingBots.Count} BuyingBots from the server.";
            return true;
        }
    }
}
