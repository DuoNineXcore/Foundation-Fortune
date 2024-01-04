using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Common.Enums;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Core.Common.Models.Items;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events;
using FoundationFortune.API.Features.Items.CustomItems;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;

namespace FoundationFortune.Commands.NonAdminCommands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public sealed class Buy : ICommand,IUsageProvider
{
	public string Command { get; } = "buy";
	public string[] Aliases { get; } = { string.Empty };
	public string Description { get; } = "buying system lol";
	public string[] Usage { get; } = { "<Alias/ItemType/PerkType>" };

	public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
	{
		Player player = Player.Get(sender);

		if (!SellingWorkstations.IsPlayerOnSellingWorkstation(player) && !NPCHelperMethods.IsPlayerNearBuyingBot(player))
		{
			response = "You must be at a Selling Workstation / Buying Bot to buy an item.";
			return false;
		}

		if (arguments.Count < 1)
		{
			response = "Type .buy list <1-3> to see the categories.";
			return false;
		}

		if (arguments.At(0).Equals("list", StringComparison.OrdinalIgnoreCase))
		{
			if (arguments.Count >= 2 && int.TryParse(arguments.At(1), out int category) && category is >= 1 and <= 3) response = GetList(category);
			else response = "Invalid category. Please specify a category number between 1 and 3.";
			return false;
		}

		string itemAliasOrEnum = arguments.At(0);
		string targetUsername = arguments.Count >= 2 ? string.Join(" ", arguments.Skip(1)) : null;

		if (TryPurchaseCustomItem(player, itemAliasOrEnum, out response, targetUsername)) return true;
		return TryPurchasePerk(player, itemAliasOrEnum, out response) || TryPurchaseItem(player, itemAliasOrEnum, out response);
	}

	private static bool TryPurchaseCustomItem(Player player, string aliasOrEnum, out string response, string username = null)
	{
		BuyableCustomItem customItem = FoundationFortune.BuyableItemsList.BuyableCustomItems.FirstOrDefault(c =>
			c.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

		if (customItem == null || !CanPurchase(player, customItem.Price))
		{
			response = customItem == null ? "That is not a valid custom category item to buy." : "You do not have enough money to buy that custom category item.";
			return false;
		}

		switch (customItem.CustomItemType)
		{
			case CustomItemType.ResurgenceBeacon:
				if (!ResurgenceBeacon.GiveBeacon(player, username))
				{
					response = "That player either doesn't exist or hasn't died yet.";
					return false;
				}
				PlayerStatsRepository.ModifyMoney(player.UserId, customItem.Price, true, true, false);
				EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.BuySuccess);
				response = $"You have successfully bought and anchored a Resurgence Beacon to {username}.";
				return true;
		}

		EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.BuySuccess);
		response = $"You have successfully bought {customItem.DisplayName} for ${customItem.Price}";
		return true;
	}

	private static bool TryPurchasePerk(Player player, string aliasOrEnum, out string response)
	{
		BuyablePerk buyablePerk = FoundationFortune.BuyableItemsList.PerkItems.FirstOrDefault(p =>
			p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
			p.PerkType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

		if (buyablePerk == null || !CanPurchase(player, buyablePerk.Price) || IndexationMethods.ExceedsPerkLimit(player, buyablePerk))
		{
			if (buyablePerk == null) response = "That is not a valid perk to buy.";

			else if (IndexationMethods.ExceedsPerkLimit(player, buyablePerk)) response = $"You have exceeded the Perk Limit for the Perk '{buyablePerk.DisplayName}'";
			else
			{
				EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.NotEnoughMoney);
				response = "You do not have enough money to buy that perk.";
			}

			response = "That is not a purchasable perk or you do not have enough money";
			return false;
		}

		EventHelperMethods.RegisterOnBoughtPerk(player, buyablePerk);
		EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.BuySuccess);
		response = $"You have successfully bought {buyablePerk.DisplayName} for ${buyablePerk.Price}";
		return true;
	}

	private static bool TryPurchaseItem(Player player, string aliasOrEnum, out string response)
	{
		BuyableItem buyItem = FoundationFortune.BuyableItemsList.BuyableItems.FirstOrDefault(p =>
			p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
			p.ItemType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

		if (buyItem == null || !CanPurchase(player, buyItem.Price) || IndexationMethods.ExceedsItemLimit(player, buyItem))
		{
			if (buyItem == null) response = "That is not a valid item to buy.";
			else if (IndexationMethods.ExceedsItemLimit(player, buyItem)) response = $"You have exceeded the Item Limit for the Item '{buyItem.DisplayName}'";
			else
			{
				EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.NotEnoughMoney);
				response = "You do not have enough money to buy that item.";
			}

			return false;
		}

		EventHelperMethods.RegisterOnBoughtItem(player, buyItem);
		EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.BuySuccess);
			
		response = $"You have successfully bought {buyItem.DisplayName} for ${buyItem.Price}";
		return true;
	}

	private static bool CanPurchase(Player player, int price)
	{
		if (PlayerSettingsRepository.GetPluginAdmin(player.UserId)) return true;
		int money = PlayerStatsRepository.GetMoneyOnHold(player.UserId);
		return money >= price;
	}

	private static string GetList(int cat)
	{
		return cat switch
		{
			1 => string.Join("\n",
				FoundationFortune.BuyableItemsList.BuyableItems.Select(buyableItem =>
					FoundationFortune.Instance.Translation.ItemsList
						.Replace("%buyableItemType%", buyableItem.ItemType.ToString())
						.Replace("%buyableItemDisplayName%", buyableItem.DisplayName)
						.Replace("%buyableItemAlias%", buyableItem.Alias)
						.Replace("%buyableItemPrice%", buyableItem.Price.ToString()))),
			2 => string.Join("\n",
				FoundationFortune.BuyableItemsList.PerkItems.Select(perkItem =>
					FoundationFortune.Instance.Translation.PerksList
						.Replace("%perkItemDisplayName%", perkItem.DisplayName)
						.Replace("%perkItemAlias%", perkItem.Alias)
						.Replace("%perkItemPrice%", perkItem.Price.ToString())
						.Replace("%perkItemDescription%", perkItem.Description))),
			3 => string.Join("\n",
				FoundationFortune.BuyableItemsList.BuyableCustomItems.Select(buyableCustomItem =>
					FoundationFortune.Instance.Translation.CustomItemsList
						.Replace("%customItemDisplayName%", buyableCustomItem.DisplayName)
						.Replace("%customItemAlias%", buyableCustomItem.Alias)
						.Replace("%customItemPrice%", buyableCustomItem.Price.ToString())
						.Replace("%customItemDescription%", buyableCustomItem.Description)
						.Replace("%customItemType%", buyableCustomItem.CustomItemType.ToString()))),
			_ => "Invalid category number. Please specify a number between 1 and 3, idiot."
		};
	}
}