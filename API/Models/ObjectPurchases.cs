using Exiled.API.Features;
using System.Collections.Generic;

namespace FoundationFortune.API.Models
{
	public class ObjectPurchases
	{
		public Player Player;
		public Dictionary<BuyableItem, int> BoughtItems = new();
		public Dictionary<PerkItem, int> BoughtPerks = new();

		public ObjectPurchases(Player player)
		{
			Player = player;
		}
	}
}