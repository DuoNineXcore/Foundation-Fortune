using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using FoundationFortune.API.Features.Perks.Active;
using FoundationFortune.API.Features.Perks.Passive;
using FoundationFortune.API.Features.Systems;

namespace FoundationFortune.API.Core
{
    public static class IndexationMethods
	{
		private static readonly List<ObjectInteractions> PlayerPurchaseLimits = new();

		public static void ClearIndexations()
        {
            foreach (var botData in NPCInitialization.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in NPCInitialization.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in NPCHelperMethods.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            EthericVitality.EthericVitalityPlayers.Clear();
            HyperactiveBehavior.HyperactiveBehaviorPlayers.Clear();
            EtherealIntervention.EtherealInterventionPlayers.Clear();
            ViolentImpulses.ViolentImpulsesPlayers.Clear();
            PerkSystem.ConsumedPerks.Clear();
            PlayerPurchaseLimits.Clear();
	        NPCInitialization.SellingBotPositions.Clear();
	        NPCInitialization.BuyingBotPositions.Clear();
	        SellingWorkstations.WorkstationPositions.Clear();
        }

        public static void AddToPlayerLimits(Player player, BuyablePerk buyablePerk)
		{
			var playerLimit = PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtPerks.ContainsKey(buyablePerk)) playerLimit.BoughtPerks[buyablePerk]++;
			else playerLimit.BoughtPerks[buyablePerk] = 1;
		}

		public static void AddToPlayerLimits(Player player, BuyableItem buyItem)
		{
			var playerLimit = PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtItems.ContainsKey(buyItem)) playerLimit.BoughtItems[buyItem]++;
            else playerLimit.BoughtItems[buyItem] = 1;
        }

        public static void AddToPlayerLimits(Player player, SellableItem sellItem)
		{
			var playerLimit = PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.SoldItems.ContainsKey(sellItem)) playerLimit.SoldItems[sellItem]++;
			else playerLimit.SoldItems[sellItem] = 1;
		}
        
        public static bool ExceedsPerkLimit(Player player, BuyablePerk buyablePerk)
        {
	        if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
	        var playerLimit = PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
	        if (playerLimit == null) return false;
	        var perkCount = playerLimit.BoughtPerks.Count(pair => pair.Key.PerkType == buyablePerk.PerkType);
	        return perkCount >= buyablePerk.Limit;
        }

        public static bool ExceedsItemLimit(Player player, BuyableItem buyItem)
        {
	        if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
	        var playerLimit = PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
	        if (playerLimit == null) return false;
	        var itemCount = playerLimit.BoughtItems.Count(pair => pair.Key.ItemType == buyItem.ItemType);
	        return itemCount >= buyItem.Limit;
        }
	}
}
