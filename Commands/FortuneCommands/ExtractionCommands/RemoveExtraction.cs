using CommandSystem;
using Exiled.API.Enums;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.ExtractionCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class RemoveExtraction : ICommand
    {
        public string Command { get; } = "ff_deactivateextractionpoint";
        public string Description { get; } = "Deactivate an extraction zone event";
        public string[] Aliases { get; } = new string[] { };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.extractionsystem.deactivate"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (FoundationFortune.Singleton.Config.MoneyExtractionSystem && !FoundationFortune.Singleton.serverEvents.limitReached)
            {
                FoundationFortune.Singleton.serverEvents.DeactivateExtractionPoint(true);
            }
            else if (FoundationFortune.Singleton.Config.MoneyExtractionSystem)
            {
                FoundationFortune.Singleton.serverEvents.DeactivateExtractionPoint(false);
            }

            response = $"Extraction zone event deactivated";
            return true;
        }
    }
}
