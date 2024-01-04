using System;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.Commands.NonAdminCommands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class Level : ICommand
{
    public string Command { get; } = "level";
    public string[] Aliases { get; } = { string.Empty };
    public string Description { get; } = "leveling system lol";

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        Player player = Player.Get(sender);

        switch (arguments.Count)
        {
            case 0:
                response = $"Your current level: {PlayerStatsRepository.GetLevel(player.UserId)}\nYour current experience: {PlayerStatsRepository.GetExperience(player.UserId)}\nYour current prestige level: {PlayerStatsRepository.GetPrestigeLevel(player.UserId)}";
                return true;
            case 1 when arguments.At(0).ToLower() == "up":
            {
                int expRequired = PlayerStatsRepository.GetLevel(player.UserId) * FoundationFortune.MoneyXPRewards.ExpToLevelUp;

                if (PlayerStatsRepository.GetExperience(player.UserId) >= expRequired)
                {
                    PlayerStatsRepository.LevelUp(player.UserId, false);
                    response = $"You leveled up! Current level: {PlayerStatsRepository.GetLevel(player.UserId)}";
                }
                else response = "Not enough experience to level up.";
                return true;
            }
            case 1 when arguments.At(0).ToLower() == "prestige":
            {
                if (PlayerStatsRepository.GetLevel(player.UserId) >= FoundationFortune.MoneyXPRewards.LevelUntilPrestige)
                {
                    PlayerStatsRepository.LevelUp(player.UserId, true);
                    response = $"You leveled up! Current prestige level: {PlayerStatsRepository.GetPrestigeLevel(player.UserId)}";
                }
                else response = "Not enough levels to prestige.";
                return true;
            }
            default:
                response = "Invalid usage. Use `.level` to check your stats or `.level up` to level up."; 
                return false;
        }
    }
}