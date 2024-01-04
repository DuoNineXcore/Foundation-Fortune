using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Passive;

public class GracefulSaint : PassivePerkBase
{
    public override PerkType PerkType => PerkType.GracefulSaint;
    public override string Alias => "Graceful Saint";

    private const float RegenerationRate = 1f;
    private const float DamageReductionScale = 0.1f;
    private PlayerVoiceChatSettings GracefulSaintSfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.GracefulSaint);
    private Dictionary<Player, bool> playersWithEnabledAbilities = new();

    public override void ApplyPassiveEffect(Player player)
    {
        PerkSystem.PerkPlayers[this.PerkType].Add(player);
        Timing.RunCoroutine(RegenerationCoroutine(player));
        playersWithEnabledAbilities[player] = true;
    }

    public override void SubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        Exiled.Events.Handlers.Player.Dying += Dying;
        Exiled.Events.Handlers.Player.Died += Died;
        Exiled.Events.Handlers.Player.Spawned += Spawned;
        base.SubscribeEvents();
    }

    public override void UnsubscribeEvents()
    {
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        Exiled.Events.Handlers.Player.Dying -= Dying;
        Exiled.Events.Handlers.Player.Died -= Died;
        Exiled.Events.Handlers.Player.Spawned -= Spawned;
        base.UnsubscribeEvents();
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (!PerkSystem.HasPerk(ev.Player, this.PerkType)) return;
        if (ev.Attacker != ev.Player)
        {
            float damageReduction = 1f - (ev.Player.Health / ev.Player.MaxHealth * DamageReductionScale);
            ev.Amount *= damageReduction;   
        }

        if (PerkSystem.HasPerk(ev.Attacker, this.PerkType)) DisablePerkAbilities(ev.Attacker, 3);
    }

    private void Dying(DyingEventArgs ev)
    {
        if (!PerkSystem.HasPerk(ev.Player, this.PerkType)) return;
        if (PerkSystem.HasPerk(ev.Player, PerkType.BlissfulAgony)) PerkSystem.RemovePerk(ev.Player, PerkType.BlissfulAgony.ToPerk());
        ev.IsAllowed = false;
        RoleTypeId role = ev.Player.Role.Type;
        Room room = Room.List.Where(r => r.Zone == ev.Player.Zone & !FoundationFortune.PerkSystemSettings.ForbiddenEtherealInterventionRoomTypes.Contains(r.Type)).GetRandomValue();

        Timing.CallDelayed(0.1f, delegate
        {
            ev.Player.Role.Set(role, SpawnReason.Revived, RoleSpawnFlags.None);
            ev.Player.Teleport(room);
        });
    }

    private void Died(DiedEventArgs ev)
    {
        if (PerkSystem.HasPerk(ev.Attacker, this.PerkType)) DisablePerkAbilities(ev.Attacker, 3);
    }
    
    private void Spawned(SpawnedEventArgs ev)
    {
        if (PerkSystem.HasPerk(ev.Player, this.PerkType) && ev.Reason == SpawnReason.Revived)
        {
            Timing.CallDelayed(0.2f, () =>
            {
                PerkSystem.RemovePerk(ev.Player, this);
                Map.Explode(ev.Player.Position, ProjectileType.Flashbang, ev.Player);
                ev.Player.EnableEffect<Blinded>(2f);
                if (GracefulSaintSfx != null) AudioPlayer.PlayTo(ev.Player, GracefulSaintSfx.AudioFile, GracefulSaintSfx.Volume, GracefulSaintSfx.Loop, true);
            });
        }
    }
    
    private void DisablePerkAbilities(Player player, int duration)
    {
        playersWithEnabledAbilities[player] = false;
        Timing.CallDelayed(duration, () => { playersWithEnabledAbilities[player] = true; });
    }
    
    private IEnumerator<float> RegenerationCoroutine(Player player)
    {
        while (PerkSystem.HasPerk(player, this.PerkType) && playersWithEnabledAbilities[player])
        {
            if (((FpcRole)player.Role).FirstPersonController.FpcModule.Motor.Velocity == Vector3.zero) player.Heal(RegenerationRate);
            yield return Timing.WaitForSeconds(1f);
        }
    }
}

