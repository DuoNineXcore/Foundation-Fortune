using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Systems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Active;

public class SacrificialSurge : IActivePerk
{
    private float lastActivationTime;
    public bool isCurrentlyActive { get; private set; }

    public void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
        PerkSystem.PerkPlayers[PerkType.SacrificialSurge].Add(player);
        if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
        else PerkSystem.ConsumedActivePerks[player][this]++;
    }

    public void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            isCurrentlyActive = true;
            player.Health /= 3;
            lastActivationTime = Time.time;
            Timing.RunCoroutine(SacrificialSurgeCoroutine(player));
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }
    
    private IEnumerator<float> SacrificialSurgeCoroutine(Player ply)
    {
        ply.EnableEffect<SoundtrackMute>(42f);

        yield return Timing.WaitForSeconds(41f);
        isCurrentlyActive = false;
    }

    public PerkType PerkType { get; } = PerkType.SacrificialSurge;
    public string Alias { get; } = "Sacrificial Surge";
    public int TimeUntilNextActivation { get; } = 240;
    
    public bool IsOnCooldown
    {
        get
        {
            return Time.time - lastActivationTime < TimeUntilNextActivation;
        }
    }
    
    public float GetRemainingCooldown()
    {
        var remainingCooldown = Mathf.Max(0f, lastActivationTime + TimeUntilNextActivation - Time.time);
        return remainingCooldown;
    }
}