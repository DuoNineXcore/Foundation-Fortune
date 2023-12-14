using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class VitalitySacrifice : IPassivePerk
{
    public PerkType PerkType { get; } = PerkType.VitalitySacrifice;
    public string Alias { get; } = "Vitality Sacrifice";
    public void ApplyPassiveEffect(Player player)
    {
        PerkSystem.PerkPlayers[PerkType.VitalitySacrifice].Add(player);
        float health = player.MaxHealth;
        player.MaxHealth = health * 0.7f;
        player.ArtificialHealth = health * 3;
    }
}