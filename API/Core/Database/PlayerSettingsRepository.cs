using System;
using Discord;
using FoundationFortune.API.Core.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Core.Common.Models.Databases;
using LiteDB;

namespace FoundationFortune.API.Core.Database;

public static class PlayerSettingsRepository
{
    private static LiteCollection<PlayerSettings> PlayersCollection => (LiteCollection<PlayerSettings>)FoundationFortune.Instance.PlayerSettingsDatabase.GetCollection<PlayerSettings>("PlayerSettings");
    public static PlayerSettings GetPlayerById(string userId) => PlayersCollection.FindOne(p => p.UserId == userId);
    public static void InsertPlayer(PlayerSettings player) => PlayersCollection.Insert(player);

    //player hint getter methods
    public static bool GetHintMinmode(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintMinmode ?? false;
    public static bool GetHintDisable(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSystem ?? false;
    public static bool GetPluginAdmin(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAdmin ?? false;
    public static bool GetHintExtension(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintExtension ?? false;
    public static int GetHintLimit(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintLimit ?? 5;
    public static int GetHintSize(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSize ?? 25;
    public static float GetHintAgeSeconds(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAgeSeconds ?? 5;
    public static HintAlign GetHintAlign(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAlign ?? HintAlign.Center;
    public static int GetSellingConfirmationTime(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.SellingConfirmationTime ?? 5;
    public static int GetActiveAbilityActivationTime(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.ActiveAbilityActivationTime ?? 5;
    
    //player hint setter methods
    public static bool SetHintMinmode(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintMinmode = v, enable);
    public static bool SetHintExtension(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintExtension = v, enable);
    public static void SetHintLimit(string userId, int hintLimit) => UpdatePlayerProperty(userId, (p, v) => p.HintLimit = v, hintLimit);
    public static void SetHintAgeSeconds(string userId, float hintSeconds) => UpdatePlayerProperty(userId, (p, v) => p.HintAgeSeconds = v, hintSeconds);
    public static void SetHintSize(string userId, int hintSize) => UpdatePlayerProperty(userId, (p, v) => p.HintSize = v, hintSize);
    public static bool SetHintDisable(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintSystem = v, enable);
    public static void SetHintAdmin(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintAdmin = v, enable);
    public static void SetHintAlign(string userId, HintAlign hintAlign) => UpdatePlayerProperty(userId, (p, v) => p.HintAlign = v, hintAlign);
    public static void SetSellingConfirmationTime(string userId, int secs) => UpdatePlayerProperty(userId, (p, v) => p.SellingConfirmationTime = v, secs);
    public static void SetActiveAbilityConfirmationTime(string userId, int secs) => UpdatePlayerProperty(userId, (p, v) => p.ActiveAbilityActivationTime = v, secs);
    
    private static bool UpdatePlayerProperty<T>(string userId, Action<PlayerSettings, T> propertyUpdater, T value)
    {
        try
        {
            var playerData = GetPlayerById(userId);
            if (playerData == null) return false;
            propertyUpdater(playerData, value);
            PlayersCollection.Update(playerData);
            return true;
        }
        catch (Exception ex)
        {
            DirectoryIterator.Log(ex.ToString(), LogLevel.Error);
            return false;
        }
    }
}