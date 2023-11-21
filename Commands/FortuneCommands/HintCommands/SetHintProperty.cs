using System;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Enums.Hints;
using RemoteAdmin;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class SetHintProperty : ICommand,IUsageProvider
    {
        public string Command { get; } = "ff_sethintproperty";
        public string Description { get; } = "Set your own Hint System settings. They do not apply to other players. Only you.";
        public string[] Aliases { get; } = new string[] { };
        public string[] Usage { get; } = new string[] { 
            "<hintanim> <left/center/right> \n",
            "<hintminmode> <true/false>     \n",
            "<hintdisable> <true/false>     \n",
            "<hintlimit> <0-50>             \n",
            "<hintsize> <0-100>             \n",
            "<hintalign> <left/center/right>\n",
            "<hintextension> <true/false>   \n",
        };
        
        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 2)
            {
                response = "Invalid usage. Correct usage: ff_sethintproperty <property> <value>";
                return false;
            }

            string property = args.At(0).ToLowerInvariant();
            string value = args.At(1);
            var player = Player.Get(playerSender);

            if (HintProperty(player, args, property, value, out response)) return true;
            return false;
        }

        private bool HintProperty(Player player, ArraySegment<string> args, string property, string value, out string response)
        {
            switch (property)
            {
                case "hintsize": return SetHintSize(player, value, out response);
                case "hintminmode": return ToggleHintMinmode(player, out response);
                case "hintlimit": return SetHintLimit(args, player, out response);
                case "hintdisable": return ToggleHintDisable(player, out response);
                case "hintanim": return SetHintAnim(args, player, out response);
                case "hintseconds": return SetHintAnim(args, player, out response);
                case "hintalign": return SetHintAlign(args, player, out response);
                case "hintextension": return ToggleHintExtension(player, out response);
                default: response = $"Unknown hint property: {property}"; return false;
            }
        }

        #region this sucks but its better than doing a bunch of different commands that are almost the same
        private bool SetHintSize(Player player, string sizeArg, out string response)
        {
            if (!int.TryParse(sizeArg, out int newSize) || newSize < 1 || newSize > 100)
            {
                response = "Invalid hint size. It must be between 0 and 100";
                return false;
            }

            PlayerDataRepository.SetHintSize(player.UserId, newSize);
            response = $"Your hint size has been set to {newSize}.";
            return true;
        }
        
        private bool ToggleHintMinmode(Player player, out string response)
        {
            bool isMinmodeEnabled = PlayerDataRepository.GetHintMinmode(player.UserId);
            bool newMinmodeState = !isMinmodeEnabled;

            if (PlayerDataRepository.ToggleHintMinmode(player.UserId, newMinmodeState))
            {
                response = newMinmodeState ? "Hint Minmode is now enabled." : "Hint Minmode is now disabled.";
                return true;
            }
            else
            {
                response = "Failed to toggle Hint Minmode.";
                return false;
            }
        }

        private bool SetHintLimit(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 1 || !int.TryParse(args.At(0), out int newLimit))
            {
                response = "Invalid usage. Correct usage: hintlimit <opacity>";
                return false;
            }

            if (newLimit < 0 || newLimit > 50)
            {
                response = "Hint limit must be between 0 and 50.";
                return false;
            }

            PlayerDataRepository.SetHintLimit(player.UserId, newLimit);
            response = $"Your max hint limit has been set to {newLimit}.";
            return true;
        }
        
        private bool SetHintSeconds(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 1 || !int.TryParse(args.At(0), out int newLimit))
            {
                response = "Invalid usage. Correct usage: hintseconds <opacity>";
                return false;
            }

            if (newLimit < 0 || newLimit > 30)
            {
                response = "Hint limit must be between 0 and 30.";
                return false;
            }

            PlayerDataRepository.SetHintSeconds(player.UserId, newLimit);
            response = $"Your max hint limit has been set to {newLimit}.";
            return true;
        }
        
        private bool ToggleHintDisable(Player player, out string response)
        {
            bool isHintSystemDisabled = PlayerDataRepository.GetHintDisable(player.UserId);

            if (PlayerDataRepository.ToggleHintDisable(player.UserId, !isHintSystemDisabled))
            {
                response = isHintSystemDisabled ? "The hint system is now disabled." : "The hint system is now enabled.";
                return true;
            }
            else
            {
                response = "Failed to toggle the hint system.";
                return false;
            }
        }
        
        private bool SetHintAnim(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 1 || !Enum.TryParse(args.At(0), true, out HintAnim anim))
            {
                response = "Invalid usage. Correct usage: ff_hintanim <right/left/center>";
                return false;
            }

            PlayerDataRepository.SetHintAnim(player.UserId, anim);
            response = $"Your hint animation has been set to {anim}.";
            return true;
        }
        
        private bool SetHintAlign(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 1 || !Enum.TryParse(args.At(0), true, out HintAlign align))
            {
                response = "Invalid usage. Correct usage: ff_hintalign <right/left/center>";
                return false;
            }

            PlayerDataRepository.SetUserHintAlign(player.UserId, align);
            response = $"Your hint alignment has been set to {align}.";
            return true;
        }
        
        private bool ToggleHintExtension(Player player, out string response)
        {
            bool isHintSystemDisabled = PlayerDataRepository.GetHintExtension(player.UserId);

            if (PlayerDataRepository.ToggleHintExtension(player.UserId, !isHintSystemDisabled))
            {
                response = isHintSystemDisabled ? "The hint system is no longer extended." : "The hint system is now extended.";
                return true;
            }
            else
            {
                response = "Failed to extend the hint system. How. What did you do.";
                return false;
            }
        }
        #endregion
    }
}
