using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Models.Enums.Player;
using FoundationFortune.API.Models.Interfaces;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs;

public class MoneyXPRewards : IFoundationFortuneConfig
{
    [YamlIgnore] public string PropertyName { get; set; } = "Server Event Settings";

    public bool KillEvent { get; set; } = true;
    public bool EscapeEvent { get; set; } = true;
    public bool RoundEndEvent { get; set; } = true;
    
    public int KillEventMoneyRewards { get; set; } = 300;
    public int EscapeEventMoneyRewards { get; set; } = 300;
    public int ExtractionMoneyRewards { get; set; } = 300;

    public int ExtractionXPRewards { get; set; } = 300;
    public int EscapeEventXPRewards { get; set; } = 300;
    public int KillEventXPRewards { get; set; } = 300;
    public int BuyingItemXPRewards { get; set; } = 300;
    public int UsingPerkXPRewards { get; set; } = 300;
    public int BuyingPerkXPRewards { get; set; } = 500;
    public int SellingXPRewards { get; set; } = 300;
    
    public Dictionary<PlayerTeamConditions, int> RoundEndRewards { get; set; } = new Dictionary<PlayerTeamConditions, int>
    {
        { PlayerTeamConditions.Winning, 500 },
        { PlayerTeamConditions.Losing, 100 },
        { PlayerTeamConditions.Draw, 250 }
    };
    
    public int EXPToLevelUp { get; set; } = 50;
    public int LevelUntilPrestige { get; set; } = 20;
    
    public Dictionary<int, double> PrestigeLevelMultiplier { get; set; } = new Dictionary<int, double>
    {
        { 0, 1.0 },
        { 1, 1.05 },
        { 2, 1.10 },
        { 3, 1.15 },
        { 4, 1.20 },
        { 5, 1.25 },
    };
}