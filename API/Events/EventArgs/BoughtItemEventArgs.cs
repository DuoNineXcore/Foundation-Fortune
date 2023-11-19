using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune.API.Events.EventArgs;

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
