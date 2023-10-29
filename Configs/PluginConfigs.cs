using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;
using System.ComponentModel;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using NPCVoiceChatSettings = FoundationFortune.API.Models.Classes.NPCVoiceChatSettings;
using PlayerVoiceChatSettings = FoundationFortune.API.Models.Classes.PlayerVoiceChatSettings;

namespace FoundationFortune.Configs
{
	public class PluginConfigs : IConfig
	{
		[Description("Plugin Settings")]
		public bool IsEnabled { get; set; } = true;
		public bool Debug { get; set; } = true;
		
		[Description("Server Events with Rewards.")]
		public bool KillEvent { get; set; } = true;
        public bool EscapeEvent { get; set; } = true;
		public bool RoundEndEvent { get; set; } = true;
		public bool KillEventRewardsOnlySCPS { get; set; } = false;
        public int KillEventRewards { get; set; } = 300;
		public int EscapeRewards { get; set; } = 300;
        public Dictionary<PlayerTeamConditions, int> RoundEndRewards { get; set; } = new Dictionary<PlayerTeamConditions, int>
		{
			{ PlayerTeamConditions.Winning, 500 },
			{ PlayerTeamConditions.Losing, 100 },
			{ PlayerTeamConditions.Draw, 250 }
		};

        [Description("Resurgence Beacon Settings.")]
		public bool HuntReviver { get; set; } = true;
		public int RevivedPlayerHealth { get; set; } = 30;
		public bool ResetRevivedInventory { get; set; } = false;
		public int RevivalBountyKillReward { get; set; } = 5000;
		public int RevivalBountyTimeSeconds { get; set; } = 300;

        [Description("Money Extraction System Settings.")]
		public bool MoneyExtractionSystem { get; set; } = true;
		public List<RoomType> ExtractionPointRooms { get; set; } = new List<RoomType>
		{
			RoomType.LczToilets,
			RoomType.Lcz914,
			RoomType.HczHid,
			RoomType.HczNuke,
		};
		public int ExtractionLimit { get; set; } = 5;
		public int MinExtractionPointGenerationTime { get; set; } = 15;
		public int MaxExtractionPointGenerationTime { get; set; } = 30;
		public int ExtractionPointDuration { get; set; } = 120;

		[Description("Hint System Update Rate Settings")] 
		public float HintSystemUpdateRate { get; set; } = 0.5f;
		public float AnimatedHintUpdateRate { get; set; } = 0.5f;

		[Description("Amount of Death Coins to drop. NOTE: the value of the coins will be divided by the amount of coins. so if there's 10 coins a coin will be worth a tenth of the player's on hold money account.")]
		public int DeathCoinsToDrop { get; set; } = 10;

		[Description("Lifespan of each hint.")]
		public float MaxHintAge { get; set; } = 3f;

		[Description("Selling Workstation Settings.")]
		public bool UseSellingWorkstation { get; set; } = false;
		public float SellingWorkstationRadius { get; set; } = 3f;

		[Description("Perk System Settings")] 
		public float ViolentImpulsesDamageMultiplier { get; set; } = 1.2f;

		public float ViolentImpulsesRecoilMultiplier { get; set; } = 1.2f;
		public Dictionary<PerkType, string> PerkEmojis { get; set; } = new Dictionary<PerkType, string>
		{
			{ PerkType.ViolentImpulses, "🔪" }, 
			{ PerkType.EthericVitality, "❤️" },
			{ PerkType.Hyperactivity, "🏃" },
			{ PerkType.BlissfulUnawareness, "💞" },
			{ PerkType.ExtrasensoryPerception, "◎" },
			{ PerkType.EtherealIntervention, "✚" } 
		};
        
		public List<PerkItem> PerkItems { get; set; } = new List<PerkItem>
		{
			new PerkItem { Limit = 1, Alias = "speed", PerkType = PerkType.Hyperactivity, Price = 2500, DisplayName = "Hyperactivity", Description = ""},
			new PerkItem { Limit = 1, Alias = "regen", PerkType = PerkType.EthericVitality, Price = 2800, DisplayName = "Etheric Vitality", Description = ""},
			new PerkItem { Limit = 1, Alias = "violence", PerkType = PerkType.ViolentImpulses, Price = 3000, DisplayName = "Violent Impulses", Description = ""},
			new PerkItem { Limit = 1, Alias = "bliss", PerkType = PerkType.BlissfulUnawareness, Price = 3500, DisplayName = "Blissful Unawareness", Description = ""},
			new PerkItem { Limit = 1, Alias = "revive", PerkType = PerkType.ResurgenceBeacon, Price = 3000, DisplayName = "Resurgence Beacon", Description = ""},
			new PerkItem { Limit = 1, Alias = "selfres", PerkType = PerkType.EtherealIntervention, Price = 4200, DisplayName = "Ethereal Intervention", Description = ""},
		};

