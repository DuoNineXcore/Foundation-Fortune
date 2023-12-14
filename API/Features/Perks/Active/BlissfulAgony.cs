using System.Collections.Generic;
using CustomPlayerEffects;
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

public class BlissfulAgony : IActivePerk
{
    private static readonly PlayerVoiceChatSettings BlissfulAgonySfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.BlissfulAgony);
    private float lastActivationTime;
    public bool isCurrentlyActive { get; private set; }

    public void ApplyPassiveEffect(Player player)
    {
        if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
        PerkSystem.PerkPlayers[PerkType.BlissfulAgony].Add(player);

        player.EnableEffect<RainbowTaste>();

        if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
        else PerkSystem.ConsumedActivePerks[player][this]++;
    }

    public void StartActivePerkAbility(Player player)
    {
        if (!IsOnCooldown)
        {
            isCurrentlyActive = true;
            Timing.RunCoroutine(BlissfulAgonyCoroutine(player));
            lastActivationTime = Time.time;
        }
        else FoundationFortune.Instance.HintSystem.BroadcastHint(player, "\n<b>Perk is on cooldown, dumbass</b>");
    }
        
    private IEnumerator<float> BlissfulAgonyCoroutine(Player ply)
    {
        Timing.RunCoroutine(FuckedUpHealingCoroutine(ply, 40.5f, 700f));
        ply.EnableEffect<SoundtrackMute>(42f);
        if (BlissfulAgonySfx != null) AudioPlayer.PlaySpecialAudio(ply, BlissfulAgonySfx.AudioFile, BlissfulAgonySfx.Volume, BlissfulAgonySfx.Loop, BlissfulAgonySfx.VoiceChat);

        yield return Timing.WaitForSeconds(41f);
        isCurrentlyActive = false;
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
        
    public int TimeUntilNextActivation { get; } = 42;
    public PerkType PerkType { get; } = PerkType.BlissfulAgony;
    public string Alias { get; } = "Blissful Agony";
}