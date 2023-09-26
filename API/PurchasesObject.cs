using Exiled.API.Features;
using System.Collections.Generic;
using FoundationFortune.Configs;

namespace FoundationFortune.API
{
	public class PurchasesObject
	{
		public Player Player;
		public Dictionary<BuyableItem, int> BoughtItems = new();
		public Dictionary<PerkItem, int> BoughtPerks = new();

		public PurchasesObject(Player player)
		{
			Player = player;
		}
	}
}