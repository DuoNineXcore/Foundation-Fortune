using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Models.Items;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;

public class SoldItemEventArgs : IExiledEvent
{
    public SoldItemEventArgs(Player player, Npc npc, SellableItem sellableItem, Item item)
    {
        Player = player;
        Npc = npc;
        SellableItem = sellableItem;
        Item = item;
    }
    
    public SellableItem SellableItem { get; }
    public Item Item { get; }
    public Npc Npc { get; }
    public Player Player { get; }
}