using System;
using System.Linq;
using FoundationFortune.API.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Common.Models.Player;
using LiteDB;

namespace FoundationFortune.API.Core.Database;

/// <summary>
/// rain world
/// </summary>
public static class PlayerDataRepository
{
    //player register
    private static LiteCollection<PlayerData> PlayersCollection => (LiteCollection<PlayerData>)FoundationFortune.Instance.DB.GetCollection<PlayerData>("players");
    public static PlayerData GetPlayerById(string userId) => PlayersCollection.FindOne(p => p.UserId == userId);
    public static void InsertPlayer(PlayerData player) => PlayersCollection.Insert(player);

    //player hint getter methods
    public static bool GetHintMinmode(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintMinmode ?? false;
    public static bool GetHintDisable(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSystem ?? false;
    public static bool GetPluginAdmin(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAdmin ?? false;
    public static bool GetHintExtension(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintExtension ?? false;
    public static int GetHintLimit(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintLimit ?? 5;
    public static int GetHintSize(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintSize ?? 25;
    public static float GetHintAgeSeconds(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAgeSeconds ?? 5;
    //public static HintAnim GetHintAnim(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAnim ?? HintAnim.None;
    public static HintAlign GetHintAlign(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.HintAlign ?? HintAlign.Center;

    //get player stats
    public static int GetExperience(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.Exp ?? 0;
    public static int GetLevel(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.Level ?? 0;
    public static int GetPrestigeLevel(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.PrestigeLevel ?? 0;
    public static int GetMoneyOnHold(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneyOnHold ?? 0;
    public static int GetMoneySaved(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.MoneySaved ?? 0;
    public static int GetSellingConfirmationTime(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.SellingConfirmationTime ?? 5;
    public static int GetActiveAbilityActivationTime(string userId) => PlayersCollection.FindOne(p => p.UserId == userId)?.ActiveAbilityActivationTime ?? 5;
    
    //setter methods
    public static bool ToggleHintMinmode(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintMinmode = v, enable);
    public static bool ToggleHintExtension(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintExtension = v, enable);
    public static void SetHintLimit(string userId, int hintLimit) => UpdatePlayerProperty(userId, (p, v) => p.HintLimit = v, hintLimit);
    public static void SetHintAgeSeconds(string userId, int hintSeconds) => UpdatePlayerProperty(userId, (p, v) => p.HintLimit = v, hintSeconds);
    public static void SetHintSize(string userId, int hintSize) => UpdatePlayerProperty(userId, (p, v) => p.HintSize = v, hintSize);
    //public static bool SetHintAnim(string userId, HintAnim hintAnim) => UpdatePlayerProperty(userId, (p, v) => p.HintAnim = v, hintAnim);
    public static bool ToggleHintDisable(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintAdmin = v, enable);
    public static void TogglePluginAdmin(string userId, bool enable) => UpdatePlayerProperty(userId, (p, v) => p.HintAdmin = v, enable);
    public static void SetUserHintAlign(string userId, HintAlign hintAlign) => UpdatePlayerProperty(userId, (p, v) => p.HintAlign = v, hintAlign);
    public static void SetSellingConfirmationTime(string userId, int secs) => UpdatePlayerProperty(userId, (p, v) => p.SellingConfirmationTime = v, secs);
    public static void SetActiveAbilityConfirmationTime(string userId, int secs) => UpdatePlayerProperty(userId, (p, v) => p.ActiveAbilityActivationTime = v, secs);

    private static bool UpdatePlayerProperty<T>(string userId, Action<PlayerData, T> propertyUpdater, T value)
    {
        var playerData = GetPlayerById(userId);
        if (playerData == null) return false;
        propertyUpdater(playerData, value);
        PlayersCollection.Update(playerData);
        return true;
    }

    /// <summary>
    /// Modifies the money of a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="amount">The amount of money to modify.</param>
    /// <param name="subtract">If true, subtracts the amount; otherwise, adds the amount.</param>
    /// <param name="hold">If true, modifies the money on hold; otherwise, modifies the saved money.</param>
    /// <param name="saved">If true, modifies the saved money; otherwise, modifies the money on hold.</param>
    /// <param name="multiplier">Optional multiplier to apply to the amount when adding money (default is 1.0).</param>
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

    /// <summary>
    /// Levels up a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="prestige">If true, increments the prestige level and resets experience and level; otherwise, increments the level and resets experience.</param>
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

    /// <summary>
    /// Sets the experience for a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="expAmount">The amount of experience to set.</param>
    /// <returns>The adjusted experience amount considering the player's prestige level.</returns>
    public static void SetExperience(string userId, int expAmount)
    {
        var player = GetPlayerById(userId);
        if (player == null) return;

        double prestigeMultiplier = GetPrestigeMultiplier(userId);
        int adjustedExp = (int)(expAmount * prestigeMultiplier);
        player.Exp += adjustedExp;

        PlayersCollection.Update(player);
    }

    /// <summary>
    /// Gets the prestige multiplier for a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <returns>The prestige multiplier for the player.</returns>
    public static double GetPrestigeMultiplier(string userId)
    {
        int prestigeLevel = GetPrestigeLevel(userId);
        if (FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier.TryGetValue(prestigeLevel, out var multiplier)) return multiplier;
        int maxPrestigeLevel = FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier.Keys.Max();
        return FoundationFortune.MoneyXPRewards.PrestigeLevelMultiplier[maxPrestigeLevel];
    }

    /// <summary>
    /// Empties the money (on hold and/or saved) for a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="onHold">If true, empties the money on hold.</param>
    /// <param name="saved">If true, empties the saved money.</param>
    public static void EmptyMoney(string userId, bool onHold = false, bool saved = false)
    {
        var player = GetPlayerById(userId);
        if (player == null) return;
        if (onHold) player.MoneyOnHold = 0;
        if (saved) player.MoneySaved = 0;
        PlayersCollection.Update(player);
    }

    /// <summary>
    /// Transfers money between on-hold and saved balances for a player identified by their user ID.
    /// </summary>
    /// <param name="userId">The user ID of the player.</param>
    /// <param name="onHoldToSaved">If true, transfers money from on hold to saved; otherwise, transfers money from saved to on hold.</param>
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