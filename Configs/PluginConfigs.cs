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

		[Description("Number of hints that can be shown")]
		public int MaxHintsToShow { get; set; } = 3;

		[Description("Killing player event.")]
		public int KillReward { get; set; } = 300;
		public bool KillRewardScpOnly { get; set; } = false;

		[Description("Escaping player event.")]
		public int EscapeReward { get; set; } = 300;

		[Description("Revival Settings.")]
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

		[Description("Update Rate Settings")]
		public float HintSystemUpdateRate { get; set; } = 0.5f;
		public float AnimatedHintUpdateRate { get; set; } = 0.5f;

		[Description("Amount of Death Coins to drop. NOTE: the value of the coins will be divided by the amount of coins. so if there's 10 coins a coin will be worth a tenth of the player's on hold money account.")]
		public int DeathCoinsToDrop { get; set; } = 10;

		[Description("Lifespan of each hint.")]
		public float MaxHintAge { get; set; } = 3f;

		[Description("Selling Workstation Settings.")]
		public bool UseSellingWorkstation { get; set; } = false;
		public float SellingWorkstationRadius { get; set; } = 3f;

		[Description("Buying/Selling Bot Settings.")]
		public bool UseBuyingBot { get; set; } = true;
		public float BuyingBotRadius { get; set; } = 3f;
		public bool BuyingBotFixedLocation { get; set; } = true;

		public List<NPCSpawn> BuyingBotSpawnSettings { get; set; } = new List<NPCSpawn>
		{
			new NPCSpawn { Name = "Buying Bot 1", Badge = "Foundation Fortune", BadgeColor = "pumpkin", IsSellingBot = false, Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.HczNuke },
			new NPCSpawn { Name = "Buying Bot 2", Badge = "Foundation Fortune", BadgeColor = "pumpkin", IsSellingBot = false, Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz079 },
			new NPCSpawn { Name = "Selling Bot 3",Badge = "Foundation Fortune", BadgeColor = "yellow", IsSellingBot = true, Role = RoleTypeId.Scientist, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz096 },
			new NPCSpawn { Name = "Selling Bot 4",Badge = "Foundation Fortune", BadgeColor = "yellow", IsSellingBot = true, Role = RoleTypeId.Scientist, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz939 },
			new NPCSpawn { Name = "Buying Bot 5", Badge = "Foundation Fortune", BadgeColor = "pumpkin", IsSellingBot = false, Role = RoleTypeId.ClassD, HeldItem = ItemType.KeycardChaosInsurgency, Scale = new Vector3(1, 1, 1), Room = RoomType.HczArmory }
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

		[Description("The time you have to sell an item after asking for confirmation.")]
		public float SellingConfirmationTime { get; set; } = 5f;

		[Description("NPC-Related Event Sound Effects.")]
		public List<NPCVoiceChatSettings> NPCVoiceChatSettings { get; set; } = new List<NPCVoiceChatSettings>()
		{
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.Selling, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.Buying, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.WrongBuyingBot, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.NotEnoughMoney, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.BuyingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new NPCVoiceChatSettings { VoiceChatUsageType = NPCVoiceChatUsageType.SellingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50}
		};

		[Description("Player-Related Event Sound Effects.")]
		public List<PlayerVoiceChatSettings> PlayerVoiceChatSettings { get; set; } = new List<PlayerVoiceChatSettings>()
		{
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.EtherealIntervention, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
		  new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.BlissfulUnawareness, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "urgoingtodie.ogg", Volume = 50},
		  new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.ResurgenceBeacon, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.ResurgenceBeacon, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunted, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
			new PlayerVoiceChatSettings { VoiceChatUsageType = PlayerVoiceChatUsageType.Hunter, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
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

		[Description("List of perks that can be bought.")]
		public List<PerkItem> PerkItems { get; set; } = new List<PerkItem>
	   {
			new PerkItem { Limit = 1, Alias = "OSP", PerkType = PerkType.OvershieldedProtection, Price = 2500, DisplayName = "Overshielded Protection", Description = ""},
			new PerkItem { Limit = 1, Alias = "BRS", PerkType = PerkType.BoostedResilience, Price = 1800, DisplayName = "Boosted Resilience", Description = ""},
			new PerkItem { Limit = 1, Alias = "CPR", PerkType = PerkType.ConcealedPresence, Price = 2000, DisplayName = "Concealed Presence", Description = ""},
			new PerkItem { Limit = 1, Alias = "EVT", PerkType = PerkType.EthericVitality, Price = 2800, DisplayName = "Etheric Vitality", Description = ""},
			new PerkItem { Limit = 1, Alias = "HAC", PerkType = PerkType.Hyperactivity, Price = 3200, DisplayName = "Hyperactivity", Description = ""},
			new PerkItem { Limit = 1, Alias = "BUA", PerkType = PerkType.BlissfulUnawareness, Price = 3500, DisplayName = "Blissful Unawareness", Description = ""},
			new PerkItem { Limit = 1, Alias = "ESP", PerkType = PerkType.ExtrasensoryPerception, Price = 4000, DisplayName = "Extrasensory Perception", Description = ""},
			new PerkItem { Limit = 1, Alias = "RBC", PerkType = PerkType.ResurgenceBeacon, Price = 3000, DisplayName = "Resurgence Beacon", Description = ""},
			new PerkItem { Limit = 1, Alias = "EIN", PerkType = PerkType.EtherealIntervention, Price = 4200, DisplayName = "Ethereal Intervention", Description = ""},
		};

		[Description("A list of rooms that you cannot be teleported to")]
		public List<RoomType> ForbiddenRooms { get; set; } = new()
		{
			RoomType.EzCollapsedTunnel,
			RoomType.HczTestRoom,
			RoomType.Hcz049,
			RoomType.Lcz173,
			RoomType.HczTesla
		};
	}
}
