using Exiled.API.Features;
using LiteDB;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Classes;
using System;

namespace FoundationFortune.API.Database
{
    public static class PlayerDataRepository
    {
        private static LiteCollection<PlayerData> PlayersCollection => (LiteCollection<PlayerData>)FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
        public static void InsertPlayer(PlayerData player) => PlayersCollection.Insert(player);
        public static PlayerData GetPlayerById(string userId) => PlayersCollection.FindOne(p => p.UserId == userId);
        public static bool GetHintMinmode(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintMinmode ?? false;
        public static int GetMoneyOnHold(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneyOnHold ?? 0;
        public static int GetMoneySaved(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneySaved ?? 0;

        public static bool SetHintMinmode(string userId, bool enable)
        {
            var player = GetPlayerById(userId);
            if (player != null)
            {
                player.HintMinmode = enable;
                return PlayersCollection.Update(player);
            }
            return false;
        }

        public static HintAlign? GetUserHintAlign(string userId)
        {
            try
            {
                return PlayersCollection.FindOne(p => p.UserId == userId)?.HintAlign ?? HintAlign.Center;
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving hint alignment for user {userId}: {ex.Message}");
                return null;
            }
        }

        public static bool SetUserHintAlign(string userId, HintAlign hintAlign)
        {
            try
            {
                var player = GetPlayerById(userId);
                if (player != null)
                {
                    player.HintAlign = hintAlign;
                    return PlayersCollection.Update(player);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving hint alignment for user {userId}: {ex.Message}");
            }
            return false;
        }

        public static void ModifyMoney(string userId, int amount, bool subtract = false, bool hold = false, bool saved = true)
        {
            var player = GetPlayerById(userId);
            if (player != null)
            {
                if (hold)
                {
                    if (subtract) player.MoneyOnHold -= amount;
                    else player.MoneyOnHold += amount;
                }
                else if (saved)
                {
                    if (subtract) player.MoneySaved -= amount;
                    else player.MoneySaved += amount;
                }
                PlayersCollection.Update(player);
            }
        }


        public static void EmptyMoney(string userId, bool onHold = false, bool saved = false)
        {
            var player = GetPlayerById(userId);
            if (player != null)
            {
                if (onHold) player.MoneyOnHold = 0;
                if (saved) player.MoneySaved = 0;
                PlayersCollection.Update(player);
            }
        }

        public static void TransferMoney(string userId, bool onHoldToSaved = false)
        {
            var player = GetPlayerById(userId);
            if (player != null)
            {
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
}
