using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;
using System;

namespace FoundationFortune.Commands.FortuneCommands.BountyCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class FlushAllBounties : ICommand
    {
        public string Command { get; } = "ff_flushbounties";
        public string Description { get; } = "Flush all bounties from the server.";
        public string[] Aliases { get; } = new string[] {};

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.bountysystem.flush") || !PlayerDataRepository.GetPluginAdmin(p.UserId))
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
