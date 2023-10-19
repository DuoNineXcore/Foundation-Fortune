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

namespace FoundationFortune.API.Perks
{
	/// <summary>
	/// This class is for perks. thats it.
	/// </summary>
	public static class PerkSystem
	{
		public static List<Player> EtherealInterventionPlayers = new();
		public static void GrantPerk(Player ply, PerkType perk)
		{
			switch (perk)
			{
				case PerkType.OvershieldedProtection:
					ply.AddAhp(50, decay: 0);
					break;
				case PerkType.BoostedResilience:
					ply.Heal(50, true);
					ply.MaxHealth += 50;
					break;
				case PerkType.Hyperactivity:
					ply.EnableEffect<MovementBoost>(60);
					ply.ChangeEffectIntensity<MovementBoost>(30);
					break;
				case PerkType.EthericVitality:
					Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 4f, 75f);
					break;
				case PerkType.ConcealedPresence:
					ply.EnableEffect<Invisible>(30);
					break;
				case PerkType.BlissfulUnawareness:
					Timing.RunCoroutine(BlissfulUnawarenessCoroutine(ply).CancelWith(ply.GameObject));
					break;
				case PerkType.ExtrasensoryPerception:
					//coming soon:tm:
					break;
				case PerkType.EtherealIntervention:
					if (!EtherealInterventionPlayers.Contains(ply))
						EtherealInterventionPlayers.Add(ply);
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

        public static void ClearConsumedPerks(Player player)
        {
            if (FoundationFortune.Singleton.ConsumedPerks.ContainsKey(player)) FoundationFortune.Singleton.ConsumedPerks[player].Clear();
        }

        public static IEnumerator<float> BlissfulUnawarenessCoroutine(Player ply)
		{
			ply.EnableEffect<MovementBoost>(120);
			ply.ChangeEffectIntensity<MovementBoost>(25);
			Log.Debug("Blissful Unawareness 1st coroutine started.");

			yield return Timing.WaitForSeconds(80f);

			Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 5f, 50f);
            Log.Debug("Blissful Unawareness 1st coroutine finished.");
			Log.Debug("Blissful Unawareness 2nd coroutine started.");
			PlayerVoiceChatSettings BlissfulAwarenessSettings = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
			    .FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.BlissfulUnawareness);
			ply.EnableEffect<SoundtrackMute>(50f);
			AudioPlayer.PlaySpecialAudio(ply, BlissfulAwarenessSettings.AudioFile, BlissfulAwarenessSettings.Volume, BlissfulAwarenessSettings.Loop, BlissfulAwarenessSettings.VoiceChat);

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
					FoundationFortune.Singleton.serverEvents.AddBounty(reviver, FoundationFortune.Singleton.Config.RevivalBountyKillReward, TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.RevivalBountyTimeSeconds));
				}
				foreach (var ply in Player.List.Where(p => !p.IsNPC))
				{
					FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RevivalSuccess.Replace("%rolecolor%", reviver.Role.Color.ToHex())
					    .Replace("%nickname%", reviver.Nickname)
					    .Replace("%target%", targetToRevive.Nickname), 3f);
				}
				return true;
			}
			else
			{
				FoundationFortune.Singleton.serverEvents.EnqueueHint(reviver, FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName), 3f);
				return false;
			}
		}
	}
}
