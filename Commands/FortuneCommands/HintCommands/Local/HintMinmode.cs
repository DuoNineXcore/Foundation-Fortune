using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintMinmode : ICommand
    {
        public string Command { get; } = "ff_hintminmode";
        public string Description { get; } = "Toggle hint minmode.";
        public string[] Aliases { get; } = new string[] { "what do you think this does" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            Player playersender = Player.Get(playerSender);
            bool isMinmodeEnabled = PlayerDataRepository.GetHintMinmode(playersender.UserId);

            if (isMinmodeEnabled)
            {
                if (PlayerDataRepository.ToggleHintMinmode(playersender.UserId, false)) response = "Hint Minmode is now disabled.";
                else response = "Failed to disable Hint Minmode.";
            }
            else
            {
                if (PlayerDataRepository.ToggleHintMinmode(playersender.UserId, true)) response = "Hint Minmode is now enabled.";
                else response = "Failed to enable Hint Minmode.";
            }
            return true;
        }
    }
}
