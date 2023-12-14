using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class ExplosiveResilience : IPassivePerk
{
    public PerkType PerkType { get; } = PerkType.ExplosiveResilience;
    public string Alias { get; } = "Explosive Resilience";
    public void ApplyPassiveEffect(Player player)
    {
        PerkSystem.PerkPlayers[PerkType.ExplosiveResilience].Add(player);
    }
}