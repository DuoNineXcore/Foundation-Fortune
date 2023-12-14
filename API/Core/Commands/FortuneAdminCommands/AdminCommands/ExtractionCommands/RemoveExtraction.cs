using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Core.Commands.FortuneAdminCommands.AdminCommands.ExtractionCommands;

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
            case true when !ExtractionSystem.LimitReached:
                ExtractionSystem.DeactivateExtractionPoint(true);
                break;
            case true:
                ExtractionSystem.DeactivateExtractionPoint(false);
                break;
        }

        response = $"Extraction zone event deactivated";
        return true;
    }
}