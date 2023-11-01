﻿using FoundationFortune.API.Database;
using MEC;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using FoundationFortune.API.NPCs;
using System.Linq;
using Exiled.Events.EventArgs.Server;
using PlayerRoles;
using System.Collections.Generic;
using FoundationFortune.API.Perks;
using Exiled.API.Extensions;
using Exiled.API.Enums;
using FoundationFortune.API.Models;
using PlayerStatsSystem;

// ReSharper disable once CheckNamespace
// STFU!!!!!!!!!!!!!!!!
namespace FoundationFortune.API.HintSystem
{
	/// <summary>
	/// the leg
	/// </summary>
	public partial class ServerEvents
	{
		private readonly Dictionary<(LeadingTeam, Team?), (PlayerTeamConditions, string)> teamConditionsMap = new()
	   {
			{(LeadingTeam.FacilityForces, Team.FoundationForces), (PlayerTeamConditions.Winning, "#0080FF")},
			{(LeadingTeam.FacilityForces, Team.Scientists), (PlayerTeamConditions.Winning, "#0080FF")},
			{(LeadingTeam.ChaosInsurgency, Team.ChaosInsurgency), (PlayerTeamConditions.Winning, "#00FF00")},
			{(LeadingTeam.ChaosInsurgency, Team.ClassD), (PlayerTeamConditions.Winning, "#FF00FF")},
			{(LeadingTeam.Anomalies, Team.SCPs), (PlayerTeamConditions.Winning, "#FF0000")},
			{(LeadingTeam.Draw, null), (PlayerTeamConditions.Draw, "#808080")}
	   };

		public void RoundStart()
		{
			CoroutineManager.KillCoroutines();
			CoroutineManager.Coroutines.Add(Timing.RunCoroutine(UpdateMoneyAndHints()));
			CoroutineManager.Coroutines.Add(Timing.RunCoroutine(NPCHelperMethods.UpdateNpcDirection()));

			if (FoundationFortune.Singleton.Config.MoneyExtractionSystem)
			{
				int extractionTime = Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
				Timing.CallDelayed(extractionTime, StartExtractionEvent);
				Log.Info($"Round Started. The first extraction will commence at T-{extractionTime} Seconds.");
			}

			InitializeWorkstationPositions();
			
			if (FoundationFortune.Singleton.Config.FoundationFortuneNPCs) InitializeFoundationFortuneNPCs();
			else Log.Debug("Foundation Fortune NPCs have been disabled. Not spawning any.");
		}

		public void RegisterInDatabase(VerifiedEventArgs ev)
		{
			if (FoundationFortune.Singleton.MusicBotPairs.All(pair => pair.Player.UserId != ev.Player.UserId) && !ev.Player.IsNPC) MusicBot.SpawnMusicBot(ev.Player);

			var existingPlayer = PlayerDataRepository.GetPlayerById(ev.Player.UserId);
			if (existingPlayer == null && !ev.Player.IsNPC)
			{
				var newPlayer = new PlayerData
				{
					Username = ev.Player.DisplayNickname,
					UserId = ev.Player.UserId,
					MoneyOnHold = 0,
					MoneySaved = 0,
					HintLimit = 5,
					HintMinmode = true,
					HintSystem = true,
					HintAdmin = false,
					HintSize = 25,
					HintAnim = HintAnim.None,
					HintAlign = HintAlign.Center
				};
				PlayerDataRepository.InsertPlayer(newPlayer);
			}
		}

		public void EtherealInterventionHandler(DyingEventArgs ev)
		{
			if (FoundationFortune.Singleton.ConsumedPerks.TryGetValue(ev.Player, out var perks))
			{
				if (perks.ContainsKey(PerkType.BlissfulUnawareness)) perks.Remove(PerkType.BlissfulUnawareness);
				if (perks.ContainsKey(PerkType.EtherealIntervention)) perks.Remove(PerkType.EtherealIntervention);
			}

			if (!PerkSystem.EtherealInterventionPlayers.Contains(ev.Player)) return;
			ev.IsAllowed = false;
			RoleTypeId role = ev.Player.Role.Type;
			Room room = Room.List.Where(r => r.Zone == ev.Player.Zone & !FoundationFortune.Singleton.Config.ForbiddenEtherealInterventionRoomTypes.Contains(r.Type)).GetRandomValue();

			Timing.CallDelayed(0.1f, delegate
			{
				ev.Player.Role.Set(role, RoleSpawnFlags.None);
				ev.Player.Teleport(room);
			});
		}


		public void EtherealInterventionSpawn(SpawnedEventArgs ev)
		{
			if (!PerkSystem.EtherealInterventionPlayers.Contains(ev.Player)) return;
			Timing.CallDelayed(0.2f, delegate
			{
				PlayerVoiceChatSettings EtherealInterventionAudio = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
					.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.EtherealIntervention);
				if (EtherealInterventionAudio != null)
					AudioPlayer.PlaySpecialAudio(ev.Player, EtherealInterventionAudio.AudioFile,
						EtherealInterventionAudio.Volume, EtherealInterventionAudio.Loop,
						EtherealInterventionAudio.VoiceChat);
			});
			PerkSystem.EtherealInterventionPlayers.Remove(ev.Player);

