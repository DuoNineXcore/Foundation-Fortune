using CommandSystem;
using System;
using System.Linq;
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
               string itemsToBuy = "\n<color=green>Items to buy:</color>\n" +
                                   string.Join("\n", FoundationFortune.Singleton.Config.BuyableItems
                                       .Select(buyableItem => $"{buyableItem.ItemType} - {buyableItem.DisplayName} - {buyableItem.Price}$")) +
                                   "\n";

               string perksToBuy = "<color=green>Perks to buy:</color>\n" +
                                   string.Join("\n", FoundationFortune.Singleton.Config.PerkItems
                                       .Select(perkItem => $"{perkItem.DisplayName} - {perkItem.Price}$ - {perkItem.Description}")) +
                                   "\n";

               response = itemsToBuy + perksToBuy;
               return false;
          }
     }
}