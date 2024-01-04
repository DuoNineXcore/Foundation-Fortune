using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.Commands.AdminCommands.BountySystem;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class FlushAllBounties : ICommand
{
    public string Command => "ff_flushbounties";
    public string Description => "Flush all bounties from the server.";
    public string[] Aliases { get; } = {};

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        Player p = Player.Get(sender);

        if (!sender.CheckPermission("ff.bountysystem.flush") && !PlayerSettingsRepository.GetPluginAdmin(p.UserId))
        {
            response = "You do not have permission to use this section.";
            return false;
        }

        API.Core.Systems.BountySystem.BountiedPlayers.Clear();
        response = "All bounties have been flushed.";
        return true;
    }
}