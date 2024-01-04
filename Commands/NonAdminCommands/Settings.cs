using System;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Core.Database;
using RemoteAdmin;

namespace FoundationFortune.Commands.NonAdminCommands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class Settings : ICommand,IUsageProvider
{
    public string Command { get; } = "ff_settings";
    public string Description { get; } = "settings lmao (they only apply to you.)";
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

        string command = args.At(0).ToLowerInvariant();
        var player = Player.Get(playerSender);

        switch (command)
        {
            case "set":
                if (args.Count < 3)
                {
                    response = "Invalid usage. Correct usage: ff_settings set <property> <value>";
                    return false;
                }

                string propertyToSet = args.At(1).ToLowerInvariant();
                string valueToSet = args.At(2);
                return SetProperty(player, args, propertyToSet, valueToSet, out response);

            case "get":
                if (args.Count < 2)
                {
                    response = "Invalid usage. Correct usage: ff_settings get <property>";
                    return false;
                }

                string propertyToGet = args.At(1).ToLowerInvariant();
                return GetProperty(player, propertyToGet, out response);

            case "export":
                response = ExportSettings(player.UserId);
                return true;

            case "import":
                if (args.Count < 2)
                {
                    response = "Invalid usage. Correct usage: ff_settings import <code>";
                    return false;
                }

                string encodedSettings = args.At(1);
                return ImportSettings(player.UserId, encodedSettings, out response);

            default:
                response = "Invalid command. Use 'set', 'get', 'import', or 'export'.";
                return false;
        }
    }

    private static string ExportSettings(string userId)
    {
        bool hintMinmode = PlayerSettingsRepository.GetHintMinmode(userId);
        bool hintDisable = PlayerSettingsRepository.GetHintDisable(userId);
        bool hintExtension = PlayerSettingsRepository.GetHintExtension(userId);
        int hintLimit = PlayerSettingsRepository.GetHintLimit(userId);
        int hintSize = PlayerSettingsRepository.GetHintSize(userId);
        float hintAgeSeconds = PlayerSettingsRepository.GetHintAgeSeconds(userId);
        HintAlign hintAlign = PlayerSettingsRepository.GetHintAlign(userId);
        int sellingConfirmationTime = PlayerSettingsRepository.GetSellingConfirmationTime(userId);
        int activeAbilityActivationTime = PlayerSettingsRepository.GetActiveAbilityActivationTime(userId);

        string encoded = $"M{(hintMinmode ? 1 : 0)}" +
                         $"D{(hintDisable ? 1 : 0)}" +
                         $"E{(hintExtension ? 1 : 0)}" +
                         $"L{hintLimit}" +
                         $"S{hintSize}" +
                         $"A{hintAgeSeconds}" +
                         $"AL{(int)hintAlign}" +
                         $"SE{sellingConfirmationTime}" +
                         $"AC{activeAbilityActivationTime}";
        return encoded;
    }

    private static bool ImportSettings(string userId, string encodedSettings, out string response)
    {
        try
        {
            var keys = new (string Key, Type ValueType, int Min, int Max)[]
            {
                ("M", typeof(bool), 0, 1), // hintMinmode
                ("D", typeof(bool), 0, 1), // hintDisable
                ("E", typeof(bool), 0, 1), // hintExtension
                ("L", typeof(int), 0, 50), // hintLimit
                ("S", typeof(int), 0, 100), // hintSize
                ("A", typeof(float), 0, 100), // hintAgeSeconds
                ("AL", typeof(int), 0, 2), // hintAlign 
                ("SE", typeof(int), 0, 10), // sellingConfirmationTime
                ("AC", typeof(int), 0, 20) // activeAbilityActivationTime
            };

            int currentIndex = 0;
            int FindNextKeyIndex(string str, int startIndex)
            {
                for (int i = startIndex; i < str.Length; i++) if (!char.IsDigit(str[i]) && str[i] != '.') return i;
                return str.Length;
            }
            
            void ApplySetting(string id, string key, object value)
            {
                switch (key)
                {
                    case "M": PlayerSettingsRepository.SetHintMinmode(id, (bool)value); break;
                    case "D": PlayerSettingsRepository.SetHintDisable(id, (bool)value); break;
                    case "E": PlayerSettingsRepository.SetHintExtension(id, (bool)value); break;
                    case "L": PlayerSettingsRepository.SetHintLimit(id, (int)value); break;
                    case "S": PlayerSettingsRepository.SetHintSize(id, (int)value); break;
                    case "A": PlayerSettingsRepository.SetHintAgeSeconds(id, (float)value); break;
                    case "AL": PlayerSettingsRepository.SetHintAlign(id, (HintAlign)value); break;
                    case "SE": PlayerSettingsRepository.SetSellingConfirmationTime(id, (int)value); break;
                    case "AC": PlayerSettingsRepository.SetActiveAbilityConfirmationTime(id, (int)value); break;
                }
            }

            foreach (var (Key, ValueType, Min, Max) in keys)
            {
                currentIndex = encodedSettings.IndexOf(Key, currentIndex, StringComparison.Ordinal);
                if (currentIndex == -1) throw new ArgumentException($"Key {Key} not found in the correct order.");
                currentIndex += Key.Length;
                int valueEndIndex = FindNextKeyIndex(encodedSettings, currentIndex);
                string valueStr = encodedSettings.Substring(currentIndex, valueEndIndex - currentIndex);

                object value;
                if (ValueType == typeof(bool)) value = valueStr == "1";
                
                else if (ValueType == typeof(int))
                {
                    int intValue = int.Parse(valueStr);
                    if (intValue < Min || intValue > Max) throw new ArgumentException($"Value for {Key} out of range: {intValue}");
                    value = intValue;
                }
                else if (ValueType == typeof(float))
                {
                    float floatValue = float.Parse(valueStr);
                    if (floatValue < Min || floatValue > Max) throw new ArgumentException($"Value for {Key} out of range: {floatValue}");
                    value = floatValue;
                }
                else throw new ArgumentException($"Unsupported type for key {Key}");

                ApplySetting(userId, Key, value);
                currentIndex = valueEndIndex;
            }

            response = "Settings imported successfully.";
            return true;
        }
        catch (Exception ex)
        {
            response = $"Error in ImportSettings, you suck. {ex.Message}";
            return false;
        }
    }

    private static bool GetProperty(Player player, string property, out string response)
    {
        switch (property.ToLowerInvariant())
        {
            case "hintsize": response = $"HintSize is set to {PlayerSettingsRepository.GetHintSize(player.UserId)}";
                return true;
            case "hintminmode": response = $"HintMinmode is set to {PlayerSettingsRepository.GetHintMinmode(player.UserId)}";
                return true;
            case "hintdisable": response = $"HintDisable is set to {PlayerSettingsRepository.GetHintDisable(player.UserId)}";
                return true;
            case "hintadmin": response = $"HintAdmin is set to {PlayerSettingsRepository.GetPluginAdmin(player.UserId)}";
                return true;
            case "hintextension": response = $"HintExtension is set to {PlayerSettingsRepository.GetHintExtension(player.UserId)}";
                return true;
            case "hintlimit": response = $"HintLimit is set to {PlayerSettingsRepository.GetHintLimit(player.UserId)}";
                return true;
            case "hintageseconds": response = $"HintAgeSeconds is set to {PlayerSettingsRepository.GetHintAgeSeconds(player.UserId)}";
                return true;
            case "hintalign": response = $"HintAlign is set to {PlayerSettingsRepository.GetHintAlign(player.UserId)}";
                return true;
            case "activeabilityactivationtime": response = $"ActiveAbilityConfirmationTime is set to {PlayerSettingsRepository.GetActiveAbilityActivationTime(player.UserId)}";
                return true;
            case "sellingconfirmationtime": response = $"SellingConfirmationTime is set to {PlayerSettingsRepository.GetSellingConfirmationTime(player.UserId)}";
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
        StringBuilder commandList = new();
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
            
        PlayerSettingsRepository.SetActiveAbilityConfirmationTime(player.UserId, newTime);
            
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

        PlayerSettingsRepository.SetSellingConfirmationTime(player.UserId, newTime);

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

        PlayerSettingsRepository.SetHintSize(player.UserId, newSize);
        response = $"Your hint size has been set to {newSize}.";
        return true;
    }
        
    private static bool ToggleHintMinmode(Player player, out string response)
    {
        bool isMinmodeEnabled = PlayerSettingsRepository.GetHintMinmode(player.UserId);
        bool newMinmodeState = !isMinmodeEnabled;

        if (PlayerSettingsRepository.SetHintMinmode(player.UserId, newMinmodeState))
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

        PlayerSettingsRepository.SetHintLimit(player.UserId, newLimit);
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

        PlayerSettingsRepository.SetHintAgeSeconds(player.UserId, newLimit);
        response = $"Your hint age has been set to {newLimit}.";
        return true;
    }
        
    private static bool ToggleHintDisable(Player player, out string response)
    {
        bool isHintSystemDisabled = PlayerSettingsRepository.GetHintDisable(player.UserId);

        if (PlayerSettingsRepository.SetHintDisable(player.UserId, !isHintSystemDisabled))
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

        PlayerSettingsRepository.SetHintAlign(player.UserId, align);
        response = $"Your hint alignment has been set to {align}.";
        return true;
    }
        
    private static bool ToggleHintExtension(Player player, out string response)
    {
        bool isHintSystemDisabled = PlayerSettingsRepository.GetHintExtension(player.UserId);

        if (PlayerSettingsRepository.SetHintExtension(player.UserId, !isHintSystemDisabled))
        {
            response = isHintSystemDisabled ? "The hint system is no longer extended." : "The hint system is now extended.";
            return true;
        }

        response = "Failed to extend the hint system. How. What did you do.";
        return false;
    }
    #endregion
}