﻿using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using System;
using System.Linq;
using Utils.NonAllocLINQ;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.HintSystem;
using FoundationFortune.API.Items.PerkItems;

namespace FoundationFortune.Commands.BuyCommand
{
	[CommandHandler(typeof(ClientCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public sealed class BuyCommand : ICommand
	{
		public string Command { get; } = "buy";
		public string[] Aliases { get; } = new string[] { "b" };
		public string Description { get; } = "Buy items, wow.";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Player player = Player.Get(sender);

			if (!FoundationFortune.Singleton.ServerEvents.IsPlayerOnSellingWorkstation(player) && !ServerEvents.IsPlayerNearBuyingBot(player))
			{
				response = "You must be at a Selling Workstation / Buying Bot to buy an item.";
				return false;
			}

			if (arguments.Count < 1)
			{
				response = GetList();
				return false;
			}

			Log.Debug($"Input argument: {arguments.At(0)}");

			if (Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType) && perkType == PerkType.ResurgenceBeacon || FoundationFortune.Singleton.Config.PerkItems.Any(p => p.PerkType == PerkType.ResurgenceBeacon && arguments.At(0).ToLower() == p.Alias.ToLower()))
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
				.Where(perk => perk.PerkType == PerkType.ResurgenceBeacon)
				.Select(perk => perk.Price)
				.FirstOrDefault();

			if (CanPurchase(player, revivalPerkPrice))
			{
				if (ResurgenceBeacon.SpawnResurgenceBeacon(player, targetName))
				{
					response = $"You have successfully bought a Resurgence Beacon for ${revivalPerkPrice} to revive '{targetName}'.";
					return true;
				}
				else
				{
					response = "That player either doesn't exist or hasnt died yet!";
					return false;
				}
			}
			response = $"You don't have enough money to purchase the Resurgence Beacon to revive '{targetName}'.";
			return false;
		}

		private static bool TryPurchasePerk(Player player, string aliasOrEnum, out string response)
		{
			PerkItem perkItem = FoundationFortune.Singleton.Config.PerkItems.FirstOrDefault(p =>
				p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
				p.PerkType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

			if (perkItem != null && CanPurchase(player, perkItem.Price) && !ExceedsPerkLimit(player, perkItem))
			{
				string boughtHint = FoundationFortune.Singleton.Translation.BuyItemSuccess
						    .Replace("%itemAlias%", perkItem.Alias)
						    .Replace("%itemPrice%", perkItem.Price.ToString());
				FoundationFortune.Singleton.ServerEvents.EnqueueHint(player, boughtHint, 3f);
				PlayerDataRepository.ModifyMoney(player.UserId, perkItem.Price, true);
				PerkBottle.GivePerkBottle(player, perkItem.PerkType);
				ServerEvents.AddToPlayerLimits(player, perkItem);

				response = $"You have successfully bought {perkItem.DisplayName} for ${perkItem.Price}";
				return true;
			}

			if (perkItem != null && ExceedsPerkLimit(player, perkItem))
			{
				response = $"You have exceeded the Perk Limit for the Perk '{perkItem.DisplayName}'";
				return true;
			}

			response = "That is not a purchasable perk or you do not have enough money";
			return false;
		}

		private static bool TryPurchaseItem(Player player, string aliasOrEnum, out string response)
		{
			BuyableItem buyItem = FoundationFortune.Singleton.Config.BuyableItems.FirstOrDefault(p =>
			    p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
			    p.ItemType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

			if (buyItem != null && CanPurchase(player, buyItem.Price) && !ExceedsItemLimit(player, buyItem))
			{
				string BoughtHint = FoundationFortune.Singleton.Translation.BuyItemSuccess
				    .Replace("%itemAlias%", buyItem.Alias)
				    .Replace("%itemPrice%", buyItem.Price.ToString());
				FoundationFortune.Singleton.ServerEvents.EnqueueHint(player, $"{BoughtHint}", 3f);
				PlayerDataRepository.ModifyMoney(player.UserId, buyItem.Price, true);
				player.AddItem(buyItem.ItemType);
				ServerEvents.AddToPlayerLimits(player, buyItem);
				response = $"You have successfully bought {buyItem.DisplayName} for ${buyItem.Price}";
				return true;
			}

			if (buyItem != null && ExceedsItemLimit(player, buyItem))
			{
				response = $"You have exceeded the Item Limit for the Item '{buyItem.DisplayName}'";
				return true;
			}

			response = "That is not a purchasable item or you don't have enough money!";
			return false;
		}

		private static bool CanPurchase(Player player, int price)
		{
			if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return true;
			int money = PlayerDataRepository.GetMoneySaved(player.UserId);
			return money >= price;
		}

		private static string GetList()
		{
			var translation = FoundationFortune.Singleton.Translation;
			var config = FoundationFortune.Singleton.Config;

			string itemsList = string.Join("\n", config.BuyableItems
			    .Select(buyableItem => translation.ItemsList
				   .Replace("%buyableItemType%", buyableItem.ItemType.ToString())
				   .Replace("%buyableItemDisplayName%", buyableItem.DisplayName)
				   .Replace("%buyableItemAlias%", buyableItem.Alias)
				   .Replace("%buyableItemPrice%", buyableItem.Price.ToString())));

			string perksList = string.Join("\n", config.PerkItems
			    .Select(perkItem => translation.PerksList
				   .Replace("%perkItemDisplayName%", perkItem.DisplayName)
				   .Replace("%perkItemAlias%", perkItem.Alias)
				   .Replace("%perkItemPrice%", perkItem.Price.ToString())
				   .Replace("%perkItemDescription%", perkItem.Description)));

			return $"{itemsList}\n{perksList}";
		}

		private static bool ExceedsPerkLimit(Player player, PerkItem perkItem)
		{
			if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null) return false;
			var perkCount = playerLimit.BoughtPerks.Count(pair => pair.Key.PerkType == perkItem.PerkType);
			return perkCount >= perkItem.Limit;
		}

		private static bool ExceedsItemLimit(Player player, BuyableItem buyItem)
		{
			if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null) return false;
			var itemCount = playerLimit.BoughtItems.Count(pair => pair.Key.ItemType == buyItem.ItemType);
			return itemCount >= buyItem.Limit;
		}
	}
}
