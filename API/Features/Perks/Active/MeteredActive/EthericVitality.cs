using System;
using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;

namespace FoundationFortune.API.Features.Perks.Active.MeteredActive;

public class EthericVitality : MeteredActivePerkBase
{
    public override PerkType PerkType => PerkType.EthericVitality;
    public override string Alias => "Etheric Vitality";
    public override float MaxMeterValue { get; } = 1200;
    public override float MeterDepletionRate { get; } = 100;
    public override DateTime LastActivationTime { get; protected set; }
    public override bool IsCurrentlyActive { get; protected set; }

    private static readonly PlayerVoiceChatSettings EthericVitalitySfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EthericVitality);

    public override void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedMeteredActivePerks.ContainsKey(player)) PerkSystem.ConsumedMeteredActivePerks.Add(player, new());
        PerkSystem.PerkPlayers[this.PerkType].Add(player);
        if (!PerkSystem.ConsumedMeteredActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedMeteredActivePerks[player][this] = 1;
        else PerkSystem.ConsumedMeteredActivePerks[player][this]++;
    }

    public override void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            IsCurrentlyActive = true;
            Timing.RunCoroutine(EthericVitalityCoroutine(player));
            LastActivationTime = DateTime.Now;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }
    
    private IEnumerator<float> EthericVitalityCoroutine(Player player)
    {
        const float healthRegenRate = 2f;
        const float regenerationInterval = 1f;
        const float invincibilityDuration = 5f;

        AudioPlayer.PlaySpecialAudio(player, EthericVitalitySfx.AudioFile, EthericVitalitySfx.Volume, EthericVitalitySfx.Loop, EthericVitalitySfx.VoiceChat);

        float invincibilityTimer = 0f;

        while (PerkSystem.HasPerk(player, PerkType.EthericVitality) && player.Health > 0f)
        {
            if (invincibilityTimer < invincibilityDuration)
            {
                player.EnableEffect<SpawnProtected>(1.5f);
                invincibilityTimer += regenerationInterval;
            }

            player.Heal(healthRegenRate);

            foreach (var otherPlayer in Player.List.Where(p => p != player && p.Role.Team == player.Role.Team && IsPlayerInHealingAura(p, player, 5f)))
            {
                if (invincibilityTimer < invincibilityDuration) otherPlayer.EnableEffect<SpawnProtected>(1.5f);
                otherPlayer.Heal(healthRegenRate);
            }

            yield return Timing.WaitForSeconds(regenerationInterval);
            IsCurrentlyActive = false;
        }
    }

    private static bool IsPlayerInHealingAura(Player targetPlayer, Player sourcePlayer, float radius)
    {
        float distanceSqr = (targetPlayer.Position - sourcePlayer.Position).sqrMagnitude;
        return distanceSqr <= radius * radius;
    }
}

