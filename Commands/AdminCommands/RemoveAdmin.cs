using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.Commands.AdminCommands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class RemoveAdmin : ICommand, IUsageProvider
{
    public string Command { get; } = "ff_removeadmin";
    public string Description { get; } = "Removes a plugin admin.";
    public string[] Aliases { get; } = new string[] { };
    public string[] Usage { get; } = new string[] { "<userId>" };

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ff.adminsystem.remove"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (args.Count < 1)
        {
            response = "Usage: ff_removeadmin <userId>";
            return false;
        }

        string ply = args.At(0);
        PlayerSettingsRepository.SetHintAdmin(ply, false);
        response = $"Done. {ply} is no longer an admin on Foundation Fortune.";
        return true;
    }
}