using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using FoundationFortune.API.Perks;
using System;
using System.Collections.Generic;
using System.Linq;
using Utils.NonAllocLINQ;
using FoundationFortune.API;

namespace FoundationFortune.Commands.Buy
{
	internal class BuyPerk : ICommand
	{
		public string Command { get; } = "Perk";
		public string Description { get; } = "Buy a perk!";
		public string[] Aliases { get; } = new string[] { "p" };
		private Perks perks = new();

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
			Player player = Player.Get(sender);

			if (FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) || FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
			{
				if (arguments.Count < 1)
				{
					response = "You must specify a perk to buy!";
					return false;
				}
				if (!Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType))
				{
					response = "You must specify a valid perk!";
					return false;
				}
				PerkItem perk = FoundationFortune.Singleton.Config.PerkItems.FirstOrDefault(p => p.PerkType == perkType);

				if (perk == null)
				{
					response = "That is not a purchasable perk!";
					return false;
				}

				PurchasesObject purchases = ParentBuyCommand.PlayerLimits.FirstOrDefault(o => o.Player == player) ?? new(player);

				if (purchases != null && purchases.BoughtPerks.ContainsKey(perk) && purchases.BoughtPerks[perk] >= perk.Limit)
				{
					response = $"You have purchased {perk.DisplayName} too many times.";
					return false;
				}

				if (purchases.BoughtPerks.ContainsKey(perk))
				{
					purchases.BoughtPerks[perk]++;
				}
				else
				{
					purchases.BoughtPerks.Add(perk, 1);
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
					AddPurchasedPerk(player.UserId, perkType);
				}

				response = $"You have successfully bought {perk.DisplayName} for ${perk.Price}";
				return true;
			}
			else
			{
				response = "You must be at a buying station to buy a perk!";
				return false;
			}
		}

		private bool HasPurchasedPerk(string userId, PerkType perkType)
		{
			return FoundationFortune.Singleton.purchasedPerks.TryGetValue(userId, out var purchasedPerksList) && purchasedPerksList.Contains(perkType);
		}

		private void AddPurchasedPerk(string userId, PerkType perkType)
		{
			if (FoundationFortune.Singleton.purchasedPerks.TryGetValue(userId, out var purchasedPerksList))
			{
				purchasedPerksList.Add(perkType);
			}
			else
			{
				FoundationFortune.Singleton.purchasedPerks[userId] = new List<PerkType> { perkType };
			}
		}
	}
}
