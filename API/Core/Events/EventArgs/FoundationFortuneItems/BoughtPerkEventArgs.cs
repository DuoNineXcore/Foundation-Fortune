using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Models.Items;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;

public class BoughtPerkEventArgs : IExiledEvent
{
    public BoughtPerkEventArgs(Player player, Npc npc, BuyablePerk buyablePerk)
    {
        Player = player;
        Npc = npc;
        BuyablePerk = buyablePerk;
    }

    public BuyablePerk BuyablePerk { get; }
    public Npc Npc { get; }
    public Player Player { get; }
}