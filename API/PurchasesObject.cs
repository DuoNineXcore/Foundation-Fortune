using Exiled.API.Features;
using System.Collections.Generic;

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
