using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Models.Items;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;

public class BoughtItemEventArgs : IExiledEvent
{
    public BoughtItemEventArgs(Player player, Npc npc, BuyableItem item)
    {
        Player = player;
        Npc = npc;
        BuyableItem = item;
    }
    
    public BuyableItem BuyableItem { get; }
    public Npc Npc { get; }
    public Player Player { get; }
}