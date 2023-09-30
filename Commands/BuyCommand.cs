using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Models;
using FoundationFortune.API.Database;
using FoundationFortune.API.Perks;
using InventorySystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.NonAllocLINQ;
using FoundationFortune.Configs;

namespace FoundationFortune.Commands.BuyCommand
{
	[CommandHandler(typeof(ClientCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public sealed class BuyCommand : ICommand
	{
		public static List<ObjectPurchases> PlayerLimits = new();

		public string Command { get; } = "buy";
		public string[] Aliases { get; } = new string[] { "b" };
		public string Description { get; } = "Buy items, wow.";
		private Perks perks = new();

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Player player = Player.Get(sender);

			if (!FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) && !FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
			{
				response = "You must be at a buying station to buy an item.";
				return false;
			}

			if (arguments.Count < 1)
			{
				response = GetList();
				return false;
			}

			Log.Debug($"Input argument: {arguments.At(0)}");


			if (Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType) && perkType == PerkType.Revival || FoundationFortune.Singleton.Config.PerkItems.Any(p => p.PerkType == PerkType.Revival && arguments.At(0).ToLower() == p.Alias.ToLower()))
			{
				if (arguments.Count < 2)
				{
					response = "You must specify a valid player to revive!";
					return false;
				}
				if (TryPurchaseRevivalPerk(player, string.Join(" ", arguments.Skip(0).Skip(1)), out response)) return true;
				else return false;
			}
			else if (TryPurchasePerk(player, arguments.At(0), out response) || TryPurchaseItem(player, arguments.At(0), out response))
			{
				Log.Debug("Successfully bought perk: " + arguments.At(0));
				return true;
			}

			response = GetList();
			return false;
		}

		private bool TryPurchaseRevivalPerk(Player player, string targetName, out string response)
		{
			int revivalPerkPrice = FoundationFortune.Singleton.Config.PerkItems
				.Where(perk => perk.PerkType == PerkType.Revival)
				.Select(perk => perk.Price)
				.FirstOrDefault();

			if (CanPurchase(player, revivalPerkPrice))
			{
				if (perks.GrantRevivalPerk(player, targetName))
				{
					response = $"You have successfully bought Revival Perk for ${revivalPerkPrice} to revive '{targetName}'.";
					return true;
				}
				else
				{
					response = "That player either doesn't exist or hasnt died yet!";
					return false;
				}
			}

			response = $"You don't have enough money to purchase the Revival Perk to revive '{targetName}'.";
			return false;
		}

		private bool TryPurchasePerk(Player player, string aliasOrEnum, out string response)
		{
			PerkItem perkItem = FoundationFortune.Singleton.Config.PerkItems.FirstOrDefault(p =>
				p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
				p.PerkType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

			if (perkItem != null && CanPurchase(player, perkItem.Price))
			{
				FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<color=red>-${perkItem.Price}</color> Bought {perkItem.Alias}", 0, 3, false, false);
				PlayerDataRepository.ModifyMoney(player.UserId, perkItem.Price, true, false, true);
				perks.GrantPerk(player, perkItem.PerkType);
				response = $"You have successfully bought {perkItem.DisplayName} for ${perkItem.Price}";
				return true;
			}

			response = "That is not a purchasable perk or you don't have enough money!";
			return false;
		}

		private bool TryPurchaseItem(Player player, string aliasOrEnum, out string response)
		{
			BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(p =>
				p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
				p.ItemType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

			if (buyItem != null && CanPurchase(player, buyItem.Price))
			{
				FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<color=red>-${buyItem.Price}</color> Bought {buyItem.Alias}", 0, 3, false, false);
				PlayerDataRepository.ModifyMoney(player.UserId, buyItem.Price, true, false, true);
				player.AddItem(buyItem.ItemType);
				response = $"You have successfully bought {buyItem.DisplayName} for ${buyItem.Price}";
				return true;
			}

			response = "That is not a purchaseable item or you don't have enough money!";
			return false;
		}

		private bool CanPurchase(Player player, int price)
		{
			int money = PlayerDataRepository.GetMoneySaved(player.UserId);
			if (money < price) return false;
			return true;
		}

		private string GetList()
		{
			string itemsToBuy = "\n<color=green>Available Items:</color>\n" +
							string.Join("\n", FoundationFortune.Singleton.Config.BuyableItems
								.Select(buyableItem => $"{buyableItem.ItemType} - {buyableItem.DisplayName} ({buyableItem.Alias}) - {buyableItem.Price}$")) +
							"\n";

			string perksToBuy = "<color=cyan>Available Perks:</color>\n" +
							string.Join("\n", FoundationFortune.Singleton.Config.PerkItems
								.Select(perkItem => $"{perkItem.DisplayName} ({perkItem.Alias}) - {perkItem.Price}$ - {perkItem.Description}")) +
							"\n";
			return itemsToBuy + perksToBuy;
		}
	}
}
