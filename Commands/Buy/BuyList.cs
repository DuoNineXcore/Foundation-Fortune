using CommandSystem;
using System;
using System.Text;

namespace FoundationFortune.Commands.Buy
{
     internal class BuyListCommand : ICommand
     {
          public string Command { get; } = "list";
          public string Description { get; } = "List of things to buy!";
          public string[]? Aliases { get; } = null;

          public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
          {
               StringBuilder sb = new();
               sb.AppendLine("<color=green>Items to buy:</color>");
               foreach (var buyableItem in FoundationFortune.Singleton.Config.BuyableItems)
               {
                    sb.AppendLine($"{buyableItem.DisplayName} - {buyableItem.Price}$");
               }
               sb.AppendLine();
               sb.AppendLine("<color=green>Perks to buy:</color>");
               foreach (var perkItem in FoundationFortune.Singleton.Config.PerkItems)
               {
                    sb.AppendLine($"{perkItem.DisplayName} - {perkItem.Price}$ - {perkItem.Description}");
               }
               response = sb.ToString();
               return false;
          }
     }
}

/*
 * case "list1": //list1 my beloved
                                StringBuilder itemList = new();
                                foreach (var buyableItem in FoundationFortune.Singleton.Config.BuyableItems)
                                {
                                    itemList.AppendLine($"{buyableItem.DisplayName} - {buyableItem.Price}$");
                                }
                                response = $"\nAvailable items for purchase:\n{itemList}";
                                return true;

                            case "list2":
                                StringBuilder perkList = new();
                                foreach (var perkItem in FoundationFortune.Singleton.Config.PerkItems)
                                {
                                    perkList.AppendLine($"{perkItem.DisplayName} - {perkItem.Price}$ - {perkItem.Description}");
                                }
                                response = $"\nAvailable perks for purchase:\n{perkList}";
                                return true;
*/