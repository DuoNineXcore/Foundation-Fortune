using System.Linq;
using Exiled.API.Features;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using FoundationFortune.API.Features.Systems;

namespace FoundationFortune.API.Core
{
    public static class IndexationMethods
	{
		public static void ClearIndexations()
        {
            foreach (var botData in FoundationFortune.Singleton.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            PerkSystem.PerkPlayers.Clear();
            FoundationFortune.Singleton.ConsumedPerks.Clear();
            FoundationFortune.PlayerPurchaseLimits.Clear();
	        NPCInitialization.sellingBotPositions.Clear();
	        NPCInitialization.buyingBotPositions.Clear();
	        SellingWorkstations.workstationPositions.Clear();
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
	}
}
