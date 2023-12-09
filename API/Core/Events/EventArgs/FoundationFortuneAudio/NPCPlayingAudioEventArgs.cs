using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Core.Models.Classes.NPCs;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;

public class NPCPlayingAudioEventArgs : IExiledEvent
{
    public NPCPlayingAudioEventArgs(PlayerMusicBotPair pair, NPCVoiceChatSettings voiceChatSettings)
    {
        MusicBotPair = pair;
        NPCVoiceChatSettings = voiceChatSettings;
    }

    public PlayerMusicBotPair MusicBotPair { get; }
    public NPCVoiceChatSettings NPCVoiceChatSettings { get; }
}