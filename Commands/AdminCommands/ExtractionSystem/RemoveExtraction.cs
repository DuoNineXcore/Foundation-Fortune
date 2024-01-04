using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.Commands.AdminCommands.ExtractionSystem;

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

        if (!sender.CheckPermission("ff.extractionsystem.deactivate") && !PlayerSettingsRepository.GetPluginAdmin(p.UserId))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        switch (FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem)
        {
            case true when !API.Core.Systems.ExtractionSystem.LimitReached:
                API.Core.Systems.ExtractionSystem.DeactivateExtractionPoint(true);
                break;
            case true:
                API.Core.Systems.ExtractionSystem.DeactivateExtractionPoint(false);
                break;
        }

        response = "Extraction zone event deactivated";
        return true;
    }
}