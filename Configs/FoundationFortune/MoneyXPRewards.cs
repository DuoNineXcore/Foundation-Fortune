using System.Collections.Generic;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs.FoundationFortune;

public class MoneyXPRewards : IFoundationFortuneConfig
{
    [YamlIgnore] public string PropertyName { get; set; } = "Server Event Settings";

    public bool KillEvent { get; set; } = true;
    public bool EscapeEvent { get; set; } = true;
    public bool RoundEndEvent { get; set; } = true;

    public bool XpSystem { get; set; } = false;
    
    public int KillEventMoneyRewards { get; set; } = 300;
    public int EscapeEventMoneyRewards { get; set; } = 300;
    public int ExtractionMoneyRewards { get; set; } = 300;

    public int ExtractionXpRewards { get; set; } = 300;
    public int EscapeEventXpRewards { get; set; } = 300;
    public int KillEventXpRewards { get; set; } = 300;
    public int BuyingItemXpRewards { get; set; } = 300;
    public int UsingPerkXpRewards { get; set; } = 300;
    public int BuyingPerkXpRewards { get; set; } = 500;
    public int SellingXpRewards { get; set; } = 300;
    
    public Dictionary<PlayerTeamConditions, int> RoundEndRewards { get; set; } = new()
    {
        { PlayerTeamConditions.Winning, 500 },
        { PlayerTeamConditions.Losing, 100 },
        { PlayerTeamConditions.Draw, 250 }
    };
    
    public int ExpToLevelUp { get; set; } = 50;
    public int LevelUntilPrestige { get; set; } = 20;
    
    public Dictionary<int, double> PrestigeLevelMultiplier { get; set; } = new()
    {
        { 0, 1.0 },
        { 1, 1.05 },
        { 2, 1.10 },
        { 3, 1.15 },
        { 4, 1.20 },
        { 5, 1.25 },
    };
}