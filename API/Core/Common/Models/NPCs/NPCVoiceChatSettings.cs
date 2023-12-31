﻿using FoundationFortune.API.Core.Common.Enums.NPCs;
using VoiceChat;

namespace FoundationFortune.API.Core.Common.Models.NPCs;

public class NPCVoiceChatSettings
{
    public VoiceChatChannel VoiceChat { get; set; }
    public byte Volume { get; set; }
    public string AudioFile { get; set; }
    public NpcType BotType { get; set; }
    public NpcVoiceChatUsageType VoiceChatUsageType { get; set; }
    public bool Loop { get; set; }
}