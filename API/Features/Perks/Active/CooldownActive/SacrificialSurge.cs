using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;
using MEC;

namespace FoundationFortune.API.Features.Perks.Active.CooldownActive;

public class SacrificialSurge : CooldownActivePerkBase
{
    public override PerkType PerkType => PerkType.SacrificialSurge;
    public override string Alias => "Sacrificial Surge";
    public override TimeSpan Duration { get; protected set; } = TimeSpan.FromSeconds(15);
    public override TimeSpan Cooldown { get; protected set; } = TimeSpan.FromSeconds(240);
    public override DateTime LastActivationTime { get; set; }
    public override bool IsCurrentlyActive { get; set; }
    
    public override void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedCooldownActivePerks.ContainsKey(player)) PerkSystem.ConsumedCooldownActivePerks.Add(player, new());
        PerkSystem.PerkPlayers[this.PerkType].Add(player);
        if (!PerkSystem.ConsumedCooldownActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedCooldownActivePerks[player][this] = 1;
        else PerkSystem.ConsumedCooldownActivePerks[player][this]++;
    }

    public override void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            LastActivationTime = DateTime.Now;
            Timing.RunCoroutine(SacrificialSurgeCoroutine(player));
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }

    public override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += Hurt;
        base.SubscribeEvents();
    }

    public override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= Hurt;
        base.UnsubscribeEvents();
    }

    private void Hurt(HurtingEventArgs ev)
    {
        if (PerkSystem.HasPerk(ev.Player, PerkType.SacrificialSurge)) return;
        if (ev.Player.IsScp) ev.Amount *= 2;
        else if (ev.Player.IsHuman) ev.Amount *= 1.5f;
    }

    private IEnumerator<float> SacrificialSurgeCoroutine(Player ply)
    {
        ply.Health /= 3;
        IsCurrentlyActive = true;
        yield return Timing.WaitForSeconds((float)Duration.TotalSeconds);
        IsCurrentlyActive = false;
    }
}
