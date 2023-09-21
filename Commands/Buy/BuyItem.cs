using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using InventorySystem;
using System;
using System.Linq;

namespace FoundationFortune.Commands.Buy
{
	[CommandHandler(typeof(ClientCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BuyItemCommand : ICommand
    {
        public string Command { get; } = "Item";
        public string Description { get; } = "Buy an item!";
        public string[] Aliases { get; } = new string[] { "i" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (!FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) && !FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
            {
                response = "You must be at a buying station to buy an item!";
                return false;
            }

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

            BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(i => i.ItemType == itemType);

            if (buyItem == null)
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

            FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<size=27><color=red>-${buyItem.Price}</color> Bought {buyItem.DisplayName}</size>", 0, 5, false, false);
            response = $"You have successfully bought {buyItem.DisplayName} for ${buyItem.Price}";
            return true;
        }
    }
}
