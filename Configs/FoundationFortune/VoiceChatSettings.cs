﻿using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using FoundationFortune.API.Core.Common.Models.NPCs;
using FoundationFortune.API.Core.Common.Models.Player;
using VoiceChat;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs.FoundationFortune;

public class VoiceChatSettings : IFoundationFortuneConfig
{
	[YamlIgnore] public string PropertyName { get; set; } = "Voice Chat Settings";
	
	[Description("NPC-Related Event Sound Effects.")]
	public List<NPCVoiceChatSettings> NpcVoiceChatSettings { get; set; } = new()
	{
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.Selling, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\Selling.ogg", Volume = 50 },
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.Buying, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\Buying.ogg", Volume = 50 },
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.WrongBot, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\WrongBot.ogg", Volume = 50 },
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.NotEnoughMoney, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\NoMoney.ogg", Volume = 50 },
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.BuyingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\BuyingRange.ogg", Volume = 50 },
		new() { VoiceChatUsageType = NpcVoiceChatUsageType.SellingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\SellingRange.ogg", Volume = 50 }
	};

	[Description("Player-Related Event Sound Effects.")]
	public List<PlayerVoiceChatSettings> PlayerVoiceChatSettings { get; set; } = new()
	{
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.GracefulSaint, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\GracefulSaint.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.BlissfulAgony, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\BlissfulAgony.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.ViolentImpulses, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\ViolentImpulses.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.EthericVitality, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\EthericVitality.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.HyperactiveBehaviorOn, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\HyperactiveBehaviorOn.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.HyperactiveBehaviorOff, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\HyperactiveBehaviorOff.ogg", Volume = 50},
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.ResurgenceBeacon, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Beacon.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunted, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Hunted.ogg", Volume = 50 },
		new() { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunter, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Hunter.ogg", Volume = 50 },
	};
}