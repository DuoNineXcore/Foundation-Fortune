using Exiled.Events.Features;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneNPCs;

namespace FoundationFortune.API.Core.Events.Handlers;

public class FoundationFortuneNPCEvents
{
    /// <summary>
    /// Event invoked after using a Foundation Fortune NPC.
    /// </summary>
    public static Event<UsedFoundationFortuneNpcEventArgs> UsedFoundationFortuneNpc { get; set; } = new();
    public static void OnUsedFoundationFortuneNPC(UsedFoundationFortuneNpcEventArgs ev) => UsedFoundationFortuneNpc.InvokeSafely(ev);
}