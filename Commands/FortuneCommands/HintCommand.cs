using CommandSystem;
using FoundationFortune.API.Database;
using FoundationFortune.Events;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using FoundationFortune.API.Models.Enums;

namespace FoundationFortune.Commands.FortuneCommands
{
    internal class HintCommand : ICommand
    {
        public string Command { get; } = "hint";
        public string Description { get; } = "Hint stuff";
        public string[] Aliases { get; } = new string[] { "hn" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (args.Count < 1)
            {
                response = "Usage: foundationfortune hint <minmode/align>";
                return false;
            }

            string subcommand = args.At(0).ToLower();

            switch (subcommand)
            {
                case "minmode":
                    return ToggleHintMinmode(args, sender, out response);
                case "align":
                    return SetHintAlignment(args, sender, out response);
                default:
                    response = "Invalid subcommand for 'hint'. Use 'minmode' or 'align'.";
                    return false;
            }
        }

        private bool ToggleHintMinmode(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            Player playersender = Player.Get(playerSender);
            var isMinmodeEnabled = PlayerDataRepository.GetHintMinmode(playersender.UserId);

            if (isMinmodeEnabled)
            {
                if (PlayerDataRepository.SetHintMinmode(playersender.UserId, false))
                {
                    response = "Hint Minmode is now disabled.";
                }
                else
                {
                    response = "Failed to disable Hint Minmode.";
                }
            }
            else
            {
                if (PlayerDataRepository.SetHintMinmode(playersender.UserId, true))
                {
                    response = "Hint Minmode is now enabled.";
                }
                else
                {
                    response = "Failed to enable Hint Minmode.";
                }
            }

            return true;
        }

        private bool SetHintAlignment(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            Player player = Player.Get(playerSender);

            if (arguments.Count < 2)
            {
                response = $"Not enough arguments. [{arguments.Count}] Usage: align <center/left/right>";
                return false;
            }

            string alignmentStr = arguments.At(1).ToLower();

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

