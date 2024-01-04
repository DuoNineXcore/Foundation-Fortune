using System;
using System.Collections.Generic;
using System.Linq;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Common.Models;
using FoundationFortune.API.Core.Common.Models.Databases;
using LiteDB;

namespace FoundationFortune.API.Core.Database;

public static class QuestRotationRepository
{
    private static LiteCollection<QuestRotationData> QuestRotationCollection => (LiteCollection<QuestRotationData>)FoundationFortune.Instance.QuestRotationDatabase.GetCollection<QuestRotationData>("QuestRotationData");
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
        var questRotationData = QuestRotationCollection.FindOne(q => q.UserId == userId) ?? ShuffleAndSaveQuests(userId);
        return questRotationData.Quests;
    }

    private static QuestRotationData ShuffleAndSaveQuests(string userId = null)
    {
        QuestType[] allQuestTypes = (QuestType[])Enum.GetValues(typeof(QuestType));
        List<QuestType> shuffledQuests = allQuestTypes.OrderBy(_ => UnityEngine.Random.Range(0f, 1f)).ToList();

        var questRotationData = new QuestRotationData
        {
            UserId = userId,
            Quests = shuffledQuests.Take(3).ToList()
        };

        QuestRotationCollection.Upsert(questRotationData);
        return questRotationData;
    }

    private static void SaveQuestRotations()
    {
        var questRotationInfo = new QuestRotationInfo
        {
            RotationCounter = RotationCounter,
            MaxRotations = MaxRotations
        };

        FoundationFortune.Instance.QuestRotationDatabase.GetCollection<QuestRotationInfo>("QuestRotationInfo").Upsert(questRotationInfo);
    }

    private static void LoadQuestRotations()
    {
        var questRotationInfoCollection = FoundationFortune.Instance.QuestRotationDatabase.GetCollection<QuestRotationInfo>("QuestRotationInfo");
        var questRotationInfo = questRotationInfoCollection.FindAll().FirstOrDefault();

        if (questRotationInfo == null) return;
        RotationCounter = questRotationInfo.RotationCounter;
        MaxRotations = questRotationInfo.MaxRotations;
    }
}

