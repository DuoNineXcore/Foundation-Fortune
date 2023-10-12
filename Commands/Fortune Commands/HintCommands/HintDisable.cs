using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.Fortune_Commands.HintCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintDisable : ICommand
    {
        public string Command { get; } = "ff_togglehintsystem";
        public string Description { get; } = "Enable or disable the hint system, this only applies to you.";
        public string[] Aliases { get; } = new string[] { string.Empty };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            Player playersender = Player.Get(playerSender);
            var isHintSystemDisabled = PlayerDataRepository.GetHintDisable(playersender.UserId);

            if (isHintSystemDisabled)
            {
                if (PlayerDataRepository.ToggleHintDisable(playersender.UserId, false)) response = "The hint system is now disabled.";
                else response = "Failed to disable the hint system.";
            }
            else
            {
                if (PlayerDataRepository.ToggleHintDisable(playersender.UserId, true)) response = "The hint system is now enabled.";
                else response = "Failed to enable the hint system.";
            }
            return true;
        }
    }
}
