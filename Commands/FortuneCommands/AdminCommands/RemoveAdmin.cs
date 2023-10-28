using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using System;
using Exiled.Permissions.Extensions;

namespace FoundationFortune.Commands.FortuneCommands.AdminCommands
{
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
            PlayerDataRepository.TogglePluginAdmin(ply, false);
            response = $"Done. {ply} is no longer an admin on FF.";
            return true;
        }
    }
}
