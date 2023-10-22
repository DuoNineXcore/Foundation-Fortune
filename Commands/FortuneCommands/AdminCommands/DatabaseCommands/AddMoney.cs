using CommandSystem;
using Exiled.Permissions.Extensions;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Database;

namespace FoundationFortune.Commands.FortuneCommands.DatabaseCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class AddMoney : ICommand
    {
        public string Command { get; } = "ff_addmoney";
        public string Description { get; } = "Add money to a player's account.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<self/steamid/all> <amount> <addsaved?> <addonhold?>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            var pluginTranslations = FoundationFortune.Singleton.Translation;
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.database.addmoney") || !PlayerDataRepository.GetPluginAdmin(p.UserId))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 2)
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

                    string SelfAddMoney = pluginTranslations.SelfAddMoney.Replace("%amount%", amount.ToString());
                    FoundationFortune.Singleton.ServerEvents.EnqueueHint(ply, $"{SelfAddMoney}", 5f);
                    PlayerDataRepository.ModifyMoney(ply.UserId, amount, false, true, false);
                    response = $"Gave {amount} money to player '{ply}'.";
                    return true;

                case "all":
                    if (!int.TryParse(args.At(1), out int allAmount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    string AllAddMoney = pluginTranslations.AllAddMoney.Replace("%amount%", allAmount.ToString());
                    foreach (var player in Player.List)
                    {
                        FoundationFortune.Singleton.ServerEvents.EnqueueHint(player, $"{AllAddMoney}", 5f);
                        PlayerDataRepository.ModifyMoney(player.UserId, allAmount, true, true, false);
                    }
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
                    if (targetPlayer != null) FoundationFortune.Singleton.ServerEvents.EnqueueHint(targetPlayer, $"{SteamIDAddMoney}", 5f);
                    PlayerDataRepository.ModifyMoney(SteamIDAddMoney, steamIdAmount, true, true, false);
                    response = $"Gave {steamIdAmount} money to player '{addmoneytarget}'.";
                    return true;
            }
        }
    }
}
