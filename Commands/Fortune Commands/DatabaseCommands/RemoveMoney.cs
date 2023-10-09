using CommandSystem;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;

namespace FoundationFortune.Commands.FortuneCommands.DatabaseCommands
{
    internal class RemoveMoney : ICommand
    {
        public string Command { get; } = "removemoney";
        public string Description { get; } = "Remove money from a player's account.";
        public string[] Aliases { get; } = new string[] { "deductmoney" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            var pluginTranslations = FoundationFortune.Singleton.Translation;
            if (!sender.CheckPermission("ff.database.removemoney"))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            if (args.Count < 3)
            {
                response = "Usage: foundationfortune database removemoney <self/steamid/all> [amount] [subtractSaved] [subtractOnHold]";
                return false;
            }

            string target = args.At(1).ToLower();
            bool subtractSaved = true;
            bool subtractOnHold = true;

            if (args.Count >= 3)
            {
                if (!int.TryParse(args.At(2), out int amount))
                {
                    response = "Invalid amount. Please provide a valid number.";
                    return false;
                }

                if (args.Count >= 4) bool.TryParse(args.At(3), out subtractSaved);
                if (args.Count >= 5) bool.TryParse(args.At(4), out subtractOnHold);

                switch (target)
                {
                    case "self":
                        if (sender is not PlayerCommandSender self)
                        {
                            response = "Command sender is not a player.";
                            return false;
                        }

                        var ply = Player.Get(self);

                        if (subtractSaved) PlayerDataRepository.ModifyMoney(ply.UserId, amount, true, false, true);
                        if (subtractOnHold) PlayerDataRepository.ModifyMoney(ply.UserId, amount, true, true, false);

                        string SelfRemoveMoney = pluginTranslations.SelfRemoveMoney.Replace("%amount%", amount.ToString());

                        FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"{SelfRemoveMoney}", 0, 5f, false, false);
                        response = $"Removed {amount} money from player '{ply}' (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;

                    case "all":
                        if (subtractSaved)
                        {
                            if (!int.TryParse(args.At(2), out int allAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            foreach (var player in Player.List)
                            {
                                string AllRemoveMoney = pluginTranslations.AllRemoveMoney.Replace("%amount%", allAmount.ToString());
                                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"{AllRemoveMoney}", 0, 5f, false, false);
                                PlayerDataRepository.ModifyMoney(player.UserId, allAmount, true, false, true);
                            }
                        }

                        if (subtractOnHold)
                        {
                            if (!int.TryParse(args.At(2), out int allAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            foreach (var player in Player.List)
                            {
                                string AllRemoveMoney = pluginTranslations.AllRemoveMoney.Replace("%amount%", allAmount.ToString());
                                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"{AllRemoveMoney}", 0, 5f, false, false);
                                PlayerDataRepository.ModifyMoney(player.UserId, allAmount, true, true, false);
                            }
                        }

                        response = $"Removed {args.At(2)} money from all players (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;

                    default:
                        if (target != null)
                        {
                            if (!int.TryParse(args.At(2), out int steamIdAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            var targetPlayer = Player.Get(target);
                            if (targetPlayer != null)
                            {
                                if (subtractSaved) PlayerDataRepository.ModifyMoney(targetPlayer.UserId, steamIdAmount, true, false, false);
                                if (subtractOnHold) PlayerDataRepository.ModifyMoney(targetPlayer.UserId, steamIdAmount, true, false, true);

                                string SteamIdRemoveMoney = pluginTranslations.AllRemoveMoney.Replace("%amount%", steamIdAmount.ToString());
                                FoundationFortune.Singleton.serverEvents.EnqueueHint(targetPlayer, $"{SteamIdRemoveMoney}", 0, 5f, false, false);
                            }
                        }

                        response = $"Removed {args.At(2)} money from player '{target}' (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;
                }
            }

            response = "Usage: foundationfortune database removemoney <self/steamid/all> [amount] [subtractSaved] [subtractOnHold]";
            return false;
        }
    }
}
