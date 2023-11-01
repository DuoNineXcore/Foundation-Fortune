using System.ComponentModel;
using NPCVoiceChatSettings = FoundationFortune.API.Models.NPCVoiceChatSettings;
using PlayerVoiceChatSettings = FoundationFortune.API.Models.PlayerVoiceChatSettings;
using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using FoundationFortune.API.Models;
using UnityEngine;
using VoiceChat;

namespace FoundationFortune.Configs
{
	public class PluginConfigs : IConfig
	{
		[Description("Plugin Settings")]
		public bool IsEnabled { get; set; } = true;
		public bool Debug { get; set; } = true;
		
		public bool DirectoryIterator { get; set; } = true;
		public bool DirectoryIteratorCheckDatabase { get; set; } = true;
		public bool DirectoryIteratorCheckAudio { get; set; } = true;

		[Description("Server Events")]
		public bool KillEvent { get; set; } = true;
		public bool EscapeEvent { get; set; } = true;
		public bool RoundEndEvent { get; set; } = true;
		
		[Description("Server Event Settings")]
		public bool KillEventRewardsOnlySCPS { get; set; } = false;
		public int KillEventRewards { get; set; } = 300;
		public int EscapeRewards { get; set; } = 300;
		public Dictionary<PlayerTeamConditions, int> RoundEndRewards { get; set; } = new Dictionary<PlayerTeamConditions, int>
		{
			{ PlayerTeamConditions.Winning, 500 },
			{ PlayerTeamConditions.Losing, 100 },
			{ PlayerTeamConditions.Draw, 250 }
		};

		[Description("Money Extraction System Settings.")]
		public bool MoneyExtractionSystem { get; set; } = false;
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
		public float ViolentImpulsesRecoilAnimationTime { get; set; } = 1.2f;
		public float ViolentImpulsesRecoilZAxis { get; set; } = 1.2f;
		public float ViolentImpulsesRecoilUpKick { get; set; } = 1.2f;
		public float ViolentImpulsesRecoilFovKick { get; set; } = 1.2f;
		public float ViolentImpulsesRecoilSideKick { get; set; } = 1.2f;
		public bool HuntReviver { get; set; } = true;
		public int RevivedPlayerHealth { get; set; } = 30;
		public bool ResetRevivedInventory { get; set; } = false;
		public int RevivalBountyKillReward { get; set; } = 5000;
		public int RevivalBountyTimeSeconds { get; set; } = 300;
		
		public List<PerkItem> PerkItems { get; set; } = new List<PerkItem>
		{
			new PerkItem { Limit = 1, Alias = "speed", PerkType = PerkType.HyperactiveBehavior, Price = 2500, DisplayName = "Hyperactivity", Description = ""},
			new PerkItem { Limit = 1, Alias = "regen", PerkType = PerkType.EthericVitality, Price = 2800, DisplayName = "Etheric Vitality", Description = ""},
			new PerkItem { Limit = 1, Alias = "damage", PerkType = PerkType.ViolentImpulses, Price = 3000, DisplayName = "Violent Impulses", Description = ""},
			new PerkItem { Limit = 1, Alias = "bliss", PerkType = PerkType.BlissfulUnawareness, Price = 3400, DisplayName = "Blissful Unawareness", Description = ""},
			new PerkItem { Limit = 1, Alias = "revive", PerkType = PerkType.ResurgenceBeacon, Price = 4000, DisplayName = "Resurgence Beacon", Description = ""},
			new PerkItem { Limit = 1, Alias = "selfres", PerkType = PerkType.EtherealIntervention, Price = 4000, DisplayName = "Ethereal Intervention", Description = ""},
		};

