using System;
using Exiled.API.Features;

namespace FoundationFortune.API.Core.Common.Interfaces.Perks;

public interface ICooldownActivePerk : IPassivePerk
{
    void StartActivePerkAbility(Player player);
    TimeSpan Cooldown { get; } 
    TimeSpan Duration { get; } 
    DateTime LastActivationTime { get; }
    bool IsOnCooldown { get; }
    bool IsCurrentlyActive { get; }
    TimeSpan GetRemainingCooldown();
}