using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationFortune.API.Models;

namespace FoundationFortune.Commands.FortuneCommands.NpcCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NpcRemove : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_removenpc";
        public string Description { get; } = "Remove an NPC from the game.";
        public string[] Aliases { get; } = new string[] { };
        public string[] Usage { get; } = new string[] { "<BotType> <IndexationNumber>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.remove"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 2)
            {
                response = "Usage: ff_removenpc <BotType> <IndexationNumber>";
                return false;
            }

            string botTypeString = args.At(0);
            if (!Enum.TryParse(botTypeString, true, out BotType botType))
            {
                response = "Invalid BotType specified.";
                return false;
            }

            if (!int.TryParse(args.At(1), out int indexationNumber))
            {
                response = "Invalid Indexation Number. Please provide a valid number.";
                return false;
            }

            string indexationString = indexationNumber.ToString();

            switch (botType)
            {
                case BotType.Buying:
                    if (BuyingBot.RemoveBuyingBot(indexationString))
                    {
                        response = $"Removed BuyingBot with Indexation Number {indexationNumber}.";
                        return true;
                    }
                    else
                    {
                        response = $"No BuyingBot found with Indexation Number {indexationNumber}.";
                        return false;
                    }
                case BotType.Selling:
                    if (SellingBot.RemoveSellingBot(indexationString))
                    {
                        response = $"Removed SellingBot with Indexation Number {indexationNumber}.";
                        return true;
                    }
                    else
                    {
                        response = $"No SellingBot found with Indexation Number {indexationNumber}.";
                        return false;
                    }
                case BotType.Music:
                    if (MusicBot.RemoveMusicBot(indexationString))
                    {
                        response = $"Removed MusicBot with Indexation Number {indexationNumber}.";
                        return true;
                    }
                    else
                    {
                        response = $"No MusicBot found with Indexation Number {indexationNumber}.";
                        return false;
                    }
                default:
                    response = "Invalid BotType specified.";
                    return false;
            }
        }
    }
}
