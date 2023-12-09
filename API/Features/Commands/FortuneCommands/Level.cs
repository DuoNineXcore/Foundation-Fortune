using System;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.API.Features.Commands.FortuneCommands
{
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
                    response = $"Your current level: {PlayerDataRepository.GetLevel(player.UserId)}\nYour current experience: {PlayerDataRepository.GetExperience(player.UserId)}\nYour current prestige level: {PlayerDataRepository.GetPrestigeLevel(player.UserId)}";
                    return true;
                case 1 when arguments.At(0).ToLower() == "up":
                {
                    int expRequired = PlayerDataRepository.GetLevel(player.UserId) * FoundationFortune.MoneyXPRewards.ExpToLevelUp;

                    if (PlayerDataRepository.GetExperience(player.UserId) >= expRequired)
                    {
                        PlayerDataRepository.LevelUp(player.UserId, false);
                        response = $"You leveled up! Current level: {PlayerDataRepository.GetLevel(player.UserId)}";
                    }
                    else response = "Not enough experience to level up.";
                    return true;
                }
                case 1 when arguments.At(0).ToLower() == "prestige":
                {
                    if (PlayerDataRepository.GetLevel(player.UserId) >= FoundationFortune.MoneyXPRewards.LevelUntilPrestige)
                    {
                        PlayerDataRepository.LevelUp(player.UserId, true);
                        response = $"You leveled up! Current prestige level: {PlayerDataRepository.GetPrestigeLevel(player.UserId)}";
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
}

