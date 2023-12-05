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
		public static void ClearIndexations()
        {
            foreach (var botData in FoundationFortune.Instance.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Instance.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Instance.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            EthericVitality.EthericVitalityPlayers.Clear();
            HyperactiveBehavior.HyperactiveBehaviorPlayers.Clear();
            EtherealIntervention.EtherealInterventionPlayers.Clear();
            ViolentImpulses.ViolentImpulsesPlayers.Clear();
            PerkSystem.ConsumedPerks.Clear();
            FoundationFortune.PlayerPurchaseLimits.Clear();
	        NpcInitialization.SellingBotPositions.Clear();
	        NpcInitialization.BuyingBotPositions.Clear();
	        SellingWorkstations.WorkstationPositions.Clear();
        }

        public static void AddToPlayerLimits(Player player, BuyablePerk buyablePerk)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtPerks.ContainsKey(buyablePerk)) playerLimit.BoughtPerks[buyablePerk]++;
			else playerLimit.BoughtPerks[buyablePerk] = 1;
		}

		public static void AddToPlayerLimits(Player player, BuyableItem buyItem)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtItems.ContainsKey(buyItem)) playerLimit.BoughtItems[buyItem]++;
            else playerLimit.BoughtItems[buyItem] = 1;
        }

        public static void AddToPlayerLimits(Player player, SellableItem sellItem)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.SoldItems.ContainsKey(sellItem)) playerLimit.SoldItems[sellItem]++;
			else playerLimit.SoldItems[sellItem] = 1;
		}
        
        public static bool ExceedsPerkLimit(Player player, BuyablePerk buyablePerk)
        {
	        if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
	        var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
	        if (playerLimit == null) return false;
	        var perkCount = playerLimit.BoughtPerks.Count(pair => pair.Key.PerkType == buyablePerk.PerkType);
	        return perkCount >= buyablePerk.Limit;
        }

        public static bool ExceedsItemLimit(Player player, BuyableItem buyItem)
        {
	        if (PlayerDataRepository.GetPluginAdmin(player.UserId)) return false;
	        var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
	        if (playerLimit == null) return false;
	        var itemCount = playerLimit.BoughtItems.Count(pair => pair.Key.ItemType == buyItem.ItemType);
	        return itemCount >= buyItem.Limit;
        }
	}
}
