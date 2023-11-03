using Exiled.API.Features;
using LiteDB;
using System;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums;

namespace FoundationFortune.API.Database
{
    public static class PlayerDataRepository
    {
        //player register
        private static LiteCollection<PlayerData> PlayersCollection => (LiteCollection<PlayerData>)FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
        public static PlayerData GetPlayerById(string userId) => PlayersCollection.FindOne(p => p.UserId == userId);
        public static void InsertPlayer(PlayerData player) => PlayersCollection.Insert(player);

        //player getter methods
        public static bool GetHintMinmode(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintMinmode ?? false;
        public static bool GetHintDisable(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSystem ?? false;
        public static bool GetPluginAdmin(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAdmin ?? false;
        public static int GetHintLimit(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintLimit ?? 5;
        public static int GetHintSize(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSize ?? 25;
        public static HintAnim GetHintAnim(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAnim ?? HintAnim.None;
        public static HintAlign GetHintAlign(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAlign ?? HintAlign.Center;

        //player setter methods
        public static bool ToggleHintMinmode(string userId, bool enable) => UpdatePlayerProperty(userId, p => p.HintMinmode, (p, v) => p.HintMinmode = v, enable);
        public static void SetHintLimit(string userId, int hintLimit) => UpdatePlayerProperty(userId, p => p.HintLimit, (p, v) => p.HintLimit = v, hintLimit);
        public static void SetHintSize(string userId, int hintSize) => UpdatePlayerProperty(userId, p => p.HintSize, (p, v) => p.HintSize = v, hintSize);
        public static bool SetHintAnim(string userId, HintAnim hintAnim) => UpdatePlayerProperty(userId, p => p.HintAnim, (p, v) => p.HintAnim = v, hintAnim);
        public static bool ToggleHintDisable(string userId, bool enable) => UpdatePlayerProperty(userId, p => p.HintAdmin, (p, v) => p.HintAdmin = v, enable);
        public static bool TogglePluginAdmin(string userId, bool enable) => UpdatePlayerProperty(userId, p => p.HintAdmin, (p, v) => p.HintAdmin = v, enable);
        public static bool SetUserHintAlign(string userId, HintAlign hintAlign) => UpdatePlayerProperty(userId, p => p.HintAlign, (p, v) => p.HintAlign = v, hintAlign);

        //get riches
        public static int GetMoneyOnHold(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneyOnHold ?? 0;
        public static int GetMoneySaved(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneySaved ?? 0;
        
        public static bool UpdatePlayerProperty<T>(string userId, Func<PlayerData, T> propertySelector, Action<PlayerData, T> propertyUpdater, T value)
        {
            var playerData = GetPlayerById(userId);
            if (playerData != null)
            {
                propertyUpdater(playerData, value);
                PlayersCollection.Update(playerData);
                return true;
            }
            return false;
        }

        public static void ModifyMoney(string userId, int amount, bool subtract = false, bool hold = false, bool saved = true)
        {
            var player = GetPlayerById(userId);
            if (player == null) return;
            if (hold)
            {
                if (subtract) player.MoneyOnHold -= amount;
                else player.MoneyOnHold += amount;
            }
            if (saved)
            {
                if (subtract) player.MoneySaved -= amount;
                else player.MoneySaved += amount;
            }
            PlayersCollection.Update(player);
        }

        public static void EmptyMoney(string userId, bool onHold = false, bool saved = false)
        {
            var player = GetPlayerById(userId);
            if (player == null) return;
            if (onHold) player.MoneyOnHold = 0;
            if (saved) player.MoneySaved = 0;
            PlayersCollection.Update(player);
        }

        public static void TransferMoney(string userId, bool onHoldToSaved = false)
        {
            var player = GetPlayerById(userId);
            if (player == null) return;
            if (onHoldToSaved)
            {
                player.MoneySaved += player.MoneyOnHold;
                player.MoneyOnHold = 0;
            }
            else
            {
                player.MoneyOnHold += player.MoneySaved;
                player.MoneySaved = 0;
            }
            PlayersCollection.Update(player);
        }
    }
}
