using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class GuardiansGrace : IPassivePerk
{
    public void ApplyPassiveEffect(Player player) => PerkSystem.PerkPlayers[PerkType.GuardiansGrace].Add(player);
    
    private void Hurt(HurtingEventArgs ev)
    {
        if (!PerkSystem.PerkPlayers[PerkType.GuardiansGrace].Contains(ev.Player)) return;
        ev.IsAllowed = false;
        PerkSystem.RemovePerk(ev.Player, this);
    }
    
    public PerkType PerkType { get; } = PerkType.GuardiansGrace;
    public string Alias { get; } = "Guardian's Grace";
}
