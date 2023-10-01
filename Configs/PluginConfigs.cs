using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;
using System.ComponentModel;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using VoiceChatSettings = FoundationFortune.API.Models.Classes.VoiceChatSettings;
using System;

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
        public bool KillRewardTransfer { get; set; } = false;
        public bool KillRewardTransferAll { get; set; } = false;
        public bool KillRewardScpOnly { get; set; } = false;

        [Description("Revival Settings.")]
        public bool HuntReviver { get; set; } = true;
        public int RevivedPlayerHealth { get; set; } = 30;
        public bool ResetRevivedInventory { get; set; } = false;
        public int RevivalBountyKillReward { get; set; } = 5000;
        public int RevivalBountyTimeSeconds { get; set; } = 300;
        public int RevivalBountyKillRewardTransfer { get; set; }
        public int RevivalBountyKillRewardTransferAll { get; set; }

        [Description("Escaping player event.")]
        public int EscapeReward { get; set; } = 300;
        public bool EscapeRewardTransfer { get; set; } = true;
        public bool EscapeRewardTransferAll { get; set; } = false;

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

        [Description("How Should the bots use their voice chat feature?")]
        public List<VoiceChatSettings> VoiceChatSettings { get; set; } = new List<VoiceChatSettings>()
        {
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.Selling, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.Buying, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.Revival, VoiceChat = VoiceChatChannel.Intercom, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.WrongBuyingBot, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.NotEnoughMoney, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.BuyingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50},
            new VoiceChatSettings { VoiceChatUsageType = VoiceChatUsageType.SellingBotInRange, VoiceChat = VoiceChatChannel.Mimicry, Loop = false, AudioFile = "BuySuccess.ogg", Volume = 50}
        };

        [Description("List of items that can be sold.")]
        public List<SellableItem> SellableItems { get; set; } = new List<SellableItem>
        {
            new SellableItem { Limit = 1, ItemType = ItemType.MicroHID, Price = 1200, DisplayName = "Micro HID" },
            new SellableItem { Limit = 1, ItemType = ItemType.SCP500, Price = 1000, DisplayName = "SCP-500" },
        };

        [Description("List of items that can be bought.")]
        public List<BuyableItem>BuyableItems { get; set; } = new List<BuyableItem>
        {
            new BuyableItem { Limit = 1, Alias = "Micro", ItemType = ItemType.MicroHID, Price = 500, DisplayName = "Micro HID" },
            new BuyableItem { Limit = 1, Alias = "500", ItemType = ItemType.SCP500, Price = 1000, DisplayName = "SCP-500" },
        };

        [Description("List of perks that can be bought.")]
        public List<PerkItem> PerkItems { get; set; } = new List<PerkItem>
        {
            new PerkItem { Limit = 1, Alias = "Rev", PerkType = PerkType.Revival, Price = 3000, DisplayName = "Revival", Description = "This is just warzone 2."},
            new PerkItem { Limit = 1, Alias = "EHP", PerkType = PerkType.ExtraHP, Price = 2200, DisplayName = "Extra HP", Description = "This is just, idk."},
            new PerkItem { Limit = 1, Alias = "AHP", PerkType = PerkType.AHPBoost, Price = 1100, DisplayName = "AHP Boost", Description = "This is just SCP Health scaling on lower values."},
            new PerkItem { Limit = 1, Alias = "Invis", PerkType = PerkType.Invisibility, Price = 1000, DisplayName = "Invisibility", Description = "This is just 268."},
            new PerkItem { Limit = 1, Alias = "Regen", PerkType = PerkType.Regeneration, Price = 500, DisplayName = "Regeneration", Description = "This is just Red Candy."},
            new PerkItem { Limit = 1, Alias = "Move", PerkType = PerkType.MovementBoost, Price = 300, DisplayName = "Movement Boost", Description = "This is just 207."},
        };
    }
}
