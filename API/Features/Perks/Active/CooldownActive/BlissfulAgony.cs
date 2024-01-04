using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Active.CooldownActive;

public class BlissfulAgony : CooldownActivePerkBase
{
    public override bool IsCurrentlyActive { get; set; }
    public override TimeSpan Cooldown { get; protected set; } = TimeSpan.FromSeconds(42);
    public override TimeSpan Duration { get; protected set; } = TimeSpan.FromSeconds(40.5f);
    public override DateTime LastActivationTime { get; set; }
    public override PerkType PerkType => PerkType.BlissfulAgony;
    public override string Alias => "Blissful Agony";

    private static readonly PlayerVoiceChatSettings BlissfulAgonySfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.BlissfulAgony);

    public override void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedCooldownActivePerks.ContainsKey(player)) PerkSystem.ConsumedCooldownActivePerks.Add(player, new());
        PerkSystem.PerkPlayers[this.PerkType].Add(player);
        if (!PerkSystem.ConsumedCooldownActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedCooldownActivePerks[player][this] = 1;
        else PerkSystem.ConsumedCooldownActivePerks[player][this]++;
        player.EnableEffect<RainbowTaste>();
    }

    public override void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            IsCurrentlyActive = true;
            Timing.RunCoroutine(BlissfulAgonyCoroutine(player));
            LastActivationTime = DateTime.Now;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }

    private IEnumerator<float> BlissfulAgonyCoroutine(Player ply)
    {
        Timing.RunCoroutine(FuckedUpHealingCoroutine(ply, Duration.Seconds, 700f));
        ply.EnableEffect<SoundtrackMute>(Duration.Seconds);
        if (BlissfulAgonySfx != null) AudioPlayer.PlaySpecialAudio(ply, BlissfulAgonySfx.AudioFile, BlissfulAgonySfx.Volume, BlissfulAgonySfx.Loop, BlissfulAgonySfx.VoiceChat);

        yield return Timing.WaitForSeconds(Duration.Seconds);
        IsCurrentlyActive = false;
        Map.Explode(ply.Position, ProjectileType.Flashbang, ply);
        Map.Explode(ply.Position, ProjectileType.FragGrenade, ply);
        ply.Kill("Blissful Agony");
    }

    private static IEnumerator<float> FuckedUpHealingCoroutine(Player ply, float duration, float totalHealedHp)
    {
        const float startHealingRate = 1f;
        const float endHealingRate = 20f;

        var elapsedTime = 0f;
        var totalHealed = 0f;

        while (elapsedTime < duration && totalHealed < totalHealedHp)
        {
            var t = elapsedTime / duration;
            var circularOutFactor = Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
            var currentHealingRate = Mathf.Lerp(startHealingRate, endHealingRate, circularOutFactor);
            var healedThisFrame = currentHealingRate * Time.deltaTime;

            if (totalHealed + healedThisFrame > totalHealedHp) healedThisFrame = totalHealedHp - totalHealed;

            ply.Heal(healedThisFrame, true);
            ply.ArtificialHealth += healedThisFrame;

            elapsedTime += Time.deltaTime;
            totalHealed += healedThisFrame;

            yield return currentHealingRate;
        }

        ply.Heal(endHealingRate);
        ply.ArtificialHealth += endHealingRate;

        yield return endHealingRate;
    }
}