using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;

namespace FoundationFortune.API.Features.Commands.FortuneAdminCommands.NpcCommands
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

            if (args.Array != null)
            {
                string npcTypeString = args.Count > 0 ? args.Array[args.Offset] : null;

                if (Enum.TryParse(npcTypeString, true, out NpcType npcType))
                {
                    int count;
                    switch (npcType)
                    {
                        case NpcType.Buying:
                            count = RemoveBots(NPCInitialization.BuyingBots, BuyingBot.RemoveBuyingBot);
                            response = $"Flushed {count} BuyingBots from the server.";
                            return true;
                        case NpcType.Selling:
                            count = RemoveBots(NPCInitialization.SellingBots, SellingBot.RemoveSellingBot);
                            response = $"Flushed {count} SellingBots from the server.";
                            return true;
                        case NpcType.Music:
                        default:
                            response = "Invalid bot type.";
                            return false;
                    }
                }

                response = "Invalid bot type.";
                return false;
            }
            response = string.Empty;
            return false;
        }

        private static int RemoveBots(Dictionary<string, (Npc bot, int indexation)> bots, Func<string, bool> removeBotFunc) => bots.Values.Count(botInfo => removeBotFunc(botInfo.bot.Nickname));
    }
}
