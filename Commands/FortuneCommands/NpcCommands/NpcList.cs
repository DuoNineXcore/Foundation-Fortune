﻿using CommandSystem;
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
        public string[] Aliases { get; } = new string[] {};

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

            if (FoundationFortune.Singleton.BuyingBotIndexation == null)
            {
                response = "FoundationFortune.Singleton.BuyingBotIndexation is null.";
                return false;
            }

            var buyingBots = FoundationFortune.Singleton.BuyingBotIndexation.Values.ToList();

            if (buyingBots.Any())
            {
                response = "\tList of BuyingBots in the server:\n";

                foreach (var botInfo in buyingBots) response += $"Indexation Number: {botInfo.indexation}, Name: {(botInfo.bot != null ? botInfo.bot.Nickname : "Null Nickname  ")}\n";
            }
            else
            {
                response = "No BuyingBots found in the server.";
            }

            return true;
        }
    }
}