		[Description("A list of rooms that you cannot be teleported to at your revival")]
		public List<RoomType> ForbiddenRooms { get; set; } = new()
		{
			RoomType.EzCollapsedTunnel,
			RoomType.HczTestRoom,
			RoomType.Hcz049,
			RoomType.Lcz173,
			RoomType.HczTesla,
			RoomType.HczHid,
			RoomType.Lcz330
		};
		
		[Description("Foundation Fortune NPC Settings.")]
		public bool FoundationFortuneNPCs { get; set; } = true;

		public bool BuyingBots { get; set; } = true;
		public bool SellingBots { get; set; } = true;
		public bool MusicBots { get; set; } = true;

		public float BuyingBotRadius { get; set; } = 3f;
		public bool BuyingBotFixedLocation { get; set; } = true;
		public List<BuyingBotSpawn> BuyingBotSpawnSettings { get; set; } = new List<BuyingBotSpawn>
		{
			new BuyingBotSpawn { Name = "Buying Bot 1", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.HczNuke },
			new BuyingBotSpawn { Name = "Buying Bot 2", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz079 },
			new BuyingBotSpawn { Name = "Buying Bot 3", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.HczArmory }
		};

        public List<RoomType> BuyingBotRandomRooms { get; set; } = new List<RoomType>()
        {
            RoomType.EzCafeteria,
            RoomType.EzCollapsedTunnel,
            RoomType.HczStraight,
            RoomType.HczNuke,
            RoomType.HczTesla,
            RoomType.LczClassDSpawn,
            RoomType.EzCheckpointHallway,
            RoomType.HczServers,
        };

        public float SellingBotRadius { get; set; } = 3f;
        public bool SellingBotFixedLocation { get; set; } = true;
        public List<SellingBotSpawn> SellingBotSpawnSettings { get; set; } = new List<SellingBotSpawn>
        {
            new SellingBotSpawn { Name = "Selling Bot 1",Badge = "Foundation Fortune", BadgeColor = "yellow", Role = RoleTypeId.Scientist, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz096 },
            new SellingBotSpawn { Name = "Selling Bot 2",Badge = "Foundation Fortune", BadgeColor = "yellow", Role = RoleTypeId.Scientist, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz939 },
        };

        public List<RoomType> SellingBotRandomRooms { get; set; } = new List<RoomType>()
        {
            RoomType.EzCafeteria,
            RoomType.EzCollapsedTunnel,
            RoomType.HczStraight,
            RoomType.HczNuke,
            RoomType.HczTesla,
            RoomType.LczClassDSpawn,
            RoomType.EzCheckpointHallway,
            RoomType.HczServers,
        };

        [Description("The time you have to sell an item after asking for confirmation.")]
		public float SellingConfirmationTime { get; set; } = 5f;

		[Description("NPC-Related Event Sound Effects.")]
		public List<NPCVoiceChatSettings> FFNPCVoiceChatSettings { get; set; } = new List<NPCVoiceChatSettings>()
		{
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.Selling, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "SellSuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.Buying, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.WrongBuyingBot, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "WrongBot.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.NotEnoughMoney, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "NoMoney.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.BuyingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuyingRange.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.SellingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "SellingRange.ogg", Volume = 50}
		};

		[Description("Player-Related Event Sound Effects.")]
		public List<PlayerVoiceChatSettings> PlayerVoiceChatSettings { get; set; } = new List<PlayerVoiceChatSettings>()
		{
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.EtherealIntervention, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.BlissfulUnawareness, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "Explode.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.ResurgenceBeacon, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "Beacon.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunted, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "Hunted.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunter, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "Hunter.ogg", Volume = 50},
		};

		[Description("List of items that can be sold.")]
		public List<SellableItem> SellableItems { get; set; } = new List<SellableItem>
		{
			new SellableItem { Limit = 1, ItemType = ItemType.MicroHID, Price = 1200, DisplayName = "Micro HID" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP500, Price = 1000, DisplayName = "SCP-500" },
		};

		[Description("List of items that can be bought.")]
		public List<BuyableItem> BuyableItems { get; set; } = new List<BuyableItem>
		{
			new BuyableItem { Limit = 1, Alias = "Micro", ItemType = ItemType.MicroHID, Price = 500, DisplayName = "Micro HID" },
			new BuyableItem { Limit = 1, Alias = "500", ItemType = ItemType.SCP500, Price = 1000, DisplayName = "SCP-500" },
		};
	}
}
