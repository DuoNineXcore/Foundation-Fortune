using System;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Features.Items.CustomItems;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using Utils.NonAllocLINQ;

namespace FoundationFortune.Commands.FortuneCommands
{
	[CommandHandler(typeof(ClientCommandHandler))]
	[CommandHandler(typeof(RemoteAdminCommandHandler))]
	public sealed class Buy : ICommand,IUsageProvider
	{
		public string Command { get; } = "buy";
		public string[] Aliases { get; } = { string.Empty };
		public string Description { get; } = "buying system lol";
		public string[] Usage { get; } = { "<ItemType/PerkType>" };

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Player player = Player.Get(sender);

			if (!SellingWorkstations.IsPlayerOnSellingWorkstation(player) && !NpcHelperMethods.IsPlayerNearBuyingBot(player))
			{
				response = "You must be at a Selling Workstation / Buying Bot to buy an item.";
				return false;
			}

			if (arguments.Count < 1)
			{
				response = GetList();
				return false;
			}
			
			if (Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType) && perkType == PerkType.ResurgenceBeacon || FoundationFortune.BuyableItemsList.PerkItems.Any(p => p.PerkType == PerkType.ResurgenceBeacon && arguments.At(0).ToLower() == p.Alias.ToLower()))
			{
				if (arguments.Count >= 2) return TryPurchaseRevivalPerk(player, string.Join(" ", arguments.Skip(0).Skip(1)), out response);
				response = "You must specify a valid player to revive!";
				return false;
			}

			if (TryPurchasePerk(player, arguments.At(0), out response) || TryPurchaseItem(player, arguments.At(0), out response)) return true;
			if(response == string.Empty) response = GetList();
			return false;
		}

		private static bool TryPurchaseRevivalPerk(Player player, string targetName, out string response)
		{
			int revivalPerkPrice = FoundationFortune.BuyableItemsList.PerkItems
				.Where(perk => perk.PerkType == PerkType.ResurgenceBeacon)
				.Select(perk => perk.Price)
				.FirstOrDefault();

			if (!CanPurchase(player, revivalPerkPrice))
			{
				response = $"You don't have enough money to purchase the Resurgence Beacon.";
				EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.NotEnoughMoney);
				return false;
			}

			if (!ResurgenceBeacon.GiveBeacon(player, targetName))
			{
				response = "That player either doesn't exist or hasnt died yet!";
				return false;
			}

			PlayerDataRepository.ModifyMoney(player.UserId, revivalPerkPrice, true, true, false);
			EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(player, NpcType.Buying, NpcUsageOutcome.BuySuccess);
			response = $"You have successfully bought a Resurgence Beacon for ${revivalPerkPrice} to revive '{targetName}'.";
			return true;
		}

		private static bool TryPurchasePerk(Player player, string aliasOrEnum, out string response)
		{
			BuyablePerk buyablePerk = FoundationFortune.BuyableItemsList.PerkItems.FirstOrDefault(p =>
				p.Alias.Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase) ||
				p.PerkType.ToString().Equals(aliasOrEnum, StringComparison.OrdinalIgnoreCase));

			if (buyablePerk == null || !CanPurchase(player, buyablePerk.Price) || IndexationMethods.ExceedsPerkLimit(player, buyablePerk))
			{
				if (buyablePerk == null) response = "That is not a valid perk to buy!";

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
				if (buyItem == null) response = "That is not a valid item to buy!";
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
			if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return true;
			int money = PlayerDataRepository.GetMoneyOnHold(player.UserId);
			return money >= price;
		}

		private static string GetList()
		{
			var translation = FoundationFortune.Instance.Translation;

			string itemsList = string.Join("\n", FoundationFortune.BuyableItemsList.BuyableItems
			    .Select(buyableItem => translation.ItemsList
				   .Replace("%buyableItemType%", buyableItem.ItemType.ToString())
				   .Replace("%buyableItemDisplayName%", buyableItem.DisplayName)
				   .Replace("%buyableItemAlias%", buyableItem.Alias)
				   .Replace("%buyableItemPrice%", buyableItem.Price.ToString())));

			string perksList = string.Join("\n", FoundationFortune.BuyableItemsList.PerkItems
			    .Select(perkItem => translation.PerksList
				   .Replace("%perkItemDisplayName%", perkItem.DisplayName)
				   .Replace("%perkItemAlias%", perkItem.Alias)
				   .Replace("%perkItemPrice%", perkItem.Price.ToString())
				   .Replace("%perkItemDescription%", perkItem.Description)));

			return $"That is not a valid item to purchase!\nItems available for purchase:\n {itemsList} \n\nPerks available for purchase:\n{perksList}";
		}
	}
}
