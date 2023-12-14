using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Server;
using FoundationFortune.API.Common.Enums.NPCs;
using FoundationFortune.API.Common.Enums.Player;
using FoundationFortune.API.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Common.Models.Items;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using FoundationFortune.API.Features.Perks;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FoundationFortune.EventHandlers;

/// <summary>
/// the leg
/// </summary>
public class ExiledEventHandlers
{
	private readonly Dictionary<(LeadingTeam, Team?), (PlayerTeamConditions, string)> _teamConditionsMap = new()
	{
		{ (LeadingTeam.FacilityForces, Team.FoundationForces), (PlayerTeamConditions.Winning, "#0080FF") },
		{ (LeadingTeam.FacilityForces, Team.Scientists), (PlayerTeamConditions.Winning, "#0080FF") },
		{ (LeadingTeam.ChaosInsurgency, Team.ChaosInsurgency), (PlayerTeamConditions.Winning, "#00FF00") },
		{ (LeadingTeam.ChaosInsurgency, Team.ClassD), (PlayerTeamConditions.Winning, "#FF00FF") },
		{ (LeadingTeam.Anomalies, Team.SCPs), (PlayerTeamConditions.Winning, "#FF0000") },
		{ (LeadingTeam.Draw, Team.Dead), (PlayerTeamConditions.Draw, "#808080") },
	};
		
