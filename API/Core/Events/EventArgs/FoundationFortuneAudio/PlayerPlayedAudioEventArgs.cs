using Exiled.API.Features;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Core.Common.Models.Player;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;

public class PlayerPlayedAudioEventArgs : IExiledEvent
{
    public PlayerPlayedAudioEventArgs(Player player, PlayerVoiceChatSettings voiceChatSettings)
    {
        Player = player;
        PlayerVoiceChatSettings = voiceChatSettings;
    }

    public Player Player { get; }
    public PlayerVoiceChatSettings PlayerVoiceChatSettings { get; }
}