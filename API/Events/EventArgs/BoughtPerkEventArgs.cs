using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune.API.Events.EventArgs;

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