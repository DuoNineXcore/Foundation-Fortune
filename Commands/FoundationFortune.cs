using CommandSystem;
using Exiled.Permissions.Extensions;
using System;
using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using MEC;
using FoundationFortune.API.Database;
using PlayerRoles;
using UnityEngine;
using RemoteAdmin;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using FoundationFortune.Events;

namespace FoundationFortune.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class FoundationFortuneCommand : ICommand
    {
        public string Command { get; } = "foundationfortune";
        public string[] Aliases { get; } = { "ff" };
        public string Description { get; } = "Manage FoundationFortune NPCs and Database.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("ff.manage"))
            {
                response = "You don't have permission to use this command.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Usage: foundationfortune <npc/database/hint> <subcommand>";
                return false;
            }

            string category = arguments.At(0).ToLower();

            switch (category)
            {
                case "npc":
                    return HandleNpcOperations(arguments, sender, out response);
                case "database":
                    return HandleDatabaseOperations(arguments, sender, out response);
                case "hint":
                    return HandleHintOperations(arguments, sender, out response);
                default:
                    response = "Invalid category. Use 'npc', 'database' or 'hint'.";
                    return false;
            }
        }

        #region Hint Section
        private bool HandleHintOperations(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: foundationfortune hint <minmode>";
                return false;
            }

            string subcommand = arguments.At(1).ToLower();

            switch (subcommand)
            {
                case "minmode":
                    return ToggleHintMinmode(arguments, sender, out response);
                case "align":
                    return SetHintAlignment(arguments, sender, out response);
                default:
                    response = "Invalid subcommand for 'hint'. Use 'minmode' or 'align'.";
                    return false;
            }
        }
        #endregion

        #region NPC Section
        private bool HandleNpcOperations(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: foundationfortune npc <add/remove/list/flush>";
                return false;
            }

            string subcommand = arguments.At(1).ToLower();

            switch (subcommand)
            {
                case "add":
                    return AddBuyingBot(arguments, sender, out response);
                case "remove":
                    return RemoveBuyingBot(arguments, sender, out response);
                case "list":
                    return ListBuyingBots(arguments, sender, out response);
                case "flush":
                    return FlushBuyingBots(arguments, sender, out response);
                default:
                    response = "Invalid subcommand for 'npc'. Use 'add' or 'remove'.";
                    return false;
            }
        }
        #endregion

        #region Database Section
        private bool HandleDatabaseOperations(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: foundationfortune database <flush/addmoney/removemoney>";
                return false;
            }

            string subcommand = arguments.At(1).ToLower();

            switch (subcommand)
            {
                case "flush":
                    return FlushDatabase(sender, out response);

                case "addmoney":
                    return AddMoneyToDatabase(arguments, sender, out response);

                case "removemoney":
                    return RemoveMoneyFromDatabase(arguments, sender, out response);

                default:
                    response = "Invalid subcommand for 'database'.";
                    return false;
            }
        }
        #endregion

        #region NPC Subsection
        private bool AddBuyingBot(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 9)
            {
                response = "Usage: foundationfortune npc add <Name> <Badge> <Color> <Role> <HeldItem> <ScaleX> <ScaleY> <ScaleZ>";
                return false;
            }

            string name = arguments.At(2);
            string badge = arguments.At(3);
            string color = arguments.At(4);
            string roleString = arguments.At(5);
            string heldItemString = arguments.At(6);

            if (!Enum.TryParse(roleString, out RoleTypeId role) || !Enum.TryParse(heldItemString, out ItemType heldItem))
            {
                response = "Invalid Role or HeldItem specified.";
                return false;
            }

            if (!float.TryParse(arguments.At(7), out float scaleX) || !float.TryParse(arguments.At(8), out float scaleY) || !float.TryParse(arguments.At(9), out float scaleZ))
            {
                response = "Invalid Scale specified. Please provide three float values for X, Y, and Z components.";
                return false;
            }

            Vector3 scale = new(scaleX, scaleY, scaleZ);

            Npc bot = BuyingBot.SpawnBuyingBot(name, badge, color, role, heldItem, scale);

            if (bot != null)
            {
                if (!(sender is PlayerCommandSender self))
                {
                    response = "Only players can use this command.";
                    return false;
                }

                Timing.CallDelayed(1f, delegate
                {
                    bot.Teleport(self.ReferenceHub.gameObject.transform.position);
                    FoundationFortune.Singleton.serverEvents.buyingBotPositions.Add(bot, bot.Position);
                });

                response = $"BuyingBot '{name}' added successfully and teleported to your position.";
                return true;
            }

            response = $"Failed to add the BuyingBot.";
            return false;
        }


        private bool RemoveBuyingBot(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count < 2)
            {
                response = "Usage: foundationfortune npc remove <IndexationNumber>";
                return false;
            }

            if (!int.TryParse(arguments.At(2), out int indexationNumber))
            {
                response = "Invalid IndexationNumber. Please provide a valid number.";
                return false;
            }

            if (BuyingBot.RemoveBuyingBot(indexationNumber))
            {
                BuyingBot.RemoveBuyingBot(indexationNumber);
                response = $"Removed BuyingBot with IndexationNumber {indexationNumber}.";
                return true;
            }
            else
            {
                response = $"No BuyingBot found with IndexationNumber {indexationNumber}.";
                return false;
            }
        }

        private bool ListBuyingBots(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (FoundationFortune.Singleton == null)
            {
                response = "FoundationFortune.Singleton is null.";
                return false;
            }

            if (FoundationFortune.Singleton.BuyingBotIndexation == null)
            {
                response = "FoundationFortune.Singleton.BuyingBotIndexation is null.";
                return false;
            }

            var buyingBots = FoundationFortune.Singleton.BuyingBotIndexation.Values.ToList();

            if (buyingBots.Any())
            {
                response = "List of BuyingBots in the server:\n";

                foreach (var botInfo in buyingBots)
                {
                    response += $"Indexation Number: {botInfo.indexation}, Name: {(botInfo.bot != null ? botInfo.bot.Nickname : "null")}\n";
                }
            }
            else
            {
                response = "No BuyingBots found in the server.";
            }

            return true;
        }

        private bool FlushBuyingBots(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var buyingBots = FoundationFortune.Singleton.BuyingBotIndexation.Values.ToList();

            foreach (var botInfo in buyingBots)
            {
                BuyingBot.RemoveBuyingBot(botInfo.indexation);
            }

            response = $"Flushed {buyingBots.Count} BuyingBots from the server.";
            return true;
        }

        #endregion

        #region Database Subsection

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

            string addmoneytarget = arguments.At(2).ToLower();

            switch (addmoneytarget)
            {
                case "self":
                    if (!(sender is PlayerCommandSender self))
                    {
                        response = "Command sender is not a player.";
                        return false;
                    }

                    var ply = Player.Get(self);

                    if (!int.TryParse(arguments.At(3), out int amount))
                    {
                        response = "Invalid amount. Please provide a valid number.";
                        return false;
                    }

                    FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"<color=green>${amount}.</color> Admin Command.", amount, 5f, false, false);
                    response = $"Gave {amount} money to player '{ply}'.";
                    return true;

                case "all":
                    if (!int.TryParse(arguments.At(3), out int allAmount))
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
                    if (!int.TryParse(arguments.At(3), out int steamIdAmount))
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

            string target = arguments.At(2).ToLower();
            bool subtractSaved = true;
            bool subtractOnHold = true;

            if (arguments.Count >= 4)
            {
                if (!int.TryParse(arguments.At(3), out int amount))
                {
                    response = "Invalid amount. Please provide a valid number.";
                    return false;
                }

                if (arguments.Count >= 5)
                {
                    bool.TryParse(arguments.At(4), out subtractSaved);
                }

                if (arguments.Count >= 6)
                {
                    bool.TryParse(arguments.At(5), out subtractOnHold);
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
                            if (!int.TryParse(arguments.At(3), out int allAmount))
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
                            if (!int.TryParse(arguments.At(3), out int allAmount))
                            {
                                response = "Invalid amount. Please provide a valid number.";
                                return false;
                            }

                            foreach (var player in Player.List)
                            {
                                PlayerDataRepository.SubtractMoneyOnHold(player.UserId, allAmount);
                            }
                        }

                        response = $"Removed {arguments.At(3)} money from all players (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;

                    default:
                        if (target != null)
                        {
                            if (!int.TryParse(arguments.At(3), out int steamIdAmount))
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

                        response = $"Removed {arguments.At(3)} money from player '{target}' (Saved: {subtractSaved}, On-Hold: {subtractOnHold}).";
                        return true;
                }
            }

            response = "Usage: foundationfortune database removemoney <self/steamid/all> [amount] [subtractSaved] [subtractOnHold]";
            return false;
        }

        #endregion

        #region Hint Subsection
        private bool ToggleHintMinmode(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerSender))
            {
                response = "Only players can use this command.";
                return false;
            }

            Player playersender = Player.Get(playerSender);
            var isMinmodeEnabled = PlayerDataRepository.GetHintMinmode(playersender.UserId);

            if (isMinmodeEnabled)
            {
                if (PlayerDataRepository.SetHintMinmode(playersender.UserId, false))
                {
                    response = "Hint Minmode is now disabled.";
                }
                else
                {
                    response = "Failed to disable Hint Minmode.";
                }
            }
            else
            {
                if (PlayerDataRepository.SetHintMinmode(playersender.UserId, true))
                {
                    response = "Hint Minmode is now enabled.";
                }
                else
                {
                    response = "Failed to enable Hint Minmode.";
                }
            }

            return true;
        }

        private bool SetHintAlignment(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!(sender is PlayerCommandSender playerSender))
            {
                response = "Only players can use this command.";
                return false;
            }

            Player player = Player.Get(playerSender);

            if (arguments.Count < 3)
            {
                response = $"Not enough arguments. [{arguments.Count}] Usage: align <center/left/right>";
                return false;
            }

            string alignmentStr = arguments.At(2).ToLower();

            if (Enum.TryParse(alignmentStr, true, out HintAlign alignment))
            {
                PlayerDataRepository.SetUserHintAlign(player.UserId, alignment);
                response = $"Hint alignment set to {alignment}.";
                return true;
            }
            else
            {
                response = "Invalid alignment. Use 'center', 'left', or 'right'.";
                return false;
            }
        }
        #endregion
    }
}
