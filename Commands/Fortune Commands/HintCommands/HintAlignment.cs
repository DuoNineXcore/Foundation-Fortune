using CommandSystem;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using CustomPlayerEffects;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands
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

            Player player = Player.Get(playerSender);

            if (args.Count < 2)
            {
                response = $"Not enough arguments. [{args.Count}] Usage: align <center/left/right>";
                return false;
            }

            string alignmentStr = args.At(1).ToLower();

            if (Enum.TryParse(alignmentStr, true, out HintAlign alignment))
            {
                PlayerDataRepository.SetUserHintAlign(player.UserId, alignment);
                response = $"Hint alignment set to {alignment}.";
                return true;
            }
            else
            {
                response = "Invalid alignment. Use 'center', 'left', or 'right'.";
                return false;
            }
        }
    }
}
