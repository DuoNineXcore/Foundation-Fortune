using System.Collections.Generic;

namespace FoundationFortune.API.Core.Common.Models.Items;

public class ObjectInteractions
{
    public readonly Exiled.API.Features.Player Player;
    public readonly Dictionary<BuyableItem, int> BoughtItems = new();
    public readonly Dictionary<BuyablePerk, int> BoughtPerks = new();
    public readonly Dictionary<SellableItem, int> SoldItems = new();

    public ObjectInteractions(Exiled.API.Features.Player player) => Player = player;
}