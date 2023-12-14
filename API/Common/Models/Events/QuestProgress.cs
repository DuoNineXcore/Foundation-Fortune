using System;
using FoundationFortune.API.Common.Enums.Systems.QuestSystem;

namespace FoundationFortune.API.Common.Models.Events;

public class QuestProgress
{
    public QuestType QuestType { get; }
    public int Progress { get; private set; }
    public int MaxProgress { get; }

    public QuestProgress(QuestType questType)
    {
        QuestType = questType;
        Progress = 0;
        MaxProgress = GetMaxProgressForQuest(QuestType);
    }

    public string Alias => GetAlias();
    public bool IsCompleted => Progress >= GetMaxProgressForQuest(QuestType);
    public void UpdateProgress(int amount)
    {
        Progress += QuestType switch
        {
            QuestType.GetAKillstreak => amount,
            QuestType.KillZombies => amount,
            QuestType.UnlockGenerators => amount,
            QuestType.BuyItems => amount,
            QuestType.UseEtherealIntervention => amount,
            QuestType.CollectMoneyFromDeathCoins => amount,
            QuestType.ThrowGhostlights => amount,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (Progress > GetMaxProgressForQuest(QuestType)) Progress = GetMaxProgressForQuest(QuestType);
    }

    private static int GetMaxProgressForQuest(QuestType questType)
    {
        return questType switch
        {
            QuestType.GetAKillstreak => 20,
            QuestType.KillZombies => 10,
            QuestType.UnlockGenerators => 10,
            QuestType.BuyItems => 5,
            QuestType.UseEtherealIntervention => 3,
            QuestType.CollectMoneyFromDeathCoins => 2500,
            QuestType.ThrowGhostlights => 5,
            _ => int.MaxValue
        };
    }
    
    private string GetAlias()
    {
        return QuestType switch
        {
            QuestType.GetAKillstreak => "Kill People",
            QuestType.KillZombies => "Kill Zombies",
            QuestType.UnlockGenerators => "Unlock Generators",
            QuestType.BuyItems => "Buy Items",
            QuestType.UseEtherealIntervention => "Use Ethereal Intervention",
            QuestType.CollectMoneyFromDeathCoins => "Collect Money from Death Coins",
            QuestType.ThrowGhostlights => "Throw Ghostlights",
            _ => ""
        };
    }
}