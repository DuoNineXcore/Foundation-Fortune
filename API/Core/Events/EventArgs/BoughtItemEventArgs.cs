using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Core.Models.Classes.Items;

namespace FoundationFortune.API.Core.Events.EventArgs;

public class BoughtItemEventArgs : IExiledEvent
{
    public BoughtItemEventArgs(Player player, Npc npc, BuyableItem item)
    {
        Player = player;
        NPC = npc;
        BuyableItem = item;
    }
    
    public BuyableItem BuyableItem { get; }
    public Npc NPC { get; }
    public Player Player { get; }
}
