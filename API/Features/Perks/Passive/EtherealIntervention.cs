using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class EtherealIntervention : IPassivePerk
{
    public void ApplyPassiveEffect(Player player) => PerkSystem.PerkPlayers[PerkType.EtherealIntervention].Add(player);
    public PerkType PerkType { get; } = PerkType.EtherealIntervention;
    public string Alias { get; } = "Ethereal Intervention";
}