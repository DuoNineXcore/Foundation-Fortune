using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using FoundationFortune.API.EventSystems;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.Models.Enums.Player;
using MEC;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Systems;

/// <summary>
/// This class is for perks. thats it.
/// </summary>
public static class PerkSystem
{
	private static PlayerVoiceChatSettings hyperactiveBehaviorOn = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOn);
	private static PlayerVoiceChatSettings hyperactiveBehaviorOff = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOff);
	private static PlayerVoiceChatSettings violentImpulses = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.ViolentImpulses);
	private static PlayerVoiceChatSettings blissfulUnawareness = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.BlissfulUnawareness);

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

		var coroutineHandle = new CoroutineHandle();
		if (!FoundationFortune.Singleton.PerkCoroutines.ContainsKey(ply)) FoundationFortune.Singleton.PerkCoroutines[ply] = new Dictionary<PerkType, CoroutineHandle>();

		coroutineHandle = perk switch
		{
			PerkType.BlissfulUnawareness => Timing.RunCoroutine(BlissfulUnawarenessCoroutine(ply)),
			PerkType.HyperactiveBehavior => Timing.RunCoroutine(HyperactiveBehaviorCoroutine(ply)),
			PerkType.EthericVitality => Timing.RunCoroutine(EthericVitalityCoroutine(ply)),
			_ => coroutineHandle
		};

		if (perk is PerkType.ViolentImpulses)
		{
			ply.Stamina = 0.02f;
			AudioPlayer.PlayTo(ply, violentImpulses.AudioFile, violentImpulses.Volume, violentImpulses.Loop, true);
		}

		FoundationFortune.Singleton.PerkCoroutines[ply][perk] = coroutineHandle;
	}

	public static void ClearConsumedPerks(Player player)
	{
		if (!FoundationFortune.Singleton.ConsumedPerks.TryGetValue(player, out var perks) || perks == null) return;
		foreach (var kvp in perks.ToList()) RemovePerk(player, kvp.Key);
	}

	public static void RemovePerk(Player player, PerkType perkType)
	{
		if (!FoundationFortune.Singleton.ConsumedPerks.TryGetValue(player, out var playerPerks) || playerPerks == null) return;

		switch (perkType)
		{
			case PerkType.HyperactiveBehavior:
			case PerkType.BlissfulUnawareness:
			case PerkType.EthericVitality:
				if (FoundationFortune.Singleton.PerkCoroutines.TryGetValue(player, out var coroutineHandle) && coroutineHandle.TryGetValue(perkType, out var specificCoroutine))
				{
					Timing.KillCoroutines(specificCoroutine);
					coroutineHandle.Remove(perkType);
				}
				break;
		}
		FoundationFortune.Singleton.ConsumedPerks[player].Remove(perkType);
	}

	public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<PerkType, int>> consumedPerks, ref StringBuilder perkIndicator)
	{
		foreach (var perkEntry in consumedPerks.SelectMany(playerPerks => playerPerks.Value))
		{
			var (perkType, count) = (perkEntry.Key, perkEntry.Value);
			if (!FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(perkType, out var emoji)) continue;
			perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} ");
		}

		perkIndicator.AppendLine();
	}

	private static IEnumerator<float> EthericVitalityCoroutine(Player player)
	{
		const float healthRegenRate = 2f;
		const float artificialHealthRegenRate = 3f;
		const float stationaryHealthLossRate = 2f;
		const float stationaryTimeThreshold = 3f;
		const float maxAHP = 50f; 
		var stationaryTime = 4f;
		var ethericVitalitySettings = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EthericVitality);
		
		AudioPlayer.PlayTo(player, ethericVitalitySettings.AudioFile, ethericVitalitySettings.Volume, ethericVitalitySettings.Loop, true);
		while (HasPerk(player, PerkType.EthericVitality) && player.Health > 0f)
		{
			player.Heal(healthRegenRate);

			if (player.ArtificialHealth < maxAHP) player.ArtificialHealth += Mathf.Min(artificialHealthRegenRate, maxAHP - player.ArtificialHealth);

			if (((FpcRole)player.Role).FirstPersonController.FpcModule.Motor.Velocity == Vector3.zero)
			{
				stationaryTime += 1f;
				if (stationaryTime >= stationaryTimeThreshold)
				{
					if (player.Health > stationaryHealthLossRate) player.Hurt(stationaryHealthLossRate, DamageType.Poison);
					else break;
				}
			}
			else stationaryTime = 0f;

			yield return Timing.WaitForSeconds(1f);
		}
	}


	public static void ActivateResurgenceBeacon(Player reviver, string targetName)
	{
		var targetToRevive = Player.Get(targetName);
		if (targetToRevive == null) return;
		if (targetToRevive.IsDead)
		{
			if (FoundationFortune.PerkSystemSettings.ResetRevivedInventory) targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
			else targetToRevive.Role.Set(reviver.Role);
			
			targetToRevive.Health = FoundationFortune.PerkSystemSettings.RevivedPlayerHealth;
			targetToRevive.Teleport(reviver.Position);
			
			if (FoundationFortune.PerkSystemSettings.HuntReviver) ServerBountySystem.AddBounty(reviver, FoundationFortune.PerkSystemSettings.RevivalBountyKillReward, TimeSpan.FromSeconds(FoundationFortune.PerkSystemSettings.RevivalBountyTimeSeconds));
			foreach (var ply in Player.List.Where(p => !p.IsNPC))
			{
				FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RevivalSuccess
						.Replace("%rolecolor%", reviver.Role.Color.ToHex())
						.Replace("%nickname%", reviver.Nickname)
						.Replace("%target%", targetToRevive.Nickname));
			}
		}
		else FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(reviver, FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName));
	}

	private static IEnumerator<float> HyperactiveBehaviorCoroutine(Player player)
	{
		while (HasPerk(player, PerkType.HyperactiveBehavior))
		{
			player.EnableEffect(EffectType.MovementBoost);
			var randomizedStamina = UnityEngine.Random.Range(150, 301);
			var randomizedMovementSpeed = UnityEngine.Random.Range(20, 51);

			player.Stamina = randomizedStamina;
			player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)randomizedMovementSpeed);
			if (hyperactiveBehaviorOn != null) AudioPlayer.PlayTo(player, hyperactiveBehaviorOn.AudioFile, hyperactiveBehaviorOn.Volume, hyperactiveBehaviorOn.Loop, false);
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(player, $"<b>+{randomizedMovementSpeed} Movement Speed, +{randomizedStamina} Stamina</b>");
			yield return Timing.WaitForSeconds(UnityEngine.Random.Range(10f, 15f));

			player.Stamina = 100;
			player.ChangeEffectIntensity(EffectType.MovementBoost, 0);
			if (hyperactiveBehaviorOff != null) AudioPlayer.PlayTo(player, hyperactiveBehaviorOff.AudioFile, hyperactiveBehaviorOff.Volume, hyperactiveBehaviorOff.Loop, false);
			yield return Timing.WaitForSeconds(5f);
		}
	}

	private static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
	{
		ply.EnableEffect<RainbowTaste>(40);
		yield return Timing.WaitForSeconds(5f);

		Timing.RunCoroutine(FuckedUpHealingCoroutine(ply, 50f, 700f));
		ply.EnableEffect<SoundtrackMute>(50f);
		if (blissfulUnawareness != null) AudioPlayer.PlaySpecialAudio(ply, blissfulUnawareness.AudioFile, blissfulUnawareness.Volume, blissfulUnawareness.Loop, blissfulUnawareness.VoiceChat);

		yield return Timing.WaitForSeconds(41f);
		Map.Explode(ply.Position, ProjectileType.Flashbang, ply);
		Map.Explode(ply.Position, ProjectileType.FragGrenade);
		ply.Kill("Blissful Unawareness");
	}

	private static IEnumerator<float> FuckedUpHealingCoroutine(Player ply, float duration, float totalHealedHP)
	{
		const float startHealingRate = 1f;
		const float endHealingRate = 20f;

		var elapsedTime = 0f;
		var totalHealed = 0f;

		while (elapsedTime < duration && totalHealed < totalHealedHP)
		{
			var t = elapsedTime / duration;
			var circularOutFactor = Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
			var currentHealingRate = Mathf.Lerp(startHealingRate, endHealingRate, circularOutFactor);
			var healedThisFrame = currentHealingRate * Time.deltaTime;

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
	
	public static bool HasPerk(Player player, PerkType perkType) => PerkPlayers.TryGetValue(perkType, out var perkPlayer) && perkPlayer.Contains(player);
}

