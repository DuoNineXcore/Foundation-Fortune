﻿using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;
using RemoteAdmin;

namespace FoundationFortune.Commands.AdminCommands.DatabaseManagement;

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
        var pluginTranslations = FoundationFortune.Instance.Translation;
        Player p = Player.Get(sender);

        if (!sender.CheckPermission("ff.database.addmoney") && !PlayerSettingsRepository.GetPluginAdmin(p.UserId))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (args.Count < 4 || !bool.TryParse(args.At(2), out bool addToHold) || !bool.TryParse(args.At(3), out bool addToSaved))
        {
            response = "Usage: foundationfortune database addmoney <self/steamid/all> [amount] [addToHold] [addToSaved]";
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

                string selfAddMoney = pluginTranslations.SelfAddMoney.Replace("%amount%", amount.ToString());
                FoundationFortune.Instance.HintSystem.BroadcastHint(ply, $"{selfAddMoney}");
                PlayerStatsRepository.ModifyMoney(ply.UserId, amount, false, addToHold, addToSaved);
                response = $"Gave {amount} money to player '{ply}'.";
                return true;

            case "all":
                if (!int.TryParse(args.At(1), out int allAmount))
                {
                    response = "Invalid amount. Please provide a valid number.";
                    return false;
                }

                string allAddMoney = pluginTranslations.AllAddMoney.Replace("%amount%", allAmount.ToString());
                foreach (var player in Player.List)
                {
                    FoundationFortune.Instance.HintSystem.BroadcastHint(player, $"{allAddMoney}");
                    PlayerStatsRepository.ModifyMoney(player.UserId, allAmount, true, addToHold, addToSaved);
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
                string steamIDAddMoney = pluginTranslations.SteamIDAddMoney.Replace("%amount%", steamIdAmount.ToString());
                if (targetPlayer != null) FoundationFortune.Instance.HintSystem.BroadcastHint(targetPlayer, $"{steamIDAddMoney}");
                PlayerStatsRepository.ModifyMoney(steamIDAddMoney, steamIdAmount, true, true, false);
                response = $"Gave {steamIdAmount} money to player '{addmoneytarget}'.";
                return true;
        }
    }
}