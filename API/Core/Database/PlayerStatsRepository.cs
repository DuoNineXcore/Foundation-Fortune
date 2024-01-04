using System;
using System.Linq;
using FoundationFortune.API.Core.Common.Models.Databases;
using LiteDB;

namespace FoundationFortune.API.Core.Database;

public static class PlayerStatsRepository
{
    private static LiteCollection<PlayerStats> PlayersCollection => (LiteCollection<PlayerStats>)FoundationFortune.Instance.PlayerStatsDatabase.GetCollection<PlayerStats>("PlayerStats");
    public static PlayerStats GetPlayerById(string userId) => PlayersCollection.FindOne(p => p.UserId == userId);
    public static void InsertPlayer(PlayerStats player) => PlayersCollection.Insert(player);
    
    public static int GetExperience(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.Exp ?? 0;
    public static int GetLevel(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.Level ?? 0;
    public static int GetPrestigeLevel(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.PrestigeLevel ?? 0;
    public static int GetMoneyOnHold(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneyOnHold ?? 0;
    public static int GetMoneySaved(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneySaved ?? 0;
    
    public static void ModifyMoney(string userId, int amount, bool subtract = false, bool hold = false, bool saved = true, double multiplier = 1.0)
    {
        var player = GetPlayerById(userId);
        if (player == null) return;

        if (!subtract && multiplier > 1.0) amount = (int)Math.Round(amount * multiplier);
        
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

    public static void LevelUp(string userId, bool prestige)
    {
        var player = GetPlayerById(userId);
        if (player == null) return;

        if (prestige)
        {
            player.PrestigeLevel++;
            player.Exp = 0;
            player.Level = 0;
        }
        else
        {
            player.Exp = 0;
            player.Level++;
        }

        PlayersCollection.Update(player);
    }

    public static void SetExperience(string userId, int expAmount)
    {
        var player = GetPlayerById(userId);
        if (player == null) return;

        double prestigeMultiplier = GetPrestigeMultiplier(userId);
        int adjustedExp = (int)(expAmount * prestigeMultiplier);
        player.Exp += adjustedExp;

        PlayersCollection.Update(player);
    }
    
    public static double GetPrestigeMultiplier(string userId)
    {
        int prestigeLevel = GetPrestigeLevel(userId);
        if (FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier.TryGetValue(prestigeLevel, out var multiplier)) return multiplier;
        int maxPrestigeLevel = FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier.Keys.Max();
        return FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier[maxPrestigeLevel];
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