using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;
using FoundationFortune.API.EventSystems;

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

            switch (FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem)
            {
                case true when !ServerExtractionSystem.limitReached:
                    ServerExtractionSystem.DeactivateExtractionPoint(true);
                    break;
                case true:
                    ServerExtractionSystem.DeactivateExtractionPoint(false);
                    break;
            }

            response = $"Extraction zone event deactivated";
            return true;
        }
    }
}
