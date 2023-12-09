using System;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Enums.Systems.HintSystem;
using RemoteAdmin;

namespace FoundationFortune.API.Features.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class Settings : ICommand,IUsageProvider
    {
        public string Command { get; } = "ff_settings";
        public string Description { get; } = "Set your own settings. They do not apply to other players. Only you.";
        public string[] Aliases { get; } = new string[] { };
        public string[] Usage { get; } = new string[] { 
            "<hintminmode> <true/false>     \n",
            "<hintdisable> <true/false>     \n",
            "<hintlimit> <0-50>             \n",
            "<hintsize> <0-100>             \n",
            "<hintageseconds> <0-100>             \n",
            "<hintalign> <left/center/right>\n",
            "<hintextension> <true/false>   \n",
            "<activeabilityactivationtime> <0-20>   \n",
            "<sellingconfirmationtime> <0-10>   \n",
        };
        
        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }
            
            if (args.Count == 0 || args.At(0).Equals("help", StringComparison.OrdinalIgnoreCase) || args.At(0).Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                response = ListCommands();
                return true;
            }

            if (args.Count < 2)
            {
                response = "Invalid usage. Correct usage: ff_settings <property> <value>";
                return false;
            }

            string command = args.At(0).ToLowerInvariant();
            string property = args.At(1).ToLowerInvariant();
            string value = args.Count > 2 ? args.At(2) : null;

            var player = Player.Get(playerSender);

            switch (command)
            {
                case "set": return SetProperty(player, args, property, value, out response);
                case "get": return GetProperty(player, property, out response);
                default: response = "Invalid command. Use 'set' or 'get'.";
                    return false;
            }
        }

        private static bool GetProperty(Player player, string property, out string response)
        {
            switch (property.ToLowerInvariant())
            {
                case "hintsize": response = $"HintSize is set to {PlayerDataRepository.GetHintSize(player.UserId)}";
                    return true;
                case "hintminmode": response = $"HintMinmode is set to {PlayerDataRepository.GetHintMinmode(player.UserId)}";
                    return true;
                case "hintdisable": response = $"HintDisable is set to {PlayerDataRepository.GetHintDisable(player.UserId)}";
                    return true;
                case "hintadmin": response = $"HintAdmin is set to {PlayerDataRepository.GetPluginAdmin(player.UserId)}";
                    return true;
                case "hintextension": response = $"HintExtension is set to {PlayerDataRepository.GetHintExtension(player.UserId)}";
                    return true;
                case "hintlimit": response = $"HintLimit is set to {PlayerDataRepository.GetHintLimit(player.UserId)}";
                    return true;
                case "hintageseconds": response = $"HintAgeSeconds is set to {PlayerDataRepository.GetHintAgeSeconds(player.UserId)}";
                    return true;
                case "hintalign": response = $"HintAlign is set to {PlayerDataRepository.GetHintAlign(player.UserId)}";
                    return true;
                case "activeabilityactivationtime": response = $"ActiveAbilityConfirmationTime is set to {PlayerDataRepository.GetActiveAbilityActivationTime(player.UserId)}";
                    return true;
                case "sellingconfirmationtime": response = $"SellingConfirmationTime is set to {PlayerDataRepository.GetSellingConfirmationTime(player.UserId)}";
                    return true;
                default: response = $"Unknown hint property: {property}";
                    return false;
            }
        }
        
        private static bool SetProperty(Player player, ArraySegment<string> args, string property, string value, out string response)
        {
            switch (property)
            {
                case "hintsize": return SetHintSize(player, value, out response);
                case "hintminmode": return ToggleHintMinmode(player, out response);
                case "hintlimit": return SetHintLimit(args, player, out response);
                case "hintdisable": return ToggleHintDisable(player, out response);
                case "hintageseconds": return SetHintAgeSeconds(args, player, out response);
                case "hintalign": return SetHintAlign(args, player, out response);
                case "hintextension": return ToggleHintExtension(player, out response);
                case "activeabilityactivationtime": return SetActiveAbilityActivationTime(player, value, out response);
                case "sellingconfirmationtime": return SetSellingConfirmationTime(player, value, out response);
                default: response = $"Unknown hint property: {property}"; return false;
            }
        }

        private static string ListCommands()
        {
            StringBuilder commandList = new StringBuilder();
            commandList.AppendLine("Available setter commands:");
            commandList.AppendLine("ff_settings set hintsize <0-100> - Set your hint size.");
            commandList.AppendLine("ff_settings set hintminmode <true/false> - Toggle Hint Minmode.");
            commandList.AppendLine("ff_settings set hintlimit <0-50> - Set your max hint limit.");
            commandList.AppendLine("ff_settings set hintdisable <true/false> - Toggle the hint system.");
            commandList.AppendLine("ff_settings set hintageseconds <0-30> - Set your max hint age in seconds.");
            commandList.AppendLine("ff_settings set hintalign <right/left/center> - Set your hint alignment.");
            commandList.AppendLine("ff_settings set hintextension <true/false> - Toggle hint system extension.");
            commandList.AppendLine("ff_settings set activeabilityactivationtime <1-20> - Set the confirmation window to activate an ability from a perk.");
            commandList.AppendLine("ff_settings set sellingconfirmationtime <1-10> - Set the confirmation window to sell an item.");
            return commandList.ToString();
        }
        
        #region this sucks but its better than doing a bunch of different commands that are almost the same
        private static bool SetActiveAbilityActivationTime(Player player, string value, out string response)
        {
            if (!int.TryParse(value, out int newTime) || newTime < 1 || newTime > 20)
            {
                response = "Invalid value for active ability activation time. It must be between 1 and 20.";
                return false;
            }
            
            PlayerDataRepository.SetActiveAbilityConfirmationTime(player.UserId, newTime);
            
            response = $"Active ability activation time has been set to {newTime}.";
            return true;
        }

        private static bool SetSellingConfirmationTime(Player player, string value, out string response)
        {
            if (!int.TryParse(value, out int newTime) || newTime < 1 || newTime > 10)
            {
                response = "Invalid value for selling confirmation time. It must be between 1 and 10.";
                return false;
            }

            PlayerDataRepository.SetSellingConfirmationTime(player.UserId, newTime);

            response = $"Selling confirmation time has been set to {newTime}.";
            return true;
        }

        private static bool SetHintSize(Player player, string sizeArg, out string response)
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
        
        private static bool ToggleHintMinmode(Player player, out string response)
        {
            bool isMinmodeEnabled = PlayerDataRepository.GetHintMinmode(player.UserId);
            bool newMinmodeState = !isMinmodeEnabled;

            if (PlayerDataRepository.ToggleHintMinmode(player.UserId, newMinmodeState))
            {
                response = newMinmodeState ? "Hint Minmode is now enabled." : "Hint Minmode is now disabled.";
                return true;
            }

            response = "Failed to toggle Hint Minmode.";
            return false;
        }

        private static bool SetHintLimit(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 2 || !int.TryParse(args.At(1), out int newLimit))
            {
                response = "Invalid usage. Correct usage: hintlimit <limit>";
                return false;
            }

            if (newLimit is < 0 or > 50)
            {
                response = "Hint limit must be between 0 and 50.";
                return false;
            }

            PlayerDataRepository.SetHintLimit(player.UserId, newLimit);
            response = $"Your max hint limit has been set to {newLimit}.";
            return true;
        }
        
        private static bool SetHintAgeSeconds(ArraySegment<string> args, Player player, out string response)
        {
            if (args.Count < 3 || !int.TryParse(args.At(2), out int newLimit))
            {
                response = "Invalid usage. Correct usage: hintageseconds <age, in seconds of course.>";
                return false;
            }

            if (newLimit is < 0 or > 30)
            {
                response = "Hint age must be between 0 and 30.";
                return false;
            }

            PlayerDataRepository.SetHintAgeSeconds(player.UserId, newLimit);
            response = $"Your hint age has been set to {newLimit}.";
            return true;
        }
        
        private static bool ToggleHintDisable(Player player, out string response)
        {
            bool isHintSystemDisabled = PlayerDataRepository.GetHintDisable(player.UserId);

            if (PlayerDataRepository.ToggleHintDisable(player.UserId, !isHintSystemDisabled))
            {
                response = isHintSystemDisabled ? "The hint system is now disabled." : "The hint system is now enabled.";
                return true;
            }

            response = "Failed to toggle the hint system.";
            return false;
        }

        private static bool SetHintAlign(ArraySegment<string> args, Player player, out string response)
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
        
        private static bool ToggleHintExtension(Player player, out string response)
        {
            bool isHintSystemDisabled = PlayerDataRepository.GetHintExtension(player.UserId);

            if (PlayerDataRepository.ToggleHintExtension(player.UserId, !isHintSystemDisabled))
            {
                response = isHintSystemDisabled ? "The hint system is no longer extended." : "The hint system is now extended.";
                return true;
            }

            response = "Failed to extend the hint system. How. What did you do.";
            return false;
        }
        #endregion
    }
}
