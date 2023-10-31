using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Models;
using FoundationFortune.API.NPCs;
using MEC;
using PlayerRoles;
using RemoteAdmin;
using UnityEngine;

namespace FoundationFortune.Commands.FortuneCommands.NpcCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class NpcAdd : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_addnpc";
        public string Description { get; } = "Add an NPC to the game.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<BotType> <Name> <Badge> <Color> <RoleTypeId> <HeldItemId> <ScaleX> <ScaleY> <ScaleZ>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.npc.add"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 10)
            {
                response = "Usage: <BotType> <Name> <Badge> <Color> <Role> <HeldItem> <ScaleX> <ScaleY> <ScaleZ>";
                return false;
            }

            string botTypeString = args.At(0);
            if (!Enum.TryParse(botTypeString, true, out BotType botType))
            {
                response = "Invalid BotType specified.";
                return false;
            }

            string name = args.At(1);
            string badge = args.At(2);
            string color = args.At(3);
            string roleString = args.At(4);
            string heldItemString = args.At(5);

            if (!Enum.TryParse(roleString, out RoleTypeId role) || !Enum.TryParse(heldItemString, out ItemType heldItem))
            {
                response = "Invalid Role or HeldItem specified.";
                return false;
            }

            if (!float.TryParse(args.At(6), out float scaleX) || !float.TryParse(args.At(7), out float scaleY) || !float.TryParse(args.At(8), out float scaleZ))
            {
                response = "Invalid Scale specified. Please provide three float values for X, Y, and Z components.";
                return false;
            }

            Vector3 scale = new(scaleX, scaleY, scaleZ);

            Npc bot = null;
            switch (botType)
            {
                case BotType.Buying:
                    bot = BuyingBot.SpawnBuyingBot(name, badge, color, role, heldItem, scale);
                    break;
                case BotType.Selling:
                    bot = SellingBot.SpawnSellingBot(name, badge, color, role, heldItem, scale);
                    break;
                default:
                    response = "Invalid BotType specified.";
                    return false;
            }

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
                    FoundationFortune.Singleton.ServerEvents.buyingBotPositions.Add(bot, bot.Position);
                });

                response = $"BuyingBot '{name}' added successfully and teleported to your position.";
                return true;
            }

            response = $"Failed to add the BuyingBot.";
            return false;
        }
    }
}
