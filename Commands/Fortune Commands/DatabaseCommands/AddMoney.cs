using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using Exiled.API.Features;

namespace FoundationFortune.Commands.FortuneCommands.DatabaseCommands
{
    internal class AddMoney : ICommand
    {
        public string Command { get; } = "addmoney";
        public string Description { get; } = "Add money to a player's account.";
        public string[] Aliases { get; } = new string[] { "givemoney" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            var pluginTranslations = FoundationFortune.Singleton.Translation;
            if (!sender.CheckPermission("ff.database.addmoney"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 1)
            {
                response = "Usage: foundationfortune database addmoney <self/steamid/all> [amount]";
                return false;
            }

            string addmoneytarget = args.At(0).ToLower();

            switch (addmoneytarget)
            {
                case "self":
                    if (sender is not PlayerCommandSender self)
                    {
                        response = "Command sender is not a player.";
                        return false;
                    }

                    var ply = Player.Get(self);

                    if (!int.TryParse(args.At(1), out int amount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    string SelfAddMoney = pluginTranslations.AllAddMoney.Replace("%amount%", amount.ToString());
                    FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"{SelfAddMoney}", amount, 5f, false, false);
                    response = $"Gave {amount} money to player '{ply}'.";
                    return true;

                case "all":
                    if (!int.TryParse(args.At(1), out int allAmount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    string AllAddMoney = pluginTranslations.AllAddMoney.Replace("%amount%", allAmount.ToString());
                    foreach (var player in Player.List) FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"{AllAddMoney}", allAmount, 5f, false, false);
                    response = $"Gave {allAmount} money to all players.";
                    return true;

                default:
                    if (!int.TryParse(args.At(1), out int steamIdAmount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    var targetPlayer = Player.Get(addmoneytarget);
                    string SteamIDAddMoney = pluginTranslations.SteamIDAddMoney.Replace("%amount%", steamIdAmount.ToString());
                    if (targetPlayer != null) FoundationFortune.Singleton.serverEvents.EnqueueHint(targetPlayer, $"{SteamIDAddMoney}", steamIdAmount, 5f, false, false);
                    response = $"Gave {steamIdAmount} money to player '{addmoneytarget}'.";
                    return true;
            }
        }
    }
}
