using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class TouchOfMidas : IPassivePerk
{
    public void ApplyPassiveEffect(Player player) => PerkSystem.PerkPlayers[PerkType.TouchOfMidas].Add(player);
    public PerkType PerkType { get; } = PerkType.TouchOfMidas;
    public string Alias { get; } = "Touch of Midas";
}