using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;

namespace FoundationFortune.Commands.FortuneCommands.AdminCommands.BountyCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class AddBounty : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_addbounty";
        public string Description { get; } = "Add a bounty to a player.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<s64> <amount> <durationInSeconds>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.bountysystem.add") && !PlayerDataRepository.GetPluginAdmin(p.UserId))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 3)
            {
                response = "Usage: ff_addbounty <s64> <amount> <durationInSeconds>";
                return false;
            }

            Player player = Player.Get(args.At(0));
            
            if (!int.TryParse(args.At(1), out int bountyAmount) || bountyAmount <= 0)
            {
                response = "Invalid bounty amount.";
                return false;
            }

            if (!int.TryParse(args.At(2), out int durationInSeconds) || durationInSeconds <= 0)
            {
                response = "Invalid duration value in seconds.";
                return false;
            }

            TimeSpan bountyDuration = TimeSpan.FromSeconds(durationInSeconds);
            FoundationFortune.Singleton.ServerEvents.AddBounty(player, bountyAmount, bountyDuration);
            response = $"Bounty of ${bountyAmount} added to {player.Nickname} for {durationInSeconds} seconds.";
            return true;
        }
    }
}
