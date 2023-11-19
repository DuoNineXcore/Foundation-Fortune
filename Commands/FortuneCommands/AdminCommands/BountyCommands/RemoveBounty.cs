﻿using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;
using FoundationFortune.API.EventSystems;

namespace FoundationFortune.Commands.FortuneCommands.AdminCommands.BountyCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class RemoveBounty : ICommand
    {
        public string Command { get; } = "ff_removebounty";
        public string Description { get; } = "Remove a bounty from a player.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<playerName>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.bountysystem.remove") && !PlayerDataRepository.GetPluginAdmin(p.UserId))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            if (args.Count < 1)
            {
                response = "Usage: foundationfortune bounty removebounty <playerName>";
                return false;
            }

            string playerName = args.At(0);
            if (!Player.TryGet(playerName, out Player player))
            {
                response = $"Player '{playerName}' not found.";
                return false;
            }

            var bountiedPlayer = ServerBountySystem.BountiedPlayers.FirstOrDefault(bounty => bounty.Player == player && bounty.IsBountied);
            if (bountiedPlayer != null)
            {
                ServerBountySystem.StopBounty(player);
                response = $"Bounty removed from {player.Nickname}.";
                return true;
            }
            else
            {
                response = $"{player.Nickname} doesn't have an active bounty.";
                return false;
            }
        }
    }
}
