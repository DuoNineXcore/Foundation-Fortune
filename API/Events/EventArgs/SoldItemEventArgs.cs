using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune.API.Events.EventArgs;

public class SoldItemEventArgs : IExiledEvent
{
    public SoldItemEventArgs(Player player, Npc npc, SellableItem sellableItem, Item item)
    {
        Player = player;
        NPC = npc;
        SellableItem = sellableItem;
        Item = item;
    }
    
    public SellableItem SellableItem { get; }
    public Item Item { get; }
    public Npc NPC { get; }
    public Player Player { get; }
}