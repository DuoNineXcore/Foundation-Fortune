using System.Collections.Generic;
using Exiled.API.Enums;
using FoundationFortune.API.Core.Models.Interfaces;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs;

public class MoneyExtractionSystemSettings : IFoundationFortuneConfig
{
    [YamlIgnore] public string PropertyName { get; set; } = "Money Extraction System Settings";
    public bool MoneyExtractionSystem { get; set; } = false;
    public List<RoomType> ExtractionPointRooms { get; set; } = new List<RoomType>
    {
        RoomType.LczToilets,
        RoomType.Lcz914,
        RoomType.HczHid,
        RoomType.HczNuke,
    };
    public int ExtractionLimit { get; set; } = 5;
    public int MinExtractionPointGenerationTime { get; set; } = 15;
    public int MaxExtractionPointGenerationTime { get; set; } = 30;
    public int ExtractionPointDuration { get; set; } = 120;
}