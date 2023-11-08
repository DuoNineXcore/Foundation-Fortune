using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Models.Enums.NPCs;

namespace FoundationFortune.API.Events.EventArgs
{
    public class UsedFoundationFortuneNPCEventArgs : IExiledEvent
    {
        public UsedFoundationFortuneNPCEventArgs(Player player, Npc npc, NpcType type, NpcUsageOutcome outcome)
        {
            Player = player;
            NPC = npc;
            Type = type;
            Outcome = outcome;
        }

        public NpcUsageOutcome Outcome { get; }
        public NpcType Type { get; }
        public Npc NPC { get; }
        public Player Player { get; }
    }
}
