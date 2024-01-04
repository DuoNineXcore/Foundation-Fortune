using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Perks.Passive;

public class ExplosiveResilience : PassivePerkBase
{
    public override PerkType PerkType => PerkType.ExplosiveResilience;
    public override string Alias => "Explosive Resilience";

    public override void ApplyPassiveEffect(Player player) => PerkSystem.PerkPlayers[this.PerkType].Add(player);

    public override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        base.SubscribeEvents();
    }

    public override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        base.UnsubscribeEvents();
    }
    
    private void Hurting(HurtingEventArgs ev)
    {
        if (!PerkSystem.HasPerk(ev.Player, this.PerkType)) return;

        switch (ev.DamageHandler.Type)
        {
            case DamageType.Custom:
            case DamageType.Strangled:
            case DamageType.Recontainment:
            case DamageType.Poison:
            case DamageType.Tesla:
            case DamageType.FriendlyFireDetector:
            case DamageType.SeveredHands:
            case DamageType.FemurBreaker: //the fuck are these damagetypes lol
            case DamageType.Unknown:
            case DamageType.Falldown:
            case DamageType.Warhead:
            case DamageType.Decontamination:
            case DamageType.Crushed:
            case DamageType.CardiacArrest:
            case DamageType.Marshmallow:
            case DamageType.Hypothermia:
                break;
            case DamageType.Explosion:
                ev.Amount *= 0.2f;
                ev.Player.EnableEffect<RainbowTaste>(10f);
                ev.Player.EnableEffect<Invigorated>(10f);
                break;
            default:
                ev.Player.EnableEffect<Burned>(1f);
                ev.Amount *= 1.1f;
                break;
        }
    }
}