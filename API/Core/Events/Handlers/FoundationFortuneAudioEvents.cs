using Exiled.Events.Features;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;

namespace FoundationFortune.API.Core.Events.Handlers
{
    public class FoundationFortuneAudioEvents
    {
        public static Event<NPCPlayedAudioEventArgs> NPCPlayedAudio { get; set; } = new();
        public static void OnNPCPlayedAudio(NPCPlayedAudioEventArgs ev) => NPCPlayedAudio.InvokeSafely(ev);
        
        public static Event<NPCPlayingAudioEventArgs> NPCPlayingAudio { get; set; } = new();
        public static void OnNPCPlayingAudio(NPCPlayingAudioEventArgs ev) => NPCPlayingAudio.InvokeSafely(ev);
        
        public static Event<PlayerPlayedAudioEventArgs> PlayerPlayedAudio { get; set; } = new();
        public static void OnPlayerPlayedAudio(PlayerPlayedAudioEventArgs ev) => PlayerPlayedAudio.InvokeSafely(ev);
        
        public static Event<PlayerPlayingAudioEventArgs> PlayerPlayingAudio { get; set; } = new();
        public static void OnPlayerPlayingAudio(PlayerPlayingAudioEventArgs ev) => PlayerPlayingAudio.InvokeSafely(ev);
    }   
}