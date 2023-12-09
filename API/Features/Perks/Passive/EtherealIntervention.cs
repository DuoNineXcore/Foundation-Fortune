using System.Collections.Generic;
using Exiled.API.Features;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;

namespace FoundationFortune.API.Features.Perks.Passive
{
    public class EtherealIntervention : IPassivePerk
    {
        public static readonly List<Player> EtherealInterventionPlayers = new();

        public void ApplyPassiveEffect(Player player)
        {
            EtherealInterventionPlayers.Add(player);
        }

        public PerkType PerkType { get; } = PerkType.EtherealIntervention;
        public string Alias { get; } = "Ethereal Intervention";
    }
}