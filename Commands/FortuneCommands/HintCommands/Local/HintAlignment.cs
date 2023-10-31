using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using FoundationFortune.API.Models;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintAlignment : ICommand,IUsageProvider
    {
        public string Command { get; } = "ff_hintalign";
        public string Description { get; } = "Set the hint system's alignment, this only applies to you.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<center/left/right>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 1 || !Enum.TryParse(args.At(0), true, out HintAlign align))
            {
                response = "Invalid usage. Correct usage: ff_hintalign <right/left/center>";
                return false;
            }

            Player player = Player.Get(playerSender);

            PlayerDataRepository.SetUserHintAlign(player.UserId, align);
            response = $"Your hint animation has been set to {align}.";
            return true;
        }
    }
}
