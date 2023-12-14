using Exiled.Events.Features;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;

namespace FoundationFortune.API.Core.Events.Handlers;

public class FoundationFortunePerkEvents
{
    /// <summary>
    /// Event invoked After Drinking a Perk.
    /// </summary>
    public static Event<UsedFoundationFortunePerkEventArgs> UsedFoundationFortunePerk { get; set; } = new();
    public static void OnUsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev) => UsedFoundationFortunePerk.InvokeSafely(ev);
}