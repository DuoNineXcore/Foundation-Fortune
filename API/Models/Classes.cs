using Exiled.API.Enums;
using LiteDB;
using PlayerRoles;
using UnityEngine;
using VoiceChat;
using Exiled.API.Features;
using FoundationFortune.API.Models.Enums;
using System;
using System.Collections.Generic;
using MEC;

namespace FoundationFortune.API.Models.Classes
{
    public class HintEntry
    {
        public string Text { get; }
        public float Timestamp { get; }
        public int Reward { get; }

        public HintEntry(string text, float timestamp, int reward)
        {
            Text = text;
            Timestamp = timestamp;
            Reward = reward;
        }
    }

    public class ExtractionTimerData
    {
        public CoroutineHandle CoroutineHandle { get; set; }
        public float StartTime { get; set; }
    }

    public class BuyingBotComponent : MonoBehaviour
    {
        internal Npc BuyingBotNPC;
        internal Player ply;
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

    public class NPCSpawn
    {
        public string Name { get; set; }
        public string Badge { get; set; }
        public string BadgeColor { get; set; }
        public bool IsSellingBot { get; set; }
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


    public class VoiceChatSettings
    {
        public VoiceChatChannel VoiceChat { get; set; }
        public byte Volume { get; set; }
        public string AudioFile { get; set; }
        public VoiceChatUsageType VoiceChatUsageType { get; set; }
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
        public bool HintMinmode { get; set; }
        public HintAlign HintAlign { get; set; }
    }
}
