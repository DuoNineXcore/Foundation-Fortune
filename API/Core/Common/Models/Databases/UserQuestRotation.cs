using System.Collections.Generic;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using LiteDB;

namespace FoundationFortune.API.Core.Common.Models.Databases;

public class QuestRotationData : IFoundationFortuneDatabase
{
    public ObjectId Id { get; set; }
    public string UserId { get; set; }
    public List<QuestType> Quests { get; set; }
}