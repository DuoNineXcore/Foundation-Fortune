using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintLimit : ICommand
    {
        public string Command { get; } = "ff_hintlimits";
        public string Description { get; } = "Maximum limit of hints that will be displayed to you";
        public string[] Aliases { get; } = new string[] { "<hints>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 1 || !int.TryParse(args.At(0), out int newLimit))
            {
                response = "Invalid usage. Correct usage: ff_hintlimit <opacity>";
                return false;
            }

            if (newLimit < 0 || newLimit > 50)
            {
                response = "Hint limit must be between 0 and 50.";
                return false;
            }

            Player player = Player.Get(playerSender);
            PlayerDataRepository.SetHintLimit(player.UserId, newLimit);
            response = $"Your max hint limit has been set to {newLimit}.";
            return true;
        }
    }
}