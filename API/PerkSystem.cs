using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.Models.Enums.Player;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API;

/// <summary>
/// This class is for perks. thats it.
/// </summary>
public static class PerkSystem
{
	public static readonly Dictionary<PerkType, List<Player>> PerkPlayers = new Dictionary<PerkType, List<Player>>
	{
		{ PerkType.EtherealIntervention, new List<Player>() },
		{ PerkType.ViolentImpulses, new List<Player>() },
		{ PerkType.EthericVitality, new List<Player>() },
		{ PerkType.HyperactiveBehavior, new List<Player>() }
	};

	public static void GrantPerk(Player ply, PerkType perk)
	{
		if (!PerkPlayers.ContainsKey(perk)) PerkPlayers[perk] = new List<Player>();
		if (!PerkPlayers[perk].Contains(ply)) PerkPlayers[perk].Add(ply);

		switch (perk)
		{
			case PerkType.HyperactiveBehavior:
				Timing.RunCoroutine(HyperactiveBehaviorCoroutine(ply).CancelWith(ply.GameObject));
				break;
			case PerkType.ViolentImpulses:
				break;
			case PerkType.EthericVitality:
				Timing.RunCoroutine(EthericVitalityCoroutine(ply).CancelWith(ply.GameObject));
				break;
			case PerkType.BlissfulUnawareness:
				Timing.RunCoroutine(BlissfulUnawarenessCoroutine(ply).CancelWith(ply.GameObject));
				break;
		}
	}

	public static bool HasPerk(Player player, PerkType perkType) => PerkPlayers.TryGetValue(perkType, out var perkPlayer) && perkPlayer.Contains(player);
	public static bool RemovePerk(Player player, PerkType perkType) => PerkPlayers.TryGetValue(perkType, out var players) && players.Remove(player);
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

	private static IEnumerator<float> EthericVitalityCoroutine(Player player)
	{
		const float healthRegenRate = 2f;
		const float artificialHealthRegenRate = 3f;
		const float stationaryHealthLossRate = 2f;
		const float stationaryTimeThreshold = 5f;

		float stationaryTime = 0f;

		while (HasPerk(player, PerkType.EthericVitality))
		{
			player.Heal(healthRegenRate);
			player.ArtificialHealth += artificialHealthRegenRate;

			if (((FpcRole)player.Role).FirstPersonController.FpcModule.Motor.Velocity == Vector3.zero)
			{
				stationaryTime += 1f;
				if (stationaryTime >= stationaryTimeThreshold) player.Hurt(stationaryHealthLossRate, DamageType.Poison);
			}
			else stationaryTime = 0f;

			yield return Timing.WaitForSeconds(1f);
		}
	}

	public static void ActivateResurgenceBeacon(Player reviver, string targetName)
	{
		Player targetToRevive = Player.Get(targetName);
		if (targetToRevive == null) return;
		if (targetToRevive.IsDead)
		{
			if (FoundationFortune.PerkSystemSettings.ResetRevivedInventory)
				targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
			else targetToRevive.Role.Set(reviver.Role);
			targetToRevive.Health = FoundationFortune.PerkSystemSettings.RevivedPlayerHealth;
			targetToRevive.Teleport(reviver.Position);
			if (FoundationFortune.PerkSystemSettings.HuntReviver)
				FoundationFortune.Singleton.FoundationFortuneAPI.AddBounty(reviver,
					FoundationFortune.PerkSystemSettings.RevivalBountyKillReward,
					TimeSpan.FromSeconds(FoundationFortune.PerkSystemSettings.RevivalBountyTimeSeconds));
			foreach (var ply in Player.List.Where(p => !p.IsNPC))
			{
				FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply,
					FoundationFortune.Singleton.Translation.RevivalSuccess
						.Replace("%rolecolor%", reviver.Role.Color.ToHex())
						.Replace("%nickname%", reviver.Nickname)
						.Replace("%target%", targetToRevive.Nickname), 3f);
			}
		}
		else
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(reviver,
				FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName), 3f);
	}

	private static IEnumerator<float> HyperactiveBehaviorCoroutine(Player player)
	{
		PlayerVoiceChatSettings HyperactiveBehaviorOn =
			FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings =>
				settings.VoiceChatUsageType == PlayerVoiceChatUsageType.HyperactiveBehaviorOn);
		PlayerVoiceChatSettings HyperactiveBehaviorOff =
			FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings =>
				settings.VoiceChatUsageType == PlayerVoiceChatUsageType.HyperactiveBehaviorOff);

		while (HasPerk(player, PerkType.HyperactiveBehavior))
		{
			player.EnableEffect(EffectType.MovementBoost);
			int randomizedStamina = UnityEngine.Random.Range(150, 301);
			int randomizedMovementSpeed = UnityEngine.Random.Range(20, 51);

			player.Stamina = randomizedStamina;
			player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)randomizedMovementSpeed);
			if (HyperactiveBehaviorOn != null)
				AudioPlayer.PlayTo(player, HyperactiveBehaviorOn.AudioFile, HyperactiveBehaviorOn.Volume,
					HyperactiveBehaviorOn.Loop, false);
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(player,
				$"<b>+{randomizedMovementSpeed} Movement Speed, +{randomizedStamina} Stamina</b>", 3);
			yield return Timing.WaitForSeconds(UnityEngine.Random.Range(10f, 15f));

			player.Stamina = 100;
			player.ChangeEffectIntensity(EffectType.MovementBoost, 0);
			if (HyperactiveBehaviorOff != null)
				AudioPlayer.PlayTo(player, HyperactiveBehaviorOff.AudioFile, HyperactiveBehaviorOff.Volume,
					HyperactiveBehaviorOff.Loop, false);
			yield return Timing.WaitForSeconds(5f);
		}
	}

	private static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
	{
		PlayerVoiceChatSettings BlissfulUnawarenessSettings =
			FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings =>
				settings.VoiceChatUsageType == PlayerVoiceChatUsageType.BlissfulUnawareness);
		ply.EnableEffect<RainbowTaste>(40);
		yield return Timing.WaitForSeconds(5f);

		Timing.RunCoroutine(FuckedUpHealingCoroutine(ply, 50f, 700f));
		ply.EnableEffect<SoundtrackMute>(50f);
		if (BlissfulUnawarenessSettings != null)
			AudioPlayer.PlaySpecialAudio(ply, BlissfulUnawarenessSettings.AudioFile, BlissfulUnawarenessSettings.Volume,
				BlissfulUnawarenessSettings.Loop, BlissfulUnawarenessSettings.VoiceChat);

		yield return Timing.WaitForSeconds(41f);
		Map.Explode(ply.Position, ProjectileType.Flashbang, ply);
		Map.Explode(ply.Position, ProjectileType.FragGrenade);
		ply.Kill("Blissful Unawareness", "");
	}

	private static IEnumerator<float> FuckedUpHealingCoroutine(Player ply, float duration, float totalHealedHP)
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
}

