//using CommandSystem;
//using System;
//using System.Text;
//using FoundationFortune.Events;
//using PluginAPI.Core;
//using RemoteAdmin;

//namespace FoundationFortune.Commands
//{
//    [CommandHandler(typeof(ClientCommandHandler))]
//    [CommandHandler(typeof(RemoteAdminCommandHandler))]
//    public class BuyCommand : ICommand
//    {
//        public string Command { get; } = "Buy";
//        public string[] Aliases { get; } = new string[] { "B" };
//        public string Description { get; } = "Buy stuff, what else.";

//        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
//        {
//            if (sender is PlayerCommandSender playerSender)
//            {
//                Player player = Player.Get(playerSender);
//                if (FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) || FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
//                {
//                    if (arguments.Count > 0)
//                    {
//                        string subCommand = arguments.Array[arguments.Offset].ToLower();
//                        switch (subCommand)
//                        {
//                            case "list1": //list1 my beloved
//                                StringBuilder itemList = new();
//                                foreach (var buyableItem in FoundationFortune.Singleton.Config.BuyableItems)
//                                {
//                                    itemList.AppendLine($"{buyableItem.DisplayName} - {buyableItem.Price}$");
//                                }
//                                response = $"\nAvailable items for purchase:\n{itemList}";
//                                return true;

//                            case "list2":
//                                StringBuilder perkList = new();
//                                foreach (var perkItem in FoundationFortune.Singleton.Config.PerkItems)
//                                {
//                                    perkList.AppendLine($"{perkItem.DisplayName} - {perkItem.Price}$ - {perkItem.Description}");
//                                }
//                                response = $"\nAvailable perks for purchase:\n{perkList}";
//                                return true;

//                            case "buyitem":
//                                // Logic for purchasing items
//                                // Check item availability, deduct money, give item, etc.
//                                // Update response accordingly
//                                response = "Item purchase logic goes here.";
//                                return true;

//                            case "buyperk":
//                                // Logic for purchasing perks
//                                // Check perk availability, deduct money, apply perk effect, etc.
//                                // Update response accordingly
//                                response = "Perk purchase logic goes here.";
//                                return true;

//                            default:
//                                response = ".buy <list1/list2/buyitem/buyperk>";
//                                return false;
//                        }
//                    }
//                    else
//                    {
//                        response = ".buy <list1/list2>";
//                        return false;
//                    }
//                }
//                else
//                {
//                    response = "You are not near a Selling workstation or a BuyingBot.";
//                    return false;
//                }
//            }
//            response = "how did you even get here";
//            return false;
//        }
//    }
//}
