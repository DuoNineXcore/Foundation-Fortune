using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Classes;
using System.Linq;
using System;
using MEC;
using System.Collections.Generic;
using PlayerRoles.PlayableScps.Scp939.Ripples;
using Exiled.API.Features.Roles;
using PlayerStatsSystem;

namespace FoundationFortune.API.Perks
{
	/// <summary>
	/// This class is for perks. thats it.
	/// </summary>
	public static class PerkSystem
	{
		public static readonly List<Player> EtherealInterventionPlayers = new();
		public static readonly List<Player> ViolentImpulsesPlayers = new();
		
        public static void ClearConsumedPerks(Player player) { if (FoundationFortune.Singleton.ConsumedPerks.TryGetValue(player, out var perk)) perk.Clear(); }

        public static void GrantPerk(Player ply, PerkType perk)
		{
			switch (perk)
			{
				case PerkType.ViolentImpulses:
					ply.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(10f).DecayRate = 0.0f;
					if (!ViolentImpulsesPlayers.Contains(ply)) ViolentImpulsesPlayers.Add(ply);
					break;
				case PerkType.Hyperactivity:
					ply.EnableEffect<MovementBoost>(60);
					ply.ChangeEffectIntensity<MovementBoost>(30);
					ply.Stamina = 300;
					break;
				case PerkType.EthericVitality:
					Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 4f, 75f);
					break;
				case PerkType.BlissfulUnawareness:
					Timing.RunCoroutine(BlissfulUnawarenessCoroutine(ply).CancelWith(ply.GameObject));
					break;
				case PerkType.EtherealIntervention:
					if (!EtherealInterventionPlayers.Contains(ply)) EtherealInterventionPlayers.Add(ply);
					break;
			}
		}

        public static void ConsumePerk(Player player, PerkType perkType)
        {
			var consumedPerks = FoundationFortune.Singleton.ConsumedPerks;
            if (!consumedPerks.ContainsKey(player)) consumedPerks[player] = new Dictionary<PerkType, int>();
            if (consumedPerks[player].ContainsKey(perkType)) consumedPerks[player][perkType]++;
            else consumedPerks[player][perkType] = 1;
        }

        private static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
		{
			Log.Debug("Blissful Unawareness 1st coroutine started.");
			ply.EnableEffect<MovementBoost>(120);
			ply.ChangeEffectIntensity<MovementBoost>(10);
			yield return Timing.WaitForSeconds(80f);
            ply.EnableEffect<Blinded>(1);
            
            
            Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 5f, 50f);
            Log.Debug("Blissful Unawareness 1st coroutine finished.");
			Log.Debug("Blissful Unawareness 2nd coroutine started.");
			PlayerVoiceChatSettings BlissfulUnawarenessSettings = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
			    .FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.BlissfulUnawareness);
			ply.EnableEffect<SoundtrackMute>(50f);
			AudioPlayer.PlaySpecialAudio(ply, BlissfulUnawarenessSettings.AudioFile, BlissfulUnawarenessSettings.Volume, BlissfulUnawarenessSettings.Loop, BlissfulUnawarenessSettings.VoiceChat);

            yield return Timing.WaitForSeconds(41f);

			Log.Debug("Blissful Unawareness 2nd coroutine finished.");
			Map.Explode(ply.Position, Exiled.API.Enums.ProjectileType.Flashbang, ply);
			Map.Explode(ply.Position, Exiled.API.Enums.ProjectileType.FragGrenade, ply);
		}

		public static bool ActivateResurgenceBeacon(Player reviver, string targetName)
		{
			Player targetToRevive = Player.Get(targetName);
			if (targetToRevive == null) return false;
			if (targetToRevive.IsDead)
			{
				if (FoundationFortune.Singleton.Config.ResetRevivedInventory) targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
				else targetToRevive.Role.Set(reviver.Role);
				targetToRevive.Health = FoundationFortune.Singleton.Config.RevivedPlayerHealth;
				targetToRevive.Teleport(reviver.Position);
				if (FoundationFortune.Singleton.Config.HuntReviver)
				{
					FoundationFortune.Singleton.ServerEvents.AddBounty(reviver, FoundationFortune.Singleton.Config.RevivalBountyKillReward, TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.RevivalBountyTimeSeconds));
				}
				foreach (var ply in Player.List.Where(p => !p.IsNPC))
				{
					FoundationFortune.Singleton.ServerEvents.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RevivalSuccess.Replace("%rolecolor%", reviver.Role.Color.ToHex())
					    .Replace("%nickname%", reviver.Nickname)
					    .Replace("%target%", targetToRevive.Nickname), 3f);
				}
				return true;
			}
			else
			{
				FoundationFortune.Singleton.ServerEvents.EnqueueHint(reviver, FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName), 3f);
				return false;
			}
		}
	}
}
