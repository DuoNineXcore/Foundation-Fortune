using CommandSystem;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using CustomPlayerEffects;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands
{
    internal class HintAlignment : ICommand
    {
        public string Command { get; } = "hintalign";
        public string Description { get; } = "Set the hint alignment.";
        public string[] Aliases { get; } = new string[] { "sethintalign" };

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
                response = $"Not enough args. [{args.Count}] Usage: align <center/left/right>";
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
