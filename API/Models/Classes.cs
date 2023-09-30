using Exiled.API.Enums;
using LiteDB;
using PlayerRoles;
using UnityEngine;
using VoiceChat;

namespace FoundationFortune.API.Models
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