	#region EXILED Events
	public void RoundStart()
	{
		if (FoundationFortune.FoundationFortuneNpcSettings.FoundationFortuneNpCs) NPCInitialization.Start();
		if (FoundationFortune.SellableItemsList.UseSellingWorkstation) SellingWorkstations.Start();
		
		CoroutineManager.StopAllCoroutines();
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(FoundationFortune.Instance.HintSystem.HintSystemCoroutine()));
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(NPCHelperMethods.UpdateNpcDirection()));

		if (!FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem) return;
		int extractionTime = Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
		Timing.CallDelayed(extractionTime, ExtractionSystem.StartExtractionEvent);
		DirectoryIterator.Log($"Round Started. The first extraction will commence at T-{extractionTime} Seconds.", LogLevel.Info);
	}

	public void RegisterInDatabase(VerifiedEventArgs ev)
	{
		if (NPCHelperMethods.MusicBotPairs.All(pair => pair.Player.UserId != ev.Player.UserId) && !ev.Player.IsNPC) MusicBot.SpawnMusicBot(ev.Player);
		if (!ev.Player.IsNPC) QuestRotation.GetShuffledQuestsForUser(ev.Player.UserId);
		var existingPlayer = PlayerDataRepository.GetPlayerById(ev.Player.UserId);
		if (existingPlayer == null && !ev.Player.IsNPC)
		{
			var newPlayer = new PlayerData
			{
				UserId = ev.Player.UserId,
				MoneyOnHold = 0,
				MoneySaved = 0,
				Exp = 0,
				Level = 0,
				PrestigeLevel = 0,
				HintAgeSeconds = 5,
				SellingConfirmationTime = 5,
				ActiveAbilityActivationTime = 5,
				HintLimit = 5,
				HintMinmode = true,
				HintSystem = true,
				HintAdmin = false,
				HintSize = 20,
				HintAlign = HintAlign.Center
			};
			PlayerDataRepository.InsertPlayer(newPlayer);
		}
	}

	public void DyingEvent(DyingEventArgs ev)
	{
		AudioPlayer.StopAudio(ev.Player);
		var bountiedPlayer = BountySystem.BountiedPlayers.FirstOrDefault(bounty => bounty.Player == ev.Player && bounty.IsBountied);

		if (bountiedPlayer != null && ev.Attacker != null)
		{
			foreach (Player ply in Player.List.Where(p => p != ev.Attacker && !p.IsDead))
			{
				var globalKillHint = FoundationFortune.Instance.Translation.BountyFinished
					.Replace("%victim%", ev.Player.Nickname)
					.Replace("%attacker%", ev.Attacker.Nickname)
					.Replace("%victimcolor%", ev.Player.Role.Color.ToHex())
					.Replace("%attackercolor%", ev.Attacker.Role.Color.ToHex())
					.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());
				FoundationFortune.Instance.HintSystem.BroadcastHint(ply, globalKillHint);
			}

			if (ev.Attacker != null && ev.Attacker != ev.Player)
			{
				var killHint = FoundationFortune.Instance.Translation.BountyKill
					.Replace("%victim%", ev.Player.Nickname)
					.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());

				FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Attacker, killHint);
				PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, bountiedPlayer.Value, false, true, false);
			}

			BountySystem.StopBounty(ev.Player);
		}
		
		else if (bountiedPlayer != null && ev.Attacker == null)
		{
			var killHint = FoundationFortune.Instance.Translation.BountyPlayerDied
				.Replace("%victim%", ev.Player.Nickname);

			foreach (Player ply in Player.List.Where(p => !p.IsNPC)) FoundationFortune.Instance.HintSystem.BroadcastHint(ply, killHint);
			BountySystem.StopBounty(ev.Player);
		}
		
		if (PerkSystem.HasPerk(ev.Player, PerkType.EtherealIntervention))
		{
			ev.IsAllowed = false;
			RoleTypeId role = ev.Player.Role.Type;
			Room room = Room.List.Where(r => r.Zone == ev.Player.Zone & !FoundationFortune.PerkSystemSettings.ForbiddenEtherealInterventionRoomTypes.Contains(r.Type)).GetRandomValue();
			QuestSystem.UpdateQuestProgress(ev.Player, QuestType.UseEtherealIntervention, 1);

			Timing.CallDelayed(0.1f, delegate
			{
				ev.Player.Role.Set(role, SpawnReason.None, RoleSpawnFlags.None);
				ev.Player.Teleport(room);
			});
		}
	}

	public void EtherealInterventionSpawn(SpawnedEventArgs ev)
	{
		if (PerkSystem.HasPerk(ev.Player, PerkType.EtherealIntervention) && ev.Reason == SpawnReason.None)
		{
			IPerk etherealInterventionPerk = PerkType.EtherealIntervention.ToPerk();
			IPerk blissfulAgonyPerk = PerkType.BlissfulAgony.ToPerk();
		
			if (PerkSystem.HasPerk(ev.Player, PerkType.BlissfulAgony)) PerkSystem.RemovePerk(ev.Player, blissfulAgonyPerk);

			Timing.CallDelayed(0.2f, () =>
			{
				var etherealInterventionAudio = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EtherealIntervention);
				PerkSystem.RemovePerk(ev.Player, etherealInterventionPerk);
				if (etherealInterventionAudio != null) AudioPlayer.PlayTo(ev.Player, etherealInterventionAudio.AudioFile, etherealInterventionAudio.Volume, etherealInterventionAudio.Loop, true);
			});
		}
	}

	public void KillingReward(DiedEventArgs ev)
	{
		PerkSystem.ClearConsumedPerks(ev.Player);
		if (PerkSystem.HasPerk(ev.Attacker, PerkType.ViolentImpulses)) ev.Attacker.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(15f).DecayRate = 2.0f;

		QuestSystem.UpdateQuestProgress(ev.Attacker, QuestType.GetAKillstreak, 1);
		if (ev.Attacker != null && ev.Player.Role == RoleTypeId.Scp0492) QuestSystem.UpdateQuestProgress(ev.Attacker, QuestType.KillZombies, 1);
		
		if (ev.Attacker != null && ev.Attacker != ev.Player && FoundationFortune.MoneyXPRewards.KillEvent)
		{
			var killHint = FoundationFortune.Instance.Translation.Kill
				.Replace("%victim%", ev.Player.Nickname)
				.Replace("%attacker%", ev.Attacker.Nickname)
				.Replace("%killMoneyReward%", FoundationFortune.MoneyXPRewards.KillEventMoneyRewards.ToString())
				.Replace("%killXPReward%", FoundationFortune.MoneyXPRewards.KillEventXpRewards.ToString())
				.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, FoundationFortune.MoneyXPRewards.KillEventMoneyRewards, false, true, false);
			PlayerDataRepository.SetExperience(ev.Attacker.UserId, FoundationFortune.MoneyXPRewards.KillEventXpRewards);
			FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Attacker, killHint);
		}
	}

	public void EscapingReward(EscapingEventArgs ev)
	{
		if (!ev.IsAllowed) return;
		if (FoundationFortune.MoneyXPRewards.EscapeEvent)
		{
			var escapeHint = FoundationFortune.Instance.Translation.Escape
				.Replace("%escapeMoneyReward%", FoundationFortune.MoneyXPRewards.KillEventMoneyRewards.ToString())
				.Replace("%escapeXPReward%", FoundationFortune.MoneyXPRewards.KillEventXpRewards.ToString())
				.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerDataRepository.TransferMoney(ev.Player.UserId, true);
			PlayerDataRepository.ModifyMoney(ev.Player.UserId, FoundationFortune.MoneyXPRewards.EscapeEventMoneyRewards);
			PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.EscapeEventXpRewards);
			FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, escapeHint);
		}
	}

	public void ActivatingPerk(TogglingNoClipEventArgs ev)
	{
		if (!PerkSystem.ConsumedActivePerks.TryGetValue(ev.Player, out var activePerks)) return;
		if (ev.Player.IsBypassModeEnabled) return;
			
		foreach (var activePerk in activePerks.Keys)
		{
			if (!FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.ContainsKey(ev.Player.UserId))
			{
				FoundationFortune.Instance.HintSystem.ConfirmActivatePerk[ev.Player.UserId] = true;
				FoundationFortune.Instance.HintSystem.ActivatePerkTimestamp[ev.Player.UserId] = Time.time;
				ev.IsAllowed = false;
				break;
			}

			if (FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.TryGetValue(ev.Player.UserId, out bool isConfirming))
			{
				if (!FoundationFortune.Instance.HintSystem.ActivatePerkTimestamp.TryGetValue(ev.Player.UserId, out float toggleTime)) continue;
				if (!isConfirming || !(Time.time - toggleTime <= activePerk.TimeUntilNextActivation)) continue;
				activePerk.StartActivePerkAbility(ev.Player);
				FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.Remove(ev.Player.UserId);
				break;
			}

			ev.IsAllowed = true;
		}
	}

	public void SellingItem(DroppingItemEventArgs ev)
	{
		if (!SellingWorkstations.IsPlayerOnSellingWorkstation(ev.Player) && !NPCHelperMethods.IsPlayerNearSellingBot(ev.Player))
		{
			ev.IsAllowed = true;
			return;
		}
		
		if (NPCHelperMethods.IsPlayerNearSellingBot(ev.Player))
		{
			if (!FoundationFortune.Instance.HintSystem.ConfirmSell.ContainsKey(ev.Player.UserId))
			{
				foreach (var sellableItem in FoundationFortune.SellableItemsList.SellableItems.Where(sellableItem => ev.Item.Type == sellableItem.ItemType))
				{
					FoundationFortune.Instance.HintSystem.ItemsBeingSold[ev.Player.UserId] = (ev.Item, sellableItem.Price);
					FoundationFortune.Instance.HintSystem.ConfirmSell[ev.Player.UserId] = true;
					FoundationFortune.Instance.HintSystem.DropTimestamp[ev.Player.UserId] = Time.time;
					ev.IsAllowed = false;
					return;
				}
			}

			if (FoundationFortune.Instance.HintSystem.ConfirmSell.TryGetValue(ev.Player.UserId, out bool isConfirming) && FoundationFortune.Instance.HintSystem.DropTimestamp.TryGetValue(ev.Player.UserId, out float dropTime))
			{
				if (isConfirming && Time.time - dropTime <= PlayerDataRepository.GetSellingConfirmationTime(ev.Player.UserId))
				{
					if (FoundationFortune.Instance.HintSystem.ItemsBeingSold.TryGetValue(ev.Player.UserId, out var soldItemData))
					{
						Item soldItem = soldItemData.item;

						if (soldItem == ev.Item)
						{
							SellableItem sellableItem = FoundationFortune.SellableItemsList.SellableItems.Find(x => x.ItemType == ev.Item.Type);
							EventHelperMethods.RegisterOnSoldItem(ev.Player, sellableItem, ev.Item);
							EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(ev.Player, NpcType.Selling, NpcUsageOutcome.SellSuccess);
						}
						else FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, FoundationFortune.Instance.Translation.SaleCancelled);

						FoundationFortune.Instance.HintSystem.ItemsBeingSold.Remove(ev.Player.UserId);
						ev.IsAllowed = true;
						return;
					}
				}
			}
		}
		else
		{
			ev.IsAllowed = true;
			EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(ev.Player, NpcType.Selling, NpcUsageOutcome.WrongBot);
			FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, FoundationFortune.Instance.Translation.WrongBot);
		}

		ev.IsAllowed = true;
	}

	public void RoundEnded(RoundEndedEventArgs ev)
	{
		var config = FoundationFortune.MoneyXPRewards;
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

			if (_teamConditionsMap.TryGetValue((leadingTeam, playerTeam), out var conditionTuple) || (leadingTeam == LeadingTeam.Draw && conditionTuple.Item1 == PlayerTeamConditions.Draw))
			{
				teamCondition = conditionTuple.Item1;
				teamColor = conditionTuple.Item2;
			}

			switch (teamCondition)
			{
				case PlayerTeamConditions.Winning:
					PlayerDataRepository.ModifyMoney(ply.UserId, winningAmount);
					PlayerDataRepository.SetExperience(ply.UserId, winningAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndWin
						.Replace("%winningFactionColor%", teamColor)
						.Replace("%winningAmount%", winningAmount.ToString())
						.Replace("%expReward%", winningAmount.ToString())
						.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
					break;
				case PlayerTeamConditions.Losing:
					PlayerDataRepository.ModifyMoney(ply.UserId, losingAmount);
					PlayerDataRepository.SetExperience(ply.UserId, losingAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndLoss
						.Replace("%losingFactionColor%", teamColor)
						.Replace("%losingAmount%", losingAmount.ToString())
						.Replace("%expReward%", losingAmount.ToString())
						.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
					break;
				case PlayerTeamConditions.Draw:
					PlayerDataRepository.ModifyMoney(ply.UserId, drawAmount);
					PlayerDataRepository.SetExperience(ply.UserId, drawAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndDraw
						.Replace("%drawFactionColor%", teamColor)
						.Replace("%drawAmount%", drawAmount.ToString())
						.Replace("%expReward%", drawAmount.ToString())
						.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
					break;
				default: throw new ArgumentOutOfRangeException();
			}
		}
	}

	public void BlinkingRequestEvent(BlinkingRequestEventArgs ev)
	{
	}
		
	public void AddingTargetEvent(AddingTargetEventArgs ev)
	{
		if (NPCHelperMethods.IsFoundationFortuneNpc(ev.Target.ReferenceHub)) ev.IsAllowed = false;
	}

	public void ShootingWeapon(ShootingEventArgs ev)
	{
		if (PerkSystem.HasPerk(ev.Player, PerkType.ViolentImpulses)) ev.Firearm.Recoil = FoundationFortune.Instance.FoundationFortuneEventHandlers.RecoilSettings;
	}

	public void DestroyMusicBots(LeftEventArgs ev)
	{
		if (NPCHelperMethods.MusicBotPairs.Find(pair => pair.Player.Nickname == ev.Player.Nickname) != null) MusicBot.RemoveMusicBot(ev.Player.Nickname);
	}

	public void HurtingPlayer(HurtingEventArgs ev)
	{
		if (PerkSystem.HasPerk(ev.Attacker, PerkType.ViolentImpulses)) ev.Amount *= FoundationFortune.PerkSystemSettings.ViolentImpulsesDamageMultiplier;
	}

	public void FuckYourAbility(ActivatingSenseEventArgs ev)
	{
		if (ev.Target.IsNPC) ev.IsAllowed = false;
	}

	public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev)
	{
		if (ev.Target.IsNPC) ev.IsAllowed = false;
	}

	public void PreventBotsFromSpawningInWaves(RespawningTeamEventArgs ev)
	{
		foreach (Player player in ev.Players.Where(p => NPCHelperMethods.IsFoundationFortuneNpc(p.ReferenceHub))) ev.Players.Remove(player);
	}

	public void RoundRestart()
	{ 
		QuestRotation.IncrementQuestRotationNumber();
		IndexationMethods.ClearIndexations();
	}
	public void ThrownGhostlight(ThrownProjectileEventArgs ev) => QuestSystem.UpdateQuestProgress(ev.Player, QuestType.ThrowGhostlights, 1);
	public void UnlockingGenerator(UnlockingGeneratorEventArgs ev) => QuestSystem.UpdateQuestProgress(ev.Player, QuestType.UnlockGenerators, 1);
	#endregion
}