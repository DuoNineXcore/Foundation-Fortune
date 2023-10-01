using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Linq;

namespace FoundationFortune.Commands.FortuneCommands
{
    internal class BountyCommand : ICommand
    {
        public string Command { get; } = "bounty";
        public string Description { get; } = "Bounty stuff";
        public string[] Aliases { get; } = new string[] { "bt" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.bountysystem"))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            if (args.Count < 1)
            {
                response = "Usage: foundationfortune bounty <addbounty/removebounty/flush>";
                return false;
            }

            string subcommand = args.At(0).ToLower();

            switch (subcommand)
            {
                case "addbounty":
                    return AddBountyToPlayer(args, sender, out response);
                case "removebounty":
                    return RemoveBountyFromPlayer(args, sender, out response);
                case "flush":
                    return FlushAllBounties(args, sender, out response);
                default:
                    response = "Invalid subcommand for 'bounty'.";
                    return false;
            }
        }

        private bool AddBountyToPlayer(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.bountysystem.add"))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            if (args.Count < 4)
            {
                response = "Usage: foundationfortune bounty addbounty <playerName> <amount> <durationInSeconds>";
                return false;
            }

            string playerName = args.At(1);
            if (!Player.TryGet(playerName, out Player player))
            {
                response = $"Player '{playerName}' not found.";
                return false;
            }

            if (!int.TryParse(args.At(2), out int bountyAmount) || bountyAmount <= 0)
            {
                response = "Invalid bounty amount.";
                return false;
            }

            if (!int.TryParse(args.At(3), out int durationInSeconds) || durationInSeconds <= 0)
            {
                response = "Invalid duration value in seconds.";
                return false;
            }

            TimeSpan bountyDuration = TimeSpan.FromSeconds(durationInSeconds);
            FoundationFortune.Singleton.serverEvents.AddBounty(player, bountyAmount, bountyDuration);
            response = $"Bounty of ${bountyAmount} added to {player.Nickname} for {durationInSeconds} seconds.";
            return true;
        }

        private bool RemoveBountyFromPlayer(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.bountysystem.remove"))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            if (args.Count < 2)
            {
                response = "Usage: foundationfortune bounty removebounty <playerName>";
                return false;
            }

            string playerName = args.At(1);
            if (!Player.TryGet(playerName, out Player player))
            {
                response = $"Player '{playerName}' not found.";
                return false;
            }

            var bountiedPlayer = FoundationFortune.Singleton.serverEvents.BountiedPlayers.FirstOrDefault(bounty => bounty.Player == player && bounty.IsBountied);
            if (bountiedPlayer != null)
            {
                FoundationFortune.Singleton.serverEvents.StopBounty(player);
                response = $"Bounty removed from {player.Nickname}.";
                return true;
            }
            else
            {
                response = $"{player.Nickname} doesn't have an active bounty.";
                return false;
            }
        }

        private bool FlushAllBounties(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.bountysystem.flush"))
            {
                response = "You do not have permission to use this section.";
                return false;
            }

            FoundationFortune.Singleton.serverEvents.BountiedPlayers.Clear();
            response = "All bounties have been flushed.";
            return true;
        }
    }
}
