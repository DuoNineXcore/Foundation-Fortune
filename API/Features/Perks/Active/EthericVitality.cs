using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Player;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Active;

public class EthericVitality : IActivePerk
{
    private static readonly PlayerVoiceChatSettings EthericVitalitySfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EthericVitality);
    private float lastActivationTime;
    public bool isCurrentlyActive { get; private set; }

    public void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
        if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
        else PerkSystem.ConsumedActivePerks[player][this]++;
        PerkSystem.PerkPlayers[PerkType.EthericVitality].Add(player);
    }

    public void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            isCurrentlyActive = true;
            Timing.RunCoroutine(EthericVitalityCoroutine(player));
            lastActivationTime = Time.time;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }

    private IEnumerator<float> EthericVitalityCoroutine(Player player)
    {
        const float healthRegenRate = 2f;
        const float regenerationInterval = 1f;
        const float invincibilityDuration = 5f;
        const float regenerationAuraDuration = 20f;

        AudioPlayer.PlaySpecialAudio(player, EthericVitalitySfx.AudioFile, EthericVitalitySfx.Volume, EthericVitalitySfx.Loop, EthericVitalitySfx.VoiceChat);

        float invincibilityTimer = 0f;
        float regenerationAuraTimer = 0f;

        while (PerkSystem.HasPerk(player, PerkType.EthericVitality) && player.Health > 0f)
        {
            if (invincibilityTimer < invincibilityDuration)
            {
                player.EnableEffect<SpawnProtected>(1.5f);
                invincibilityTimer += regenerationInterval;
            }

            if (regenerationAuraTimer < regenerationAuraDuration)
            {
                player.Heal(healthRegenRate);
                foreach (var otherPlayer in Player.List.Where(p => p != player && p.Role.Team == player.Role.Team && IsPlayerInHealingAura(p, player, 5f)))
                {
                    if (invincibilityTimer < invincibilityDuration) otherPlayer.EnableEffect<SpawnProtected>(1.5f);
                    otherPlayer.Heal(healthRegenRate);
                }
                regenerationAuraTimer += regenerationInterval;
            }

            yield return Timing.WaitForSeconds(regenerationInterval);
            if (regenerationAuraTimer >= regenerationAuraDuration) break;
            isCurrentlyActive = false;
        }
    }

    private static bool IsPlayerInHealingAura(Player targetPlayer, Player sourcePlayer, float radius)
    {
        float distanceSqr = (targetPlayer.Position - sourcePlayer.Position).sqrMagnitude;
        return distanceSqr <= radius * radius;
    }
        
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

    public int TimeUntilNextActivation { get; } = 50;
    public PerkType PerkType { get; } = PerkType.EthericVitality;
    public string Alias { get; } = "Etheric Vitality";
}