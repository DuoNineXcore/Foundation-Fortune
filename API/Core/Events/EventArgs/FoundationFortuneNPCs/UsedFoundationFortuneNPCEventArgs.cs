using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Enums.NPCs;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneNPCs;

public class UsedFoundationFortuneNpcEventArgs : IExiledEvent
{
    public UsedFoundationFortuneNpcEventArgs(Player player, Npc npc, NpcType type, NpcUsageOutcome outcome)
    {
        Player = player;
        Npc = npc;
        Type = type;
        Outcome = outcome;
    }

    public NpcUsageOutcome Outcome { get; }
    public NpcType Type { get; }
    public Npc Npc { get; }
    public Player Player { get; }
}