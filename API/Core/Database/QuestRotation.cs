using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FoundationFortune.API.Common.Enums.Systems.QuestSystem;

namespace FoundationFortune.API.Core.Database;

public static class QuestRotation
{
    private static readonly Dictionary<string, List<QuestType>> UserQuestRotations = new Dictionary<string, List<QuestType>>();
    private static readonly string RotationFilePath = Path.Combine(DirectoryIterator.DatabaseDirectoryPath, "QuestRotations.txt");
    public static int RotationCounter;
    public static int MaxRotations = 3;

    public static void IncrementQuestRotationNumber()
    {
        RotationCounter++;
        SaveQuestRotations();
        if (RotationCounter < MaxRotations) return;
        ShuffleAndSaveQuests();
        RotationCounter = 0;
    }

    public static List<QuestType> GetShuffledQuestsForUser(string userId)
    {
        if (!UserQuestRotations.TryGetValue(userId, out var user)) user = ShuffleAndSaveQuests(userId);
        return user;
    }

    private static List<QuestType> ShuffleAndSaveQuests(string userId = null)
    {
        QuestType[] allQuestTypes = (QuestType[])Enum.GetValues(typeof(QuestType));
        List<QuestType> shuffledQuests = allQuestTypes.OrderBy(x => UnityEngine.Random.Range(0f, 1f)).ToList();

        if (userId != null) UserQuestRotations[userId] = shuffledQuests.Take(3).ToList();
        else foreach (var user in UserQuestRotations.Keys.ToList()) UserQuestRotations[user] = shuffledQuests.Take(3).ToList();
        SaveQuestRotations();

        return userId != null ? UserQuestRotations[userId] : null;
    }

    private static void SaveQuestRotations()
    {
        using StreamWriter writer = new StreamWriter(RotationFilePath);
        writer.WriteLine($"Quest Rotation: {RotationCounter} / {MaxRotations}");
        foreach (var line in UserQuestRotations.Select(userRotation => $"{userRotation.Key}: {string.Join(",", userRotation.Value)}")) writer.WriteLine(line);
    }

    private static void LoadQuestRotations()
    {
        if (!File.Exists(RotationFilePath)) DirectoryIterator.CreateFile(RotationFilePath);
        
        foreach (string line in File.ReadLines(RotationFilePath))
        {
            if (line.StartsWith("Quest Rotation:"))
            {
                string[] parts = line.Split('/');
                if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int max)) MaxRotations = max;
            }
            else
            {
                string[] parts = line.Split(':');
                if (parts.Length != 2) continue;
                string userId = parts[0].Trim();
                string[] questTypeStrings = parts[1].Split(',');
                List<QuestType> quests = questTypeStrings.Select(q => (QuestType)Enum.Parse(typeof(QuestType), q.Trim())).ToList();
                UserQuestRotations[userId] = quests;
            }
        }
    }
}