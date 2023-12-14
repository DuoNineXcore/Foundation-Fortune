using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Player;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Active;

public class HyperactiveBehavior : IActivePerk
{
    private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOn = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOn);
    private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOff = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOff);
    private float lastActivationTime;
    public bool isCurrentlyActive { get; private set; }
    
    public void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
        PerkSystem.PerkPlayers[PerkType.HyperactiveBehavior].Add(player);
        if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
        else PerkSystem.ConsumedActivePerks[player][this]++;
    }

    public void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            float activationDuration = Random.Range(5f, 10f);
            float cooldownTime = Random.Range(5f, 10f);
            Timing.RunCoroutine(HyperactiveBehaviorCoroutine(player, activationDuration));
            lastActivationTime = Time.time;
            TimeUntilNextActivation = (int)cooldownTime;
            isCurrentlyActive = true;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }
        
    private IEnumerator<float> HyperactiveBehaviorCoroutine(Player player, float activationDuration)
    {
        while (PerkSystem.HasPerk(player, PerkType.HyperactiveBehavior))
        {
            player.EnableEffect(EffectType.MovementBoost);
            var randomizedStamina = Random.Range(150, 301);
            var randomizedMovementSpeed = Random.Range(20, 51);

            player.Stamina = randomizedStamina;
            player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)randomizedMovementSpeed);
            if (HyperactiveBehaviorOn != null) AudioPlayer.PlayTo(player, HyperactiveBehaviorOn.AudioFile, HyperactiveBehaviorOn.Volume, HyperactiveBehaviorOn.Loop, false);
            FoundationFortune.Instance.HintSystem.BroadcastHint(player, $"<b>+{randomizedMovementSpeed} Movement Speed, +{randomizedStamina} Stamina</b>");
            yield return Timing.WaitForSeconds(activationDuration);

            player.Stamina = 100;
            player.ChangeEffectIntensity(EffectType.MovementBoost, 0);
            if (HyperactiveBehaviorOff != null) AudioPlayer.PlayTo(player, HyperactiveBehaviorOff.AudioFile, HyperactiveBehaviorOff.Volume, HyperactiveBehaviorOff.Loop, false);
            isCurrentlyActive = false;
            yield break;
        }
    }

    public int TimeUntilNextActivation { get; private set; } = 5;

    public bool IsOnCooldown
    {
        get
        {
            return Time.time - lastActivationTime < TimeUntilNextActivation;
        }
    }
    
    public bool IsCurrentlyActive
    {
        get
        {
            return isCurrentlyActive;
        }
    }
    
    public float GetRemainingCooldown()
    {
        var remainingCooldown = Mathf.Max(0f, lastActivationTime + TimeUntilNextActivation - Time.time);
        return remainingCooldown;
    }
        
    public PerkType PerkType { get; } = PerkType.HyperactiveBehavior;
    public string Alias { get; } = "Hyperactive Behavior";
}