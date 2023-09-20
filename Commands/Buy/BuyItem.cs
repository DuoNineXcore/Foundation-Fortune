using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using InventorySystem;
using System;
using System.Linq;

namespace FoundationFortune.Commands.Buy
{
     internal class BuyItemCommand : ICommand
     {
          public string Command { get; } = "Item";
          public string Description { get; } = "Buy an item!";
          public string[] Aliases { get; } = new string[] { "i" };

          public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
          {
               Player player = Player.Get(sender);

               if (FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) || FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
               {
                    if (arguments.Count < 1)
                    {
                         response = "You must specify an item to buy!";
                         return false;
                    }
                    if (!Enum.TryParse(arguments.At(0), ignoreCase: true, out ItemType itemType))
                    {
                         response = "You must specify a valid item!";
                         return false;
                    }
                    BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.Where(i => i.ItemType == itemType).FirstOrDefault();
                    if(buyItem == null)
                    {
                         response = "That is not a purchaseable item!";
                         return false;
                    }
                    int money = PlayerDataRepository.GetMoneySaved(player.UserId);
                    if (money < buyItem.Price)
                    {
                         response = $"You are missing ${buyItem.Price - money}!";
                         return false;
                    }
                    PlayerDataRepository.SubtractMoneySaved(player.UserId, buyItem.Price);
                    player.Inventory.ServerAddItem(buyItem.ItemType);

                    response = $"You have successfully bought a {buyItem.DisplayName} for ${buyItem.Price}";
                    return true;
               }
               else
               {
                    response = "You must be at a buying station to buy an item!";
                    return false;
               }
          }
     }
}
