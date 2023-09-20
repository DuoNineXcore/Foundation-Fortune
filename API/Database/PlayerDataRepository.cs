using Exiled.API.Features;
using FoundationFortune.Events;
using LiteDB;
using System;

namespace FoundationFortune.API.Database
{
    public static class PlayerDataRepository
    {
        public static void InsertPlayer(PlayerData player)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            playersCollection.Insert(player);
        }

        public static PlayerData GetPlayerById(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            return playersCollection.FindOne(p => p.UserId == userId);
        }

        public static bool GetHintMinmode(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            return player?.HintMinmode ?? false;
        }

        public static bool SetHintMinmode(string userId, bool enable)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.HintMinmode = enable;
                playersCollection.Update(player);
                return true;
            }

            return false;
        }

        public static HintAlign? GetUserHintAlign(string userId)
        {
            try
            {
                var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
                var player = playersCollection.FindOne(p => p.UserId == userId);
                return player?.HintAlign ?? HintAlign.Center;
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
                var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
                var player = playersCollection.FindOne(p => p.UserId == userId);

                if (player != null)
                {
                    player.HintAlign = hintAlign;
                    return playersCollection.Update(player);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error retrieving hint alignment for user {userId}: {ex.Message}");
                return false;
            }
            return false;
        }

        public static void SetMoneySaved(string userId, int amount)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneySaved = amount;
                playersCollection.Update(player);
            }
        }

        public static void AddMoneyOnHold(string userId, int amount)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneyOnHold += amount;
                playersCollection.Update(player);
            }
        }

        public static void AddMoneySaved(string userId, int amount)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneySaved += amount;
                playersCollection.Update(player);
            }
        }

        public static int GetMoneyOnHold(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            return player?.MoneyOnHold ?? 0;
        }

        public static int GetMoneySaved(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            return player?.MoneySaved ?? 0;
        }

        public static void TransferAllMoneyToSaved(string userId)
        {
            int moneyOnHold = GetMoneyOnHold(userId);
            int moneySaved = GetMoneySaved(userId);

            moneySaved += moneyOnHold;

            SetMoneySaved(userId, moneySaved);
        }

        public static void SubtractMoneySaved(string userId, int amount)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneySaved -= amount;
                playersCollection.Update(player);
            }
        }

        public static void SubtractMoneyOnHold(string userId, int amount)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneyOnHold -= amount;
                playersCollection.Update(player);
            }
        }

        public static void EmptyMoneyOnHold(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneyOnHold = 0;
                playersCollection.Update(player);
            }
        }

        public static void EmptyMoneySaved(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneySaved = 0;
                playersCollection.Update(player);
            }
        }

        public static void TransferMoneyOnHoldToSaved(string userId)
        {
            var playersCollection = FoundationFortune.Singleton.db.GetCollection<PlayerData>("players");
            var player = playersCollection.FindOne(p => p.UserId == userId);

            if (player != null)
            {
                player.MoneySaved += player.MoneyOnHold;
                player.MoneyOnHold = 0;
                playersCollection.Update(player);
            }
        }
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
