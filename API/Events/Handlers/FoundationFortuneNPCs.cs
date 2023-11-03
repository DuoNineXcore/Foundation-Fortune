using Exiled.Events.Features;
using FoundationFortune.API.Events.EventArgs;

namespace FoundationFortune.API.Events.Handlers
{
    public class FoundationFortuneNPC
    {
        public static Event<UsedFoundationFortuneNPCEventArgs> UsedFoundationFortuneNPC { get; set; } = new();
    
        public static void OnUsedFoundationFortuneNPC(UsedFoundationFortuneNPCEventArgs ev) => UsedFoundationFortuneNPC.InvokeSafely(ev);
    }   
}