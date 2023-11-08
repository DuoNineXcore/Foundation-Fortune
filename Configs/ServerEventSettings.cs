using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Models.Enums.Player;
using FoundationFortune.API.Models.Interfaces;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs;

public class ServerEventSettings : IFoundationFortuneConfig
{
    [YamlIgnore] public string PropertyName { get; set; } = "Server Event Settings";

    public bool KillEvent { get; set; } = true;
    public bool EscapeEvent { get; set; } = true;
    public bool RoundEndEvent { get; set; } = true;
    public bool KillEventRewardsOnlySCPS { get; set; } = false;
    public int KillEventRewards { get; set; } = 300;
    public int EscapeRewards { get; set; } = 300;
    public float MaxHintAge { get; set; } = 3f;
    public Dictionary<PlayerTeamConditions, int> RoundEndRewards { get; set; } = new Dictionary<PlayerTeamConditions, int>
    {
        { PlayerTeamConditions.Winning, 500 },
        { PlayerTeamConditions.Losing, 100 },
        { PlayerTeamConditions.Draw, 250 }
    };
}