using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Systems;
using MEC;

namespace FoundationFortune.API.Features.Perks.Active.MeteredActive
{
    public class ViolentImpulses : PassivePerkBase
    {
        private const float VulnerabilityMultiplier = 1.5f;
        private const int StaminaPenalty = 5;
        private readonly Dictionary<Player, CoroutineHandle> _vulnerabilityCoroutines = new();
        private readonly Dictionary<Player, bool> _vulnerablePlayers = new();

        public override PerkType PerkType => PerkType.ViolentImpulses;
        public override string Alias => "Violent Impulses";

        public override void ApplyPassiveEffect(Player player)
        {
            PerkSystem.PerkPlayers[this.PerkType].Add(player);
            _vulnerablePlayers[player] = false;
        }

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
            Exiled.Events.Handlers.Player.Shooting += OnPlayerShooting;
            Exiled.Events.Handlers.Player.Shot += OnPlayerShot;
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
        }
        
        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            Exiled.Events.Handlers.Player.Shooting -= OnPlayerShooting;
            Exiled.Events.Handlers.Player.Shot -= OnPlayerShot;
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;

            foreach (var coroutine in _vulnerabilityCoroutines.Values) Timing.KillCoroutines(coroutine);
            _vulnerabilityCoroutines.Clear();
            _vulnerablePlayers.Clear();
        }

        private void OnPlayerShooting(ShootingEventArgs ev)
        {
            if (_vulnerabilityCoroutines.TryGetValue(ev.Player, out var coroutine) && PerkSystem.HasPerk(ev.Player, this.PerkType)) Timing.KillCoroutines(coroutine);
            _vulnerabilityCoroutines[ev.Player] = Timing.CallDelayed(1f, () => StartVulnerabilityWindow(ev.Player));
        }

        private void OnPlayerShot(ShotEventArgs ev)
        {
            if (ev.Target == null && ev.Player.Stamina > StaminaPenalty && PerkSystem.HasPerk(ev.Player, this.PerkType)) ev.Player.Stamina -= StaminaPenalty;
        }

        private void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (ev.Attacker != null && PerkSystem.HasPerk(ev.Player, this.PerkType)) 
                if (ev.Player != ev.Attacker) ev.Player.EnableEffect(EffectType.Disabled, 2f);
            
            if (_vulnerablePlayers.TryGetValue(ev.Player, out var isVulnerable) && isVulnerable) ev.Amount *= VulnerabilityMultiplier;
        }
        
        private void StartVulnerabilityWindow(Player player)
        {
            _vulnerablePlayers[player] = true;
            Timing.CallDelayed(5f, () => _vulnerablePlayers[player] = false);
        }
    }
}
