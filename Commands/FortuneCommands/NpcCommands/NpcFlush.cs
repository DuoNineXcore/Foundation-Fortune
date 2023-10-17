using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.NpcCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NpcFlush : ICommand
    {
        public string Command { get; } = "ff_flushnpcs";
        public string Description { get; } = "Flush all NPCs from the game.";
        public string[] Aliases { get; } = new string[] { };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.flush"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            int count;
            string botTypeString = args.Count > 0 ? args.Array[args.Offset] : null;

            if (Enum.TryParse(botTypeString, true, out BotType botType))
            {
                switch (botType)
                {
                    case BotType.Buying:
                        count = RemoveBots(FoundationFortune.Singleton.BuyingBots, BuyingBot.RemoveBuyingBot);
                        response = $"Flushed {count} BuyingBots from the server.";
                        return true;
                    case BotType.Selling:
                        count = RemoveBots(FoundationFortune.Singleton.SellingBots, SellingBot.RemoveSellingBot);
                        response = $"Flushed {count} SellingBots from the server.";
                        return true;
                    case BotType.Music:
                        count = RemoveBots(FoundationFortune.Singleton.MusicBots, MusicBot.RemoveMusicBot);
                        response = $"Flushed {count} MusicBots from the server.";
                        return true;
                    default:
                        response = "Invalid bot type.";
                        return false;
                }
            }
            else
            {
                response = "Invalid bot type.";
                return false;
            }
        }

        private int RemoveBots(Dictionary<string, (Npc bot, int indexation)> bots, Func<string, bool> removeBotFunc)
        {
            int count = 0;
            foreach (var botInfo in bots.Values.ToList())
            {
                if (removeBotFunc(botInfo.bot.Nickname))
                {
                    count++;
                }
            }
            return count;
        }
    }
}
