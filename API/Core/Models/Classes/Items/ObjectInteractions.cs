using System.Collections.Generic;

namespace FoundationFortune.API.Core.Models.Classes.Items
{
    public class ObjectInteractions
    {
        public Exiled.API.Features.Player Player;
        public Dictionary<BuyableItem, int> BoughtItems = new();
        public Dictionary<BuyablePerk, int> BoughtPerks = new();
        public Dictionary<SellableItem, int> SoldItems = new();

        public ObjectInteractions(Exiled.API.Features.Player player) => Player = player;
    }
}