			if (NPCHelperMethods.IsFoundationFortuneNPC(ev.Player.ReferenceHub)) Round.ChaosTargetCount -= 1;
		}

		public void KillingReward(DiedEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			var bountiedPlayer = BountiedPlayers.FirstOrDefault(bounty => bounty.Player == ev.Player && bounty.IsBountied);

			PerkSystem.ClearConsumedPerks(ev.Player);

			if (PerkSystem.ViolentImpulsesPlayers.Contains(ev.Attacker)) ev.Attacker.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(15f).DecayRate = 2.0f;

			if (bountiedPlayer != null && ev.Attacker != null)
			{
				foreach (Player ply in Player.List.Where(p => p != ev.Attacker && !p.IsDead))
				{
					var globalKillHint = FoundationFortune.Singleton.Translation.BountyFinished
						.Replace("%victim%", ev.Player.Nickname)
						.Replace("%attacker%", ev.Attacker.Nickname)
						.Replace("%victimcolor%", ev.Player.Role.Color.ToHex())
						.Replace("%attackercolor%", ev.Attacker.Role.Color.ToHex())
						.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());
					EnqueueHint(ply, globalKillHint, config.MaxHintAge);
				}

				if (ev.Attacker != null && ev.Attacker != ev.Player)
				{
					var killHint = FoundationFortune.Singleton.Translation.BountyKill
						.Replace("%victim%", ev.Player.Nickname)
						.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());

					EnqueueHint(ev.Attacker, killHint, config.MaxHintAge);
					PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, bountiedPlayer.Value, false, true, false);
				}
				StopBounty(ev.Player);
			}
			else if (bountiedPlayer != null && ev.Attacker == null)
			{
				var killHint = FoundationFortune.Singleton.Translation.BountyPlayerDied
					.Replace("%victim%", ev.Player.Nickname);

				foreach (Player ply in Player.List.Where(p => !p.IsNPC)) EnqueueHint(ply, killHint, config.MaxHintAge);
				StopBounty(ev.Player);
			}

			if (ev.Attacker != null && ev.Attacker != ev.Player && config.KillEvent)
			{
				var killHint = FoundationFortune.Singleton.Translation.Kill.Replace("%victim%", ev.Player.Nickname);
				EnqueueHint(ev.Attacker, killHint, config.MaxHintAge);
			}

			if (ev.Player.IsNPC) Round.ChaosTargetCount += 1;
		}

		public void EscapingReward(EscapingEventArgs ev)
		{
			if (!ev.IsAllowed) return;
			var config = FoundationFortune.Singleton.Config;
			if (config.EscapeEvent)
			{
				EnqueueHint(ev.Player, $"{FoundationFortune.Singleton.Translation.Escape}", config.MaxHintAge);
				PlayerDataRepository.ModifyMoney(ev.Player.UserId, config.EscapeRewards);
			}
		}
		
		public void SellingItem(DroppingItemEventArgs ev)
		{
			var translation = FoundationFortune.Singleton.Translation;
			if (!IsPlayerOnSellingWorkstation(ev.Player) && !IsPlayerNearSellingBot(ev.Player))
			{
				ev.IsAllowed = true;
				return;
			}

			Npc sellingBot = GetNearestSellingBot(ev.Player);

			if (IsPlayerNearSellingBot(ev.Player))
			{
				if (!confirmSell.ContainsKey(ev.Player.UserId))
				{
					foreach (var sellableItem in FoundationFortune.Singleton.Config.SellableItems.Where(sellableItem => ev.Item.Type == sellableItem.ItemType))
					{
						itemsBeingSold[ev.Player.UserId] = (ev.Item, sellableItem.Price);

						confirmSell[ev.Player.UserId] = true;
						dropTimestamp[ev.Player.UserId] = Time.time;
						ev.IsAllowed = false;
						return;
					}
				}

				if (confirmSell.TryGetValue(ev.Player.UserId, out bool isConfirming) && dropTimestamp.TryGetValue(ev.Player.UserId, out float dropTime))
				{
					if (isConfirming && Time.time - dropTime <= FoundationFortune.Singleton.Config.SellingConfirmationTime)
					{
						if (itemsBeingSold.TryGetValue(ev.Player.UserId, out var soldItemData))
						{
							Item soldItem = soldItemData.item;
							int price = soldItemData.price;

							if (soldItem == ev.Item)
							{
								NPCVoiceChatSettings sellVoiceChatSettings = FoundationFortune.Singleton.Config.FFNPCVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == NPCVoiceChatUsageType.Selling);
								if (sellVoiceChatSettings != null)
									AudioPlayer.PlayAudio(sellingBot, sellVoiceChatSettings.AudioFile,
										sellVoiceChatSettings.Volume, sellVoiceChatSettings.Loop,
										sellVoiceChatSettings.VoiceChat);
								var str = FoundationFortune.Singleton.Translation.SellSuccess
									.Replace("%price%", price.ToString())
									.Replace("%itemName%", FoundationFortune.Singleton.Config.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName);

								PlayerDataRepository.ModifyMoney(ev.Player.UserId, price, false, true, false);
								EnqueueHint(ev.Player, str, 3f);
								ev.Player.RemoveItem(ev.Item);
							}
							else EnqueueHint(ev.Player, translation.SaleCancelled, 3f);

							itemsBeingSold.Remove(ev.Player.UserId);
							ev.IsAllowed = true;
							return;
						}
					}
				}
			}
			else
			{
				ev.IsAllowed = true;
				NPCVoiceChatSettings wrongBotSettings = FoundationFortune.Singleton.Config.FFNPCVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == NPCVoiceChatUsageType.WrongBuyingBot);
				if (wrongBotSettings != null)
					AudioPlayer.PlayAudio(sellingBot, wrongBotSettings.AudioFile, wrongBotSettings.Volume,
						wrongBotSettings.Loop, wrongBotSettings.VoiceChat);
				EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.WrongBot, 3f);
			}
			ev.IsAllowed = true;
		}

		public void RoundEnded(RoundEndedEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			if (!config.RoundEndEvent) return;
			LeadingTeam leadingTeam = ev.LeadingTeam;
			int winningAmount = config.RoundEndRewards.TryGetValue(PlayerTeamConditions.Winning, out var winningValue) ? winningValue : 0;
			int losingAmount = config.RoundEndRewards.TryGetValue(PlayerTeamConditions.Losing, out var losingValue) ? losingValue : 0;
			int drawAmount = config.RoundEndRewards.TryGetValue(PlayerTeamConditions.Draw, out var drawValue) ? drawValue : 0;

			foreach (Player ply in Player.List.Where(p => p.IsAlive && !p.IsNPC))
			{
				Team playerTeam = ply.Role.Team;
				string teamColor = "#FFFFFF";
				PlayerTeamConditions teamCondition = PlayerTeamConditions.Losing;

				if (teamConditionsMap.TryGetValue((leadingTeam, playerTeam), out var conditionTuple) || (leadingTeam == LeadingTeam.Draw && conditionTuple.Item1 == PlayerTeamConditions.Draw))
				{
					teamCondition = conditionTuple.Item1;
					teamColor = conditionTuple.Item2;
				}
				
				switch (teamCondition)
				{
					case PlayerTeamConditions.Winning:
						PlayerDataRepository.ModifyMoney(ply.UserId, winningAmount);
						EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndWin
							.Replace("%winningFactionColor%", teamColor)
							.Replace("%winningAmount%", winningAmount.ToString()), config.MaxHintAge);
						break;
					case PlayerTeamConditions.Losing:
						PlayerDataRepository.ModifyMoney(ply.UserId, losingAmount);
						EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndLoss
							.Replace("%losingFactionColor%", teamColor)
							.Replace("%losingAmount%", losingAmount.ToString()), config.MaxHintAge);
						break;
					case PlayerTeamConditions.Draw:
						PlayerDataRepository.ModifyMoney(ply.UserId, drawAmount);
						EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndDraw
							.Replace("%drawFactionColor%", teamColor)
							.Replace("%drawAmount%", drawAmount.ToString()), config.MaxHintAge);
						break;
				}
			}
			CoroutineManager.KillCoroutines();
		}

		public void ShootingWeapon(ShootingEventArgs ev) { if (PerkSystem.ViolentImpulsesPlayers.Contains(ev.Player)) ev.Firearm.Recoil = new(FoundationFortune.Singleton.Config.ViolentImpulsesRecoilAnimationTime, FoundationFortune.Singleton.Config.ViolentImpulsesRecoilZAxis, FoundationFortune.Singleton.Config.ViolentImpulsesRecoilFovKick, FoundationFortune.Singleton.Config.ViolentImpulsesRecoilUpKick, FoundationFortune.Singleton.Config.ViolentImpulsesRecoilSideKick); }
		public void DestroyMusicBots(LeftEventArgs ev) { if (FoundationFortune.Singleton.MusicBotPairs.Find(pair => pair.Player.Nickname == ev.Player.Nickname) != null) MusicBot.RemoveMusicBot(ev.Player.Nickname); }
        public void HurtingPlayer(HurtingEventArgs ev) { if (PerkSystem.ViolentImpulsesPlayers.Contains(ev.Attacker)) ev.Amount *= FoundationFortune.Singleton.Config.ViolentImpulsesDamageMultiplier; }
        public void RoundRestart() => ClearIndexations();
        public void FuckYourAbility(ActivatingSenseEventArgs ev) { if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false; }
		public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev) { if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false; }
        public void PreventBotsFromSpawningInWaves(RespawningTeamEventArgs ev) { foreach (Player player in ev.Players.Where(p => p.IsNPC)) ev.Players.Remove(player); }
    }
}
