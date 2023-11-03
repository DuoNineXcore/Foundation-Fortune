using CustomPlayerEffects;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using System.Linq;
using System;
using System.Collections;
using MEC;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Enums;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.Models.Enums.Player;
using PlayerStatsSystem;
using UnityEngine;

namespace FoundationFortune.API.Perks
{
	/// <summary>
	/// This class is for perks. thats it.
	/// </summary>
	public static class PerkSystem
	{
		public static readonly List<Player> EtherealInterventionPlayers = new();
		public static readonly List<Player> ViolentImpulsesPlayers = new();
		public static readonly List<Player> EthericVitalityPlayers = new();

		public static void ClearConsumedPerks(Player player) { if (FoundationFortune.Singleton.ConsumedPerks.TryGetValue(player, out var perk)) perk.Clear(); }
		public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<PerkType, int>> consumedPerks, ref StringBuilder perkIndicator)
		{
			foreach (var perkEntry in consumedPerks.SelectMany(playerPerks => playerPerks.Value))
			{
				var (perkType, count) = (perkEntry.Key, perkEntry.Value);
				if (!FoundationFortune.Singleton.Translation.PerkCounterEmojis.TryGetValue(perkType, out var emoji)) continue;
				perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} "); 
			}
			perkIndicator.AppendLine();
		}
		
        public static void GrantPerk(Player ply, PerkType perk)
		{
			switch (perk)
			{
				case PerkType.ViolentImpulses:
					if (!ViolentImpulsesPlayers.Contains(ply)) ViolentImpulsesPlayers.Add(ply);
					break;
				case PerkType.HyperactiveBehavior:
					ply.EnableEffect<MovementBoost>(60);
					ply.ChangeEffectIntensity<MovementBoost>(30);
					ply.Stamina = 300;
					break;
				case PerkType.EthericVitality:
					if (!EthericVitalityPlayers.Contains(ply)) EthericVitalityPlayers.Add(ply);
					Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 1f, 75f);
					break;
				case PerkType.BlissfulUnawareness:
					Timing.RunCoroutine(BlissfulUnawarenessCoroutine(ply).CancelWith(ply.GameObject));
					break;
				case PerkType.EtherealIntervention:
					if (!EtherealInterventionPlayers.Contains(ply)) EtherealInterventionPlayers.Add(ply);
					break;
			}
		}

        private static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
		{
			ply.EnableEffect<RainbowTaste>(40);
			
			yield return Timing.WaitForSeconds(80f);

			Timing.RunCoroutine(CircularOutHealingCoroutine(ply, 50f, 700f));
			ply.EnableEffect<SoundtrackMute>(50f);
			
			PlayerVoiceChatSettings BlissfulUnawarenessSettings = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
				.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.BlissfulUnawareness);
			if (BlissfulUnawarenessSettings != null)
				AudioPlayer.PlaySpecialAudio(ply, BlissfulUnawarenessSettings.AudioFile,
					BlissfulUnawarenessSettings.Volume, BlissfulUnawarenessSettings.Loop,
					BlissfulUnawarenessSettings.VoiceChat);

			yield return Timing.WaitForSeconds(1f);
			
			Map.Explode(ply.Position, ProjectileType.Flashbang, ply);
			Map.Explode(ply.Position, ProjectileType.FragGrenade);
			ply.Kill("Blissful Unawareness", "");
		}
        
        private static IEnumerator<float> CircularOutHealingCoroutine(Player ply, float duration, float totalHealedHP)
        {
	        const float startHealingRate = 1f;
	        const float endHealingRate = 20f;

	        float elapsedTime = 0f;
	        float totalHealed = 0f;

	        while (elapsedTime < duration && totalHealed < totalHealedHP)
	        {
		        float t = elapsedTime / duration;
		        float circularOutFactor = Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
		        float currentHealingRate = Mathf.Lerp(startHealingRate, endHealingRate, circularOutFactor);
		        float healedThisFrame = currentHealingRate * Time.deltaTime;

		        if (totalHealed + healedThisFrame > totalHealedHP) healedThisFrame = totalHealedHP - totalHealed;

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

        public static void ActivateResurgenceBeacon(Player reviver, string targetName)
		{
			Player targetToRevive = Player.Get(targetName);
			if (targetToRevive == null) return;
			if (targetToRevive.IsDead)
			{
				if (FoundationFortune.Singleton.Config.ResetRevivedInventory) targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
				else targetToRevive.Role.Set(reviver.Role);
				targetToRevive.Health = FoundationFortune.Singleton.Config.RevivedPlayerHealth;
				targetToRevive.Teleport(reviver.Position);
				if (FoundationFortune.Singleton.Config.HuntReviver)
				{
					FoundationFortune.Singleton.FoundationFortuneAPI.AddBounty(reviver, FoundationFortune.Singleton.Config.RevivalBountyKillReward, TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.RevivalBountyTimeSeconds));
				}
				foreach (var ply in Player.List.Where(p => !p.IsNPC))
				{
					FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RevivalSuccess.Replace("%rolecolor%", reviver.Role.Color.ToHex())
					    .Replace("%nickname%", reviver.Nickname)
					    .Replace("%target%", targetToRevive.Nickname), 3f);
				}
			}
			else FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(reviver, FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName), 3f);
		}
	}
}
