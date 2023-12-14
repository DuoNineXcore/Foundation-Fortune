using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Common.Models.Player;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;

public class PlayerPlayingAudioEventArgs : IExiledEvent
{
    public PlayerPlayingAudioEventArgs(Player player, PlayerVoiceChatSettings voiceChatSettings)
    {
        Player = player;
        PlayerVoiceChatSettings = voiceChatSettings;
    }

    public Player Player { get; }
    public PlayerVoiceChatSettings PlayerVoiceChatSettings { get; }
}