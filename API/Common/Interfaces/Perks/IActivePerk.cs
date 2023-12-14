using Exiled.API.Features;

namespace FoundationFortune.API.Common.Interfaces.Perks;

public interface IActivePerk : IPassivePerk
{
    void StartActivePerkAbility(Player player);
    int TimeUntilNextActivation { get; }
    bool IsOnCooldown { get; }
    bool isCurrentlyActive { get; }
    float GetRemainingCooldown();
}