using Exiled.Events.Features;
using FoundationFortune.API.Events.EventArgs;

namespace FoundationFortune.API.Events.Handlers
{
    public class FoundationFortunePerks
    {
        public static Event<UsedFoundationFortunePerkEventArgs> UsedFoundationFortunePerk { get; set; } = new();
    
        public static void OnUsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev) => UsedFoundationFortunePerk.InvokeSafely(ev);
    }   
}
