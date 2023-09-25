using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API;
using FoundationFortune.API.Database;
using FoundationFortune.API.Perks;
using InventorySystem;
using InventorySystem.Items.Firearms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.NonAllocLINQ;

namespace FoundationFortune.Commands.Buy
{
	[CommandHandler(typeof(ClientCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public sealed class BuyCommand : ICommand
	{
		private List<PurchasesObject> PlayerLimits = new();
		private Perks perks = new();

		public string Command { get; } = "buy";
		public string[] Aliases { get; } = new string[] { "b" };
		public string Description { get; } = "You buy stuff.. What else did you expect?";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Player player = Player.Get(sender);

			if (!FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) && !FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
			{
				response = "You must be at a buying station to buy an item!";
				return false;
			}

			if (arguments.Count != 1)
			{
				response = GetList();
				return false;
			}

			PurchasesObject purchases = PlayerLimits.FirstOrDefault(o => o.Player == player);

			PerkItem perkItem = FoundationFortune.Singleton.Config.PerkItems.FirstOrDefault(p => p.Alias == arguments.At(0));
			BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(p => p.Alias == arguments.At(0));

			if (Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType) || perkItem != null)
			{
				PerkItem perk = perkItem == null ? FoundationFortune.Singleton.Config.PerkItems.FirstOrDefault(p => p.PerkType == perkType) : perkItem;

				if (perk == null)
				{
					response = "That is not a purchasable perk!";
					return false;
				}

				if (purchases != null && purchases.BoughtPerks.ContainsKey(perk) && purchases.BoughtPerks[perk] >= perk.Limit)
				{
					response = $"You have purchased {perk.DisplayName} too many times.";
					return false;
				}

				if (perkType == PerkType.Revival)
				{
					string targetName = string.Join(" ", arguments.Skip(1));

					if (perks.GrantRevivalPerk(player, targetName))
					{
						response = $"You have successfully revived '{targetName}' with the {perk.DisplayName} perk.";
					}
					else
					{
						response = $"Failed to revive the player named '{targetName}' with the {perk.DisplayName} perk.";
						return false;
					}
				}
				else
				{
					int money = PlayerDataRepository.GetMoneySaved(player.UserId);

					if (money < perk.Price)
					{
						response = $"You are missing ${perk.Price - money}!";
						return false;
					}

					PlayerDataRepository.SubtractMoneySaved(player.UserId, perk.Price);
					perks.GrantPerk(player, perkType);

					if (purchases.BoughtPerks.ContainsKey(perk))
					{
						purchases.BoughtPerks[perk]++;
					}
					else
					{
						purchases.BoughtPerks.Add(perk, 1);
					}
				}

				response = $"You have successfully bought {perk.DisplayName} for ${perk.Price}";
				return true;
			}
			else if (Enum.TryParse(arguments.At(0), ignoreCase: true, out ItemType itemType) || buyItem != null)
			{
				//BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(i => i.ItemType == itemType);
				BuyableItem item = buyItem == null ? FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(p => p.ItemType == itemType) : buyItem;

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

				if (purchases != null && purchases.BoughtItems.ContainsKey(buyItem) && purchases.BoughtItems[buyItem] >= buyItem.Limit)
				{
					response = $"You have purchased {buyItem.DisplayName} too many times.";
					return false;
				}

				if (purchases.BoughtItems.ContainsKey(buyItem))
				{
					purchases.BoughtItems[buyItem]++;
				}
				else
				{
					purchases = new(player);
					purchases.BoughtItems.Add(buyItem, 1);
					PlayerLimits.Add(purchases);
				}

				PlayerDataRepository.SubtractMoneySaved(player.UserId, buyItem.Price);
				player.Inventory.ServerAddItem(buyItem.ItemType);

				FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<size=27><color=red>-${buyItem.Price}</color> Bought {buyItem.DisplayName}</size>", 0, 5, false, false);
				response = $"You have successfully bought {buyItem.DisplayName} for ${buyItem.Price}";
				return true;
			}

			response = GetList();
			return false;
		}

		private string GetList()
		{
			string itemsToBuy = "\n<color=green>Items to buy:</color>\n" +
							string.Join("\n", FoundationFortune.Singleton.Config.BuyableItems
							    .Select(buyableItem => $"{buyableItem.ItemType} - {buyableItem.DisplayName} - {buyableItem.Price}$")) +
							"\n";

			string perksToBuy = "<color=green>Perks to buy:</color>\n" +
							string.Join("\n", FoundationFortune.Singleton.Config.PerkItems
							    .Select(perkItem => $"{perkItem.DisplayName} - {perkItem.Price}$ - {perkItem.Description}")) +
							"\n";
			return itemsToBuy + perksToBuy;
		}
	}
}
