using System;
using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;
using Random = UnityEngine.Random;

namespace FoundationFortune.API.Features.Perks.Active.CooldownActive;

public class HyperactiveBehavior : CooldownActivePerkBase
{
    public override bool IsCurrentlyActive { get; set; }
    public override PerkType PerkType => PerkType.HyperactiveBehavior;
    public override string Alias => "Hyperactive Behavior";
    public override DateTime LastActivationTime { get; set; }
    public override TimeSpan Cooldown { get; protected set; } = TimeSpan.FromSeconds(5);
    public override TimeSpan Duration { get; protected set; } = TimeSpan.FromSeconds(10);

    private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOn = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOn);
    private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOff = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOff);

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
            Timing.RunCoroutine(HyperactiveBehaviorCoroutine(player));
            LastActivationTime = DateTime.Now;
            IsCurrentlyActive = true;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }

    private IEnumerator<float> HyperactiveBehaviorCoroutine(Player player)
    {
        float activationDuration = Random.Range(5f, 10f);
        float cooldownTime = Random.Range(5f, 10f);

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
        IsCurrentlyActive = false;

        Cooldown = TimeSpan.FromSeconds(cooldownTime);
    }
}


