using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;

namespace FoundationFortune.Commands.FortuneCommands.AdminCommands.ExtractionCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class RemoveExtraction : ICommand
    {
        public string Command { get; } = "ff_removeextraction";
        public string Description { get; } = "Deactivate an extraction zone event";
        public string[] Aliases { get; } = new string[] { };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.extractionsystem.deactivate") && !PlayerDataRepository.GetPluginAdmin(p.UserId))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            switch (FoundationFortune.Singleton.Config.MoneyExtractionSystem)
            {
                case true when !FoundationFortune.Singleton.ServerEvents.limitReached:
                    FoundationFortune.Singleton.ServerEvents.DeactivateExtractionPoint(true);
                    break;
                case true:
                    FoundationFortune.Singleton.ServerEvents.DeactivateExtractionPoint(false);
                    break;
            }

            response = $"Extraction zone event deactivated";
            return true;
        }
    }
}
