using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Core.Models.Classes.NPCs;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Core.Models.Interfaces;
using FoundationFortune.API.Core.Models.Interfaces.Configs;
using VoiceChat;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs
{
	public class VoiceChatSettings : IFoundationFortuneConfig
	{
		[YamlIgnore] public string PropertyName { get; set; } = "Voice Chat Settings";
	
		[Description("NPC-Related Event Sound Effects.")]
		public List<NPCVoiceChatSettings> NpcVoiceChatSettings { get; set; } = new List<NPCVoiceChatSettings>()
		{
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.Selling, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\Selling.ogg", Volume = 50 },
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.Buying, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\Buying.ogg", Volume = 50 },
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.WrongBot, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\WrongBot.ogg", Volume = 50 },
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.NotEnoughMoney, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\NoMoney.ogg", Volume = 50 },
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.BuyingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\BuyingRange.ogg", Volume = 50 },
			new NPCVoiceChatSettings { VoiceChatUsageType = NpcVoiceChatUsageType.SellingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NPCVoiceChatUsageType\\SellingRange.ogg", Volume = 50 }
		};

		[Description("Player-Related Event Sound Effects.")]
		public List<PlayerVoiceChatSettings> PlayerVoiceChatSettings { get; set; } = new List<PlayerVoiceChatSettings>()
		{
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.EtherealIntervention, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\EtherealIntervention.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.BlissfulUnawareness, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\BlissfulUnawareness.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.ViolentImpulses, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\ViolentImpulses.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.EthericVitality, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\EthericVitality.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.HyperactiveBehaviorOn, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\HyperactiveBehaviorOn.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.HyperactiveBehaviorOff, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\HyperactiveBehaviorOff.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.ResurgenceBeacon, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Beacon.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunted, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Hunted.ogg", Volume = 50 },
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunter, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "PlayerVoiceChatUsageType\\Hunter.ogg", Volume = 50 },
		};
	}
}