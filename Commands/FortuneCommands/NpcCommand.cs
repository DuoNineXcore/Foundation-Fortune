using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Linq;
using FoundationFortune.API.NPCs;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.Commands.FortuneCommands
{
    internal class NpcCommand : ICommand
    {
        public string Command { get; } = "npc";
        public string Description { get; } = "Npc stuff";
        public string[] Aliases { get; } = new string[] { "n" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc"))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            if (args.Count < 1)
            {
                response = "Usage: foundationfortune npc <add/remove/list/flush>";
                return false;
            }

            string subcommand = args.At(0).ToLower();

            switch (subcommand)
            {
                case "add":
                    return AddBuyingBot(args, sender, out response);
                case "remove":
                    return RemoveBuyingBot(args, sender, out response);
                case "list":
                    return ListBuyingBots(args, sender, out response);
                case "flush":
                    return FlushBuyingBots(args, sender, out response);
                default:
                    response = "Invalid subcommand for 'npc'. Use 'add', 'remove', 'list', or 'flush'.";
                    return false;
            }
        }

        private bool AddBuyingBot(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.add"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (arguments.Count < 9)
            {
                response = "Usage: foundationfortune npc add <Name> <Badge> <Color> <Role> <HeldItem> <ScaleX> <ScaleY> <ScaleZ>";
                return false;
            }

            string name = arguments.At(1);
            string badge = arguments.At(2);
            string color = arguments.At(3);
            string roleString = arguments.At(4);
            string heldItemString = arguments.At(5);

            if (!Enum.TryParse(roleString, out RoleTypeId role) || !Enum.TryParse(heldItemString, out ItemType heldItem))
            {
                response = "Invalid Role or HeldItem specified.";
                return false;
            }

            if (!float.TryParse(arguments.At(6), out float scaleX) || !float.TryParse(arguments.At(7), out float scaleY) || !float.TryParse(arguments.At(8), out float scaleZ))
            {
                response = "Invalid Scale specified. Please provide three float values for X, Y, and Z components.";
                return false;
            }

            Vector3 scale = new(scaleX, scaleY, scaleZ);

            Npc bot = BuyingBot.SpawnBuyingBot(name, badge, color, role, heldItem, scale);

            if (bot != null)
            {
                if (!(sender is PlayerCommandSender self))
                {
                    response = "Only players can use this command.";
                    return false;
                }

                Timing.CallDelayed(1f, delegate
                {
                    bot.Teleport(self.ReferenceHub.gameObject.transform.position);
                    FoundationFortune.Singleton.serverEvents.buyingBotPositions.Add(bot, bot.Position);
                });

                response = $"BuyingBot '{name}' added successfully and teleported to your position.";
                return true;
            }

            response = $"Failed to add the BuyingBot.";
            return false;
        }

        private bool RemoveBuyingBot(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.remove"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (arguments.Count < 2)
            {
                response = "Usage: foundationfortune npc remove <IndexationNumber>";
                return false;
            }

            if (!int.TryParse(arguments.At(1), out int indexationNumber))
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

        private bool ListBuyingBots(ArraySegment<string> arguments, ICommandSender sender, out string response)
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

            if (FoundationFortune.Singleton.BuyingBotIndexation == null)
            {
                response = "FoundationFortune.Singleton.BuyingBotIndexation is null.";
                return false;
            }

            var buyingBots = FoundationFortune.Singleton.BuyingBotIndexation.Values.ToList();

            if (buyingBots.Any())
            {
                response = "List of BuyingBots in the server:\n";

                foreach (var botInfo in buyingBots)
                {
                    response += $"Indexation Number: {botInfo.indexation}, Name: {(botInfo.bot != null ? botInfo.bot.Nickname : "null")}\n";
                }
            }
            else
            {
                response = "No BuyingBots found in the server.";
            }

            return true;
        }

        private bool FlushBuyingBots(ArraySegment<string> arguments, ICommandSender sender, out string response)
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
