using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Core.Models.Classes.Items;

namespace FoundationFortune.API.Core.Events.EventArgs;

public class BoughtPerkEventArgs : IExiledEvent
{
    public BoughtPerkEventArgs(Player player, Npc npc, BuyablePerk buyablePerk)
    {
        Player = player;
        NPC = npc;
        BuyablePerk = buyablePerk;
    }

    public BuyablePerk BuyablePerk { get; }
    public Npc NPC { get; }
    public Player Player { get; }
}