using System.Collections.Generic;
using System.Text;
using Discord;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Common.Models;

namespace FoundationFortune.API.Core.Systems;

public static class QuestSystem
{
    private static readonly Dictionary<string, QuestProgress> ActiveQuests = new();

    public static void EnableQuest(string userId, QuestType questType)
    {
        if (ActiveQuests.ContainsKey(userId)) return;
        var questProgress = new QuestProgress(questType);
        ActiveQuests.Add(userId, questProgress);
        DirectoryIterator.Log($"Quest {questType} enabled for user {userId}.", LogLevel.Debug);
    }
    
    public static bool IsQuestActive(string userId, QuestType questType)
    {
        if (ActiveQuests.TryGetValue(userId, out var questProgress)) return questProgress.QuestType == questType;
        return false;
    }
    
    public static bool IsQuestActive(string userId) => ActiveQuests.ContainsKey(userId);

    private static void TrackQuestProgress(string userId, int amount)
    {
        if (!ActiveQuests.ContainsKey(userId)) return;
        var questProgress = ActiveQuests[userId];
        questProgress.UpdateProgress(amount);
        if (!questProgress.IsCompleted) return;
        DirectoryIterator.Log($"User {userId} completed quest: {questProgress.QuestType}", LogLevel.Debug);
        DisableQuest(userId);
    }

    public static void UpdateQuestProgress(Player player, QuestType questType, int amount)
    {
        if (player != null && IsQuestActive(player.UserId, questType)) TrackQuestProgress(player.UserId, amount);
    }
    
    public static void UpdateQuestMessages(Player player, ref StringBuilder indicatorBuilder)
    {
        if (!ActiveQuests.ContainsKey(player.UserId)) return;
        var questProgress = ActiveQuests[player.UserId];
        indicatorBuilder.Append($"<b>{questProgress.Alias} {questProgress.Progress}/{questProgress.MaxProgress}</b>");
    }

    private static void DisableQuest(string userId)
    {
        if (!ActiveQuests.ContainsKey(userId)) return;
        ActiveQuests.Remove(userId);
        DirectoryIterator.Log($"Quest disabled for user {userId}.", LogLevel.Debug);
    }
}