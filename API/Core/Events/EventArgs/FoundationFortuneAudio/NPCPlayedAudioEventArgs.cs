using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Models.NPCs;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;

public class NPCPlayedAudioEventArgs : IExiledEvent
{
    public NPCPlayedAudioEventArgs(PlayerMusicBotPair pair, NPCVoiceChatSettings voiceChatSettings)
    {
        MusicBotPair = pair;
        NPCVoiceChatSettings = voiceChatSettings;
    }

    public PlayerMusicBotPair MusicBotPair { get; }
    public NPCVoiceChatSettings NPCVoiceChatSettings { get; }
}