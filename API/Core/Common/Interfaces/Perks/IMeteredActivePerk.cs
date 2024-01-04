using System;
using Exiled.API.Features;

namespace FoundationFortune.API.Core.Common.Interfaces.Perks;

public interface IMeteredActivePerk : IPassivePerk
{
    DateTime LastActivationTime { get; }
    TimeSpan Cooldown { get; }
    bool IsOnCooldown { get; }
    bool IsCurrentlyActive { get; }
    TimeSpan GetRemainingCooldown();
    float CurrentMeterValue { get; }
    float MaxMeterValue { get; }
    float MeterDepletionRate { get; }
    bool IsMeterFull { get; }
    void FillMeter(float amount);
    void StartActivePerkAbility(Player player);
}
