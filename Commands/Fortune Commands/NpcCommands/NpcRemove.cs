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
    internal class NpcRemove : ICommand
    {
        public string Command { get; } = "remove";
        public string Description { get; } = "Remove an NPC from the game.";
        public string[] Aliases { get; } = new string[] { "npcremove", "removenpc" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.remove"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 2)
            {
                response = "Usage: foundationfortune npc remove <IndexationNumber>";
                return false;
            }

            if (!int.TryParse(args.At(1), out int indexationNumber))
            {
                response = "Invalid IndexationNumber. Please provide a valid number.";
                return false;
            }

            if (BuyingBot.RemoveBuyingBot(indexationNumber))
            {
                BuyingBot.RemoveBuyingBot(indexationNumber);
                response = $"Removed BuyingBot with IndexationNumber {indexationNumber}.";
                return true;
            }
            else
            {
                response = $"No BuyingBot found with IndexationNumber {indexationNumber}.";
                return false;
            }
        }
    }
}
