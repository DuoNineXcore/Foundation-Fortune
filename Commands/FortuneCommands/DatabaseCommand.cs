using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Linq;

namespace FoundationFortune.Commands.FortuneCommands
{
    internal class DatabaseCommand : ICommand
    {
        public string Command { get; } = "database";
        public string Description { get; } = "Database stuff";
        public string[] Aliases { get; } = new string[] { "d" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (args.Count < 1)
            {
                response = "Usage: foundationfortune database <flush/addmoney/removemoney>";
                return false;
            }

            string subcommand = args.At(0).ToLower();

            switch (subcommand)
            {
                case "flush":
                    return FlushDatabase(sender, out response);
                case "addmoney":
                    return AddMoneyToDatabase(args, sender, out response);
                case "removemoney":
                    return RemoveMoneyFromDatabase(args, sender, out response);
                default:
                    response = "Invalid subcommand for 'database'.";
                    return false;
            }
        }

        private bool FlushDatabase(ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.database.flush"))
            {
                response = "You don't have permission to use this command!";
                return false;
            }

            foreach (var player in Player.List.Where(player => !player.IsNPC))
            {
                int? moneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);
                int? moneySaved = PlayerDataRepository.GetMoneySaved(player.UserId);

                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<size=24><color=red>-${moneyOnHold} (On Hold) -${moneySaved} (Saved)</color> Database Flushed.</size>", 0, 5f, false, false);

                PlayerDataRepository.EmptyMoneyOnHold(player.UserId);
                PlayerDataRepository.EmptyMoneySaved(player.UserId);
            }

            response = "Flushed Database.";
            return true;
        }

        private bool AddMoneyToDatabase(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
            {
                response = "Usage: foundationfortune database addmoney <self/steamid/all> [amount]";
                return false;
            }

            string addmoneytarget = arguments.At(1).ToLower();

            switch (addmoneytarget)
            {
                case "self":
                    if (!(sender is PlayerCommandSender self))
                    {
                        response = "Command sender is not a player.";
                        return false;
                    }

                    var ply = Player.Get(self);

                    if (!int.TryParse(arguments.At(2), out int amount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"<color=green>${amount}.</color> Admin Command.", amount, 5f, false, false);
                    response = $"Gave {amount} money to player '{ply}'.";
                    return true;

                case "all":
                    if (!int.TryParse(arguments.At(2), out int allAmount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    foreach (var player in Player.List)
                    {
                        FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<color=green>${allAmount}.</color> Admin Command.", allAmount, 5f, false, false);
                    }

                    response = $"Gave {allAmount} money to all players.";
                    return true;

                default:
                    if (!int.TryParse(arguments.At(2), out int steamIdAmount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    var targetPlayer = Player.Get(addmoneytarget);

                    if (targetPlayer != null)
                    {
                        FoundationFortune.Singleton.serverEvents.EnqueueHint(targetPlayer, $"<color=green>${steamIdAmount}.</color> Admin Command.", steamIdAmount, 5f, false, false);
                    }

                    response = $"Gave {steamIdAmount} money to player '{addmoneytarget}'.";
                    return true;
            }
        }

        private bool RemoveMoneyFromDatabase(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 3)
            {
                response = "Usage: foundationfortune database removemoney <self/steamid/all> [amount] [subtractSaved] [subtractOnHold]";
                return false;
            }

            string target = arguments.At(1).ToLower();
            bool subtractSaved = true;
            bool subtractOnHold = true;

            if (arguments.Count >= 3)
            {
                if (!int.TryParse(arguments.At(2), out int amount))
                {
                    response = "Invalid amount. Please provide a valid number.";
                    return false;
                }

                if (arguments.Count >= 4)
                {
                    bool.TryParse(arguments.At(3), out subtractSaved);
                }

                if (arguments.Count >= 5)
                {
                    bool.TryParse(arguments.At(4), out subtractOnHold);
                }

                switch (target)
                {
                    case "self":
                        if (!(sender is PlayerCommandSender self))
                        {
                            response = "Command sender is not a player.";
                            return false;
                        }

                        var ply = Player.Get(self);

                        if (subtractSaved)
                        {
                            PlayerDataRepository.SubtractMoneySaved(ply.UserId, amount);
                        }

                        if (subtractOnHold)
                        {
                            PlayerDataRepository.SubtractMoneyOnHold(ply.UserId, amount);
                        }

                        FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"<color=red>-${amount}.</color> Admin Command.", 0, 5f, false, false);
                        response = $"Removed {amount} money from player '{ply}' (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;

                    case "all":
                        if (subtractSaved)
                        {
                            if (!int.TryParse(arguments.At(2), out int allAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            foreach (var player in Player.List)
                            {
                                PlayerDataRepository.SubtractMoneySaved(player.UserId, allAmount);
                            }
                        }

                        if (subtractOnHold)
                        {
                            if (!int.TryParse(arguments.At(2), out int allAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            foreach (var player in Player.List)
                            {
                                PlayerDataRepository.SubtractMoneyOnHold(player.UserId, allAmount);
                            }
                        }

                        response = $"Removed {arguments.At(2)} money from all players (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;

                    default:
                        if (target != null)
                        {
                            if (!int.TryParse(arguments.At(2), out int steamIdAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            var targetPlayer = Player.Get(target);
                            if (targetPlayer != null)
                            {
                                if (subtractSaved)
                                {
                                    PlayerDataRepository.SubtractMoneySaved(targetPlayer.UserId, steamIdAmount);
                                }

                                if (subtractOnHold)
                                {
                                    PlayerDataRepository.SubtractMoneyOnHold(targetPlayer.UserId, steamIdAmount);
                                }

                                FoundationFortune.Singleton.serverEvents.EnqueueHint(targetPlayer, $"<color=red>-${steamIdAmount}.</color> Admin Command.", 0, 5f, false, false);
                            }
                        }

                        response = $"Removed {arguments.At(2)} money from player '{target}' (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;
                }
            }

            response = "Usage: foundationfortune database removemoney <self/steamid/all> [amount] [subtractSaved] [subtractOnHold]";
            return false;
        }
    }
}
