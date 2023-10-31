using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using LiteDB;
using MEC;
using PlayerRoles;
using UnityEngine;
using VoiceChat;

namespace FoundationFortune.API.Models
{
    public class HintEntry
    {
        public string Text { get; set; }
        public float Timestamp { get; set; }
        public bool IsAnimated { get; set; }

        public HintEntry(string text, float timestamp, bool isAnimated)
        {
            Text = text;
            Timestamp = timestamp;
            IsAnimated = isAnimated;
        }
    }
    
    [Serializable]
    public class ExpectedResponse
    {
        [Serializable]
        public class Asset
        {
            public string url;
            public string name;
        }

        public Asset[] assets;
    }

    public class BotData
    {
        public Npc Bot { get; set; }
        public int Indexation { get; set; }
    }
    
    public class ExtractionTimerData
    {
        public CoroutineHandle CoroutineHandle { get; set; }
        public float StartTime { get; set; }
    }

    public class Bounty
    {
        public Player Player { get; }
        public bool IsBountied { get; set; }
        public int Value { get; }
        public DateTime ExpirationTime { get; }

        public Bounty(Player player, bool isBountied, int value, DateTime expirationTime)
        {
            Player = player;
            IsBountied = isBountied;
            Value = value;
            ExpirationTime = expirationTime;
        }
    }

    public class PlayerMusicBotPair
    {
        public Player Player { get;  }
        public Npc MusicBot { get; }

        public PlayerMusicBotPair(Player player, Npc musicBot)
        {
            Player = player;
            MusicBot = musicBot;
        }
    }

    public class ObjectInteractions
    {
        public Player Player;
        public Dictionary<BuyableItem, int> BoughtItems = new();
        public Dictionary<PerkItem, int> BoughtPerks = new();
        public Dictionary<SellableItem, int> SoldItems = new();

        public ObjectInteractions(Player player)
        {
            Player = player;
        }
    }

    public class BuyingBotSpawn
    {
        public string Name { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public RoleTypeId Role { get; set; }
        public ItemType HeldItem { get; set; }
        public Vector3 Scale { get; set; }
        public RoomType Room { get; set; }
    }

    public class SellingBotSpawn
    {
        public string Name { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public RoleTypeId Role { get; set; }
        public ItemType HeldItem { get; set; }
        public Vector3 Scale { get; set; }
        public RoomType Room { get; set; }
    }
    
    public class MusicBotSpawn
    {
        public string Name { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public RoleTypeId Role { get; set; }
        public ItemType HeldItem { get; set; }
        public Vector3 Scale { get; set; }
        public RoomType Room { get; set; }
    }

    public class BuyableItem
    {
        public ItemType ItemType { get; set; }
        public int Price { get; set; }
        public int Limit { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
    }

    public class NPCVoiceChatSettings
    {
        public VoiceChatChannel VoiceChat { get; set; }
        public byte Volume { get; set; }
        public string AudioFile { get; set; }
        public NPCVoiceChatUsageType VoiceChatUsageType { get; set; }
        public bool Loop { get; set; }
    }

    public class PlayerVoiceChatSettings
    {
        public VoiceChatChannel VoiceChat { get; set; }
        public byte Volume { get; set; }
        public string AudioFile { get; set; }
        public PlayerVoiceChatUsageType VoiceChatUsageType { get; set; }
        public bool Loop { get; set; }
    }

    public class SellableItem
    {
        public ItemType ItemType { get; set; }
        public int Price { get; set; }
        public int Limit { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
    }

    public class PerkItem
    {
        public PerkType PerkType { get; set; }
        public int Price { get; set; }
        public int Limit { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }

    public class PlayerData
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string UserId { get; set; }
        public int MoneyOnHold { get; set; }
        public int MoneySaved { get; set; }
        public int HintOpacity { get; set; }
        public int HintSize { get; set; }
        public int HintLimit { get; set; }
        public bool HintMinmode { get; set; }
        public bool HintSystem { get; set; }
        public bool HintAdmin { get; set; }
        public HintAlign HintAlign { get; set; }
        public HintAnim HintAnim { get; set; }
    }
}
