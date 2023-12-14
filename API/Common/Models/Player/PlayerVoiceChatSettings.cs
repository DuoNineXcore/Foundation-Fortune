using FoundationFortune.API.Common.Enums.Player;
using VoiceChat;

namespace FoundationFortune.API.Common.Models.Player;

public class PlayerVoiceChatSettings
{
    public VoiceChatChannel VoiceChat { get; set; }
    public byte Volume { get; set; }
    public string AudioFile { get; set; }
    public PlayerVoiceChatUsageType VoiceChatUsageType { get; set; }
    public bool Loop { get; set; }
}