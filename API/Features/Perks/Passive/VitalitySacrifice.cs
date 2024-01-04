using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Systems;
using PlayerRoles;

namespace FoundationFortune.API.Features.Perks.Passive
{
    public class VitalitySacrifice : PassivePerkBase
    {
        public override PerkType PerkType => PerkType.VitalitySacrifice;
        public override string Alias => "Vitality Sacrifice";

        public override void ApplyPassiveEffect(Player player)
        {
            PerkSystem.PerkPlayers[this.PerkType].Add(player);
            player.MaxHealth *= 0.7f;
            if (player.Health > player.MaxHealth) player.Health = player.MaxHealth;
        }

        public override void SubscribeEvents()
        {
            base.SubscribeEvents();
            Exiled.Events.Handlers.Player.Hurting += OnPlayerHurting;
        }
        
        public override void UnsubscribeEvents()
        {
            base.UnsubscribeEvents();
            Exiled.Events.Handlers.Player.Hurting -= OnPlayerHurting;
        }

        protected override void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
        {
            if (PerkSystem.HasPerk(ev.Player, PerkType.EthericVitality)) PerkSystem.RemovePerk(ev.Player, PerkType.EthericVitality.ToPerk());
            base.UsedFoundationFortunePerk(ev);
        }
        
        private void OnPlayerHurting(HurtingEventArgs ev)
        {
            if (PerkSystem.HasPerk(ev.Player, this.PerkType)) return;

            float damageBoost = CalculateDamageBoost(ev.Player);
            ev.Amount *= damageBoost;
        }

        private static float CalculateDamageBoost(Player player)
        {
            float healthPercentage = player.Health / player.MaxHealth;
            float maxBoost = player.Role.Team == Team.SCPs ? 2.4f : 1.7f;
            return 1.0f + (1 - healthPercentage) * (maxBoost - 1.0f);
        }
    }
}