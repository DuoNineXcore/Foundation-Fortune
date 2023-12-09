using System.Collections.Generic;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using FoundationFortune.API.Features.Systems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Perks.Active
{
    public class BlissfulUnawareness : IActivePerk
    {
        private static readonly PlayerVoiceChatSettings BlissfulUnawarenessSfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.BlissfulUnawareness);
        public static readonly List<Player> BlissfulUnawarenessPlayers = new();

        public void ApplyPassiveEffect(Player player)
        {
            if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());

            BlissfulUnawarenessPlayers.Add(player);
            player.EnableEffect<RainbowTaste>();

            if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
            else PerkSystem.ConsumedActivePerks[player][this]++;
        }

        public CoroutineHandle StartActivePerkAbility(Player player) => Timing.RunCoroutine(BlissfulUnawarenessCoroutine(player));
    
        private static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
        {
            Timing.RunCoroutine(FuckedUpHealingCoroutine(ply, 42f, 700f));
            ply.EnableEffect<SoundtrackMute>(42f);
            if (BlissfulUnawarenessSfx != null) AudioPlayer.PlaySpecialAudio(ply, BlissfulUnawarenessSfx.AudioFile, BlissfulUnawarenessSfx.Volume, BlissfulUnawarenessSfx.Loop, BlissfulUnawarenessSfx.VoiceChat);

            yield return Timing.WaitForSeconds(41f);
            Map.Explode(ply.Position, ProjectileType.Flashbang, ply);
            Map.Explode(ply.Position, ProjectileType.FragGrenade);
            ply.Kill("Blissful Unawareness");
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

        public PerkType PerkType { get; } = PerkType.BlissfulUnawareness;
        public string Alias { get; } = "Blissful Unawareness";
    }
}