		[Description("A list of rooms that you cannot be teleported to at your revival")]
		public List<RoomType> ForbiddenEtherealInterventionRoomTypes { get; set; } = new()
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
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardJanitor, Price = 50, DisplayName = "Janitor Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardScientist, Price = 100, DisplayName = "Scientist Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardResearchCoordinator, Price = 120, DisplayName = "Research Coordinator Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardZoneManager, Price = 150, DisplayName = "Zone Manager Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardGuard, Price = 200, DisplayName = "Guard Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFPrivate, Price = 250, DisplayName = "MTF Private Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardContainmentEngineer, Price = 300, DisplayName = "Containment Engineer Keycard" }, 
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFOperative, Price = 350, DisplayName = "MTF Operative Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFCaptain, Price = 400, DisplayName = "MTF Captain Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardFacilityManager, Price = 450, DisplayName = "Facility Manager Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardChaosInsurgency, Price = 500, DisplayName = "Chaos Insurgency Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.KeycardO5, Price = 600, DisplayName = "O5 Keycard" },
			new SellableItem { Limit = 1, ItemType = ItemType.Radio, Price = 75, DisplayName = "Radio" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunCOM15, Price = 150, DisplayName = "COM-15 Pistol" },
			new SellableItem { Limit = 1, ItemType = ItemType.Medkit, Price = 100, DisplayName = "Medkit" },
			new SellableItem { Limit = 1, ItemType = ItemType.Flashlight, Price = 20, DisplayName = "Flashlight" },
			new SellableItem { Limit = 1, ItemType = ItemType.MicroHID, Price = 600, DisplayName = "Micro HID" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP500, Price = 800, DisplayName = "SCP-500" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP207, Price = 300, DisplayName = "SCP-207" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunE11SR, Price = 600, DisplayName = "E-11 SR" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunCrossvec, Price = 700, DisplayName = "Crossvec" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunFSP9, Price = 550, DisplayName = "FSP-9" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunLogicer, Price = 900, DisplayName = "Logicer" },
			new SellableItem { Limit = 1, ItemType = ItemType.GrenadeHE, Price = 200, DisplayName = "HE Grenade" },
			new SellableItem { Limit = 1, ItemType = ItemType.GrenadeFlash, Price = 150, DisplayName = "Flash Grenade" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunCOM18, Price = 450, DisplayName = "COM-18 Pistol" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP018, Price = 300, DisplayName = "SCP-018" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP268, Price = 500, DisplayName = "SCP-268" },
			new SellableItem { Limit = 1, ItemType = ItemType.Adrenaline, Price = 75, DisplayName = "Adrenaline" },
			new SellableItem { Limit = 1, ItemType = ItemType.Painkillers, Price = 60, DisplayName = "Painkillers" },
			new SellableItem { Limit = 1, ItemType = ItemType.Coin, Price = 1, DisplayName = "Coin" },
			new SellableItem { Limit = 1, ItemType = ItemType.ArmorLight, Price = 150, DisplayName = "Light Armor" },
			new SellableItem { Limit = 1, ItemType = ItemType.ArmorCombat, Price = 250, DisplayName = "Combat Armor" },
			new SellableItem { Limit = 1, ItemType = ItemType.ArmorHeavy, Price = 350, DisplayName = "Heavy Armor" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunRevolver, Price = 500, DisplayName = "Revolver" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunAK, Price = 700, DisplayName = "AK" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunShotgun, Price = 800, DisplayName = "Shotgun" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP330, Price = 350, DisplayName = "SCP-330" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP2176, Price = 450, DisplayName = "SCP-2176" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP244a, Price = 550, DisplayName = "SCP-244a" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP244b, Price = 550, DisplayName = "SCP-244b" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP1853, Price = 400, DisplayName = "SCP-1853" },
			new SellableItem { Limit = 1, ItemType = ItemType.ParticleDisruptor, Price = 800, DisplayName = "Particle Disruptor" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunCom45, Price = 500, DisplayName = "COM-45 Pistol" },
			new SellableItem { Limit = 1, ItemType = ItemType.SCP1576, Price = 650, DisplayName = "SCP-1576" },
			new SellableItem { Limit = 1, ItemType = ItemType.Jailbird, Price = 250, DisplayName = "Jailbird" },
			new SellableItem { Limit = 1, ItemType = ItemType.AntiSCP207, Price = 150, DisplayName = "Anti-SCP-207" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunFRMG0, Price = 900, DisplayName = "FRMG-0" },
			new SellableItem { Limit = 1, ItemType = ItemType.GunA7, Price = 700, DisplayName = "A7" },
			new SellableItem { Limit = 1, ItemType = ItemType.Lantern, Price = 10, DisplayName = "Lantern" },
		};

		[Description("List of items that can be bought.")]
		public List<BuyableItem> BuyableItems { get; set; } = new List<BuyableItem>
		{
			new BuyableItem { Limit = 1, Alias = "Janitor", ItemType = ItemType.KeycardJanitor, Price = 100, DisplayName = "Janitor Keycard" },
			new BuyableItem { Limit = 1, Alias = "Scientist", ItemType = ItemType.KeycardScientist, Price = 200, DisplayName = "Scientist Keycard" },
			new BuyableItem { Limit = 1, Alias = "ResearchCoordinator", ItemType = ItemType.KeycardResearchCoordinator, Price = 250, DisplayName = "Research Coordinator Keycard" },
			new BuyableItem { Limit = 1, Alias = "ZoneManager", ItemType = ItemType.KeycardZoneManager, Price = 300, DisplayName = "Zone Manager Keycard" },
			new BuyableItem { Limit = 1, Alias = "Guard", ItemType = ItemType.KeycardGuard, Price = 400, DisplayName = "Guard Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFPrivate", ItemType = ItemType.KeycardMTFPrivate, Price = 500, DisplayName = "MTF Private Keycard" },
			new BuyableItem { Limit = 1, Alias = "ContainmentEngineer", ItemType = ItemType.KeycardContainmentEngineer, Price = 600, DisplayName = "Containment Engineer Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFOperative", ItemType = ItemType.KeycardMTFOperative, Price = 700, DisplayName = "MTF Operative Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFCaptain", ItemType = ItemType.KeycardMTFCaptain, Price = 800, DisplayName = "MTF Captain Keycard" },
			new BuyableItem { Limit = 1, Alias = "FacilityManager", ItemType = ItemType.KeycardFacilityManager, Price = 900, DisplayName = "Facility Manager Keycard" },
			new BuyableItem { Limit = 1, Alias = "ChaosInsurgency", ItemType = ItemType.KeycardChaosInsurgency, Price = 1000, DisplayName = "Chaos Insurgency Keycard" },
			new BuyableItem { Limit = 1, Alias = "O5", ItemType = ItemType.KeycardO5, Price = 1200, DisplayName = "O5 Keycard" },
			new BuyableItem { Limit = 1, Alias = "Radio", ItemType = ItemType.Radio, Price = 300, DisplayName = "Radio" },
			new BuyableItem { Limit = 1, Alias = "GunCOM15", ItemType = ItemType.GunCOM15, Price = 800, DisplayName = "COM-15 Pistol" },
			new BuyableItem { Limit = 1, Alias = "Medkit", ItemType = ItemType.Medkit, Price = 600, DisplayName = "Medkit" },
			new BuyableItem { Limit = 1, Alias = "Flashlight", ItemType = ItemType.Flashlight, Price = 100, DisplayName = "Flashlight" },
			new BuyableItem { Limit = 1, Alias = "MicroHID", ItemType = ItemType.MicroHID, Price = 1500, DisplayName = "Micro HID" },
			new BuyableItem { Limit = 1, Alias = "SCP500", ItemType = ItemType.SCP500, Price = 2000, DisplayName = "SCP-500" },
			new BuyableItem { Limit = 1, Alias = "SCP207", ItemType = ItemType.SCP207, Price = 800, DisplayName = "SCP-207" },
			new BuyableItem { Limit = 1, Alias = "Ammo12gauge", ItemType = ItemType.Ammo12gauge, Price = 50, DisplayName = "12 Gauge Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunE11SR", ItemType = ItemType.GunE11SR, Price = 1200, DisplayName = "E-11 SR" },
			new BuyableItem { Limit = 1, Alias = "GunCrossvec", ItemType = ItemType.GunCrossvec, Price = 1400, DisplayName = "Crossvec" },
			new BuyableItem { Limit = 1, Alias = "Ammo556x45", ItemType = ItemType.Ammo556x45, Price = 100, DisplayName = "5.56x45mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunFSP9", ItemType = ItemType.GunFSP9, Price = 1100, DisplayName = "FSP-9" },
			new BuyableItem { Limit = 1, Alias = "GunLogicer", ItemType = ItemType.GunLogicer, Price = 1800, DisplayName = "Logicer" },
			new BuyableItem { Limit = 1, Alias = "GrenadeHE", ItemType = ItemType.GrenadeHE, Price = 400, DisplayName = "HE Grenade" },
			new BuyableItem { Limit = 1, Alias = "GrenadeFlash", ItemType = ItemType.GrenadeFlash, Price = 300, DisplayName = "Flash Grenade" },
			new BuyableItem { Limit = 1, Alias = "Ammo44cal", ItemType = ItemType.Ammo44cal, Price = 60, DisplayName = ".44 Cal Ammo" },
			new BuyableItem { Limit = 1, Alias = "Ammo762x39", ItemType = ItemType.Ammo762x39, Price = 80, DisplayName = "7.62x39mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "Ammo9x19", ItemType = ItemType.Ammo9x19, Price = 40, DisplayName = "9x19mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunCOM18", ItemType = ItemType.GunCOM18, Price = 900, DisplayName = "COM-18 Pistol" },
			new BuyableItem { Limit = 1, Alias = "SCP018", ItemType = ItemType.SCP018, Price = 600, DisplayName = "SCP-018" },
			new BuyableItem { Limit = 1, Alias = "SCP268", ItemType = ItemType.SCP268, Price = 1000, DisplayName = "SCP-268" },
			new BuyableItem { Limit = 1, Alias = "Adrenaline", ItemType = ItemType.Adrenaline, Price = 150, DisplayName = "Adrenaline" },
			new BuyableItem { Limit = 1, Alias = "Painkillers", ItemType = ItemType.Painkillers, Price = 120, DisplayName = "Painkillers" },
			new BuyableItem { Limit = 1, Alias = "Coin", ItemType = ItemType.Coin, Price = 5, DisplayName = "Coin" },
			new BuyableItem { Limit = 1, Alias = "ArmorLight", ItemType = ItemType.ArmorLight, Price = 300, DisplayName = "Light Armor" },
			new BuyableItem { Limit = 1, Alias = "ArmorCombat", ItemType = ItemType.ArmorCombat, Price = 500, DisplayName = "Combat Armor" },
			new BuyableItem { Limit = 1, Alias = "ArmorHeavy", ItemType = ItemType.ArmorHeavy, Price = 700, DisplayName = "Heavy Armor" },
			new BuyableItem { Limit = 1, Alias = "GunRevolver", ItemType = ItemType.GunRevolver, Price = 1000, DisplayName = "Revolver" },
			new BuyableItem { Limit = 1, Alias = "GunAK", ItemType = ItemType.GunAK, Price = 1400, DisplayName = "AK" },
			new BuyableItem { Limit = 1, Alias = "GunShotgun", ItemType = ItemType.GunShotgun, Price = 1600, DisplayName = "Shotgun" },
			new BuyableItem { Limit = 1, Alias = "SCP330", ItemType = ItemType.SCP330, Price = 700, DisplayName = "SCP-330" },
			new BuyableItem { Limit = 1, Alias = "SCP2176", ItemType = ItemType.SCP2176, Price = 900, DisplayName = "SCP-2176" },
			new BuyableItem { Limit = 1, Alias = "SCP244a", ItemType = ItemType.SCP244a, Price = 1100, DisplayName = "SCP-244a" },
			new BuyableItem { Limit = 1, Alias = "SCP244b", ItemType = ItemType.SCP244b, Price = 1100, DisplayName = "SCP-244b" },
			new BuyableItem { Limit = 1, Alias = "SCP1853", ItemType = ItemType.SCP1853, Price = 800, DisplayName = "SCP-1853" },
			new BuyableItem { Limit = 1, Alias = "ParticleDisruptor", ItemType = ItemType.ParticleDisruptor, Price = 1600, DisplayName = "Particle Disruptor" },
			new BuyableItem { Limit = 1, Alias = "GunCom45", ItemType = ItemType.GunCom45, Price = 1000, DisplayName = "COM-45 Pistol" },
			new BuyableItem { Limit = 1, Alias = "SCP1576", ItemType = ItemType.SCP1576, Price = 1300, DisplayName = "SCP-1576" },
			new BuyableItem { Limit = 1, Alias = "Jailbird", ItemType = ItemType.Jailbird, Price = 500, DisplayName = "Jailbird" },
			new BuyableItem { Limit = 1, Alias = "AntiSCP207", ItemType = ItemType.AntiSCP207, Price = 300, DisplayName = "Anti-SCP-207" },
			new BuyableItem { Limit = 1, Alias = "GunFRMG0", ItemType = ItemType.GunFRMG0, Price = 1800, DisplayName = "FRMG-0" },
			new BuyableItem { Limit = 1, Alias = "GunA7", ItemType = ItemType.GunA7, Price = 1400, DisplayName = "A7" },
			new BuyableItem { Limit = 1, Alias = "Lantern", ItemType = ItemType.Lantern, Price = 200, DisplayName = "Lantern" },
		};
	}
}
