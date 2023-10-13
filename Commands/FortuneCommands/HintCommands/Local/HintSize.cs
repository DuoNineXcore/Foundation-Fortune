using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintSize : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_hintsize";
        public string Description { get; } = "Change your hint size.";
        public string[] Aliases { get; } = new string[] { };
        public string[] Usage { get; } = new string[] { "<size (0 - 100)>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 1 || !int.TryParse(args.At(0), out int newSize))
            {
                response = "Invalid usage. Correct usage: ff_hintsize <size>";
                return false;
            }

            if (newSize < 1 || newSize > 100)
            {
                response = "Hint size must be between 1 and 100.";
                return false;
            }

            Player player = Player.Get(playerSender);
            PlayerDataRepository.SetHintSize(player.UserId, newSize);
            response = $"Your hint size has been set to {newSize}.";
            return true;
        }
    }

}
