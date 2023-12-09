using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.API.Features.Commands.FortuneAdminCommands.AdminCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class AddAdmin : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_addadmin";
        public string Description { get; } = "Adds an admin to the plugin, granting infinite money and unlimited purchases.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<userId>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.adminsystem.add"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 1)
            {
                response = "Usage: ff_addadmin <userId>";
                return false;
            }

            string ply = args.At(0);
            PlayerDataRepository.TogglePluginAdmin(ply, true);
            response = $"Done. {ply} is now an admin on FF.";
            return true;
        }
    }
}
