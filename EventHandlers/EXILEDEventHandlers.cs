using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using Exiled.Events.EventArgs.Scp096;
using Exiled.Events.EventArgs.Scp173;
using Exiled.Events.EventArgs.Server;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Core.Common.Enums.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Common.Models.Databases;
using FoundationFortune.API.Core.Common.Models.Items;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
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
		if (FoundationFortune.FoundationFortuneNpcSettings.FoundationFortuneNPCs) NPCInitialization.Start();
		if (FoundationFortune.SellableItemsList.UseSellingWorkstation) SellingWorkstations.Start();
		
		CoroutineManager.StopAllCoroutines();
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(FoundationFortune.Instance.HintSystem.HintSystemCoroutine()));
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(NPCHelperMethods.UpdateNpcDirection()));
		QuestRotationRepository.IncrementQuestRotationNumber();
		
		if (!FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem) return;
		int extractionTime = Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
		Timing.CallDelayed(extractionTime, ExtractionSystem.StartExtractionEvent);
		DirectoryIterator.Log($"Round Started. The first extraction will commence at T-{extractionTime} Seconds.", LogLevel.Info);
	}

	public void RegisterInDatabase(VerifiedEventArgs ev)
	{
		if (NPCHelperMethods.MusicBotPairs.All(pair => pair.Player.UserId != ev.Player.UserId) && !ev.Player.IsNPC) MusicBot.SpawnMusicBot(ev.Player);
		if (!ev.Player.IsNPC) QuestRotationRepository.GetShuffledQuestsForUser(ev.Player.UserId);

		var existingPlayerStats = PlayerStatsRepository.GetPlayerById(ev.Player.UserId);
		var existingPlayerSettings = PlayerSettingsRepository.GetPlayerById(ev.Player.UserId);

		if (existingPlayerStats == null && !ev.Player.IsNPC)
		{
			var newPlayerStats = new API.Core.Common.Models.Databases.PlayerStats
			{
				UserId = ev.Player.UserId,
				MoneyOnHold = 0,
				MoneySaved = 0,
				Exp = 0,
				Level = 0,
				PrestigeLevel = 0
			};

			PlayerStatsRepository.InsertPlayer(newPlayerStats);
		}

		if (existingPlayerSettings == null && !ev.Player.IsNPC)
		{
			var newPlayerSettings = new PlayerSettings
			{
				UserId = ev.Player.UserId,
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

			PlayerSettingsRepository.InsertPlayer(newPlayerSettings);
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
				PlayerStatsRepository.ModifyMoney(ev.Attacker.UserId, bountiedPlayer.Value, false, true, false);
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
		
		if (!PerkSystem.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks)) return;
		var perksToRemove = new List<IPerk>();
		foreach (var perk in playerPerks.Keys)
		{
			switch (perk.PerkType)
			{
				case PerkType.GracefulSaint:
					if (playerPerks.ContainsKey(PerkType.BlissfulAgony.ToPerk()))
					{
						perksToRemove.Add(PerkType.BlissfulAgony.ToPerk());
						DirectoryIterator.Log($"Removed Blissful Agony due to Ethereal Intervention for player {ev.Player.Nickname}.", LogLevel.Info);
					}
					break;
				default:
					perksToRemove.Add(perk);
					break;
			}
		}

		foreach (var perkToRemove in perksToRemove)
		{
			PerkSystem.RemovePerk(ev.Player, perkToRemove);
			DirectoryIterator.Log($"Removed perk {perkToRemove.Alias} for player {ev.Player.Nickname}. they died lol.", LogLevel.Info);
		}
	}

	public void KillingReward(DiedEventArgs ev)
	{
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
				.Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerStatsRepository.ModifyMoney(ev.Attacker.UserId, FoundationFortune.MoneyXPRewards.KillEventMoneyRewards, false, true, false);
			PlayerStatsRepository.SetExperience(ev.Attacker.UserId, FoundationFortune.MoneyXPRewards.KillEventXpRewards);
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
				.Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerStatsRepository.TransferMoney(ev.Player.UserId, true);
			PlayerStatsRepository.ModifyMoney(ev.Player.UserId, FoundationFortune.MoneyXPRewards.EscapeEventMoneyRewards);
			PlayerStatsRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.EscapeEventXpRewards);
			FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, escapeHint);
		}
	}

	public void ActivatingPerk(TogglingNoClipEventArgs ev)
	{
		if (ev.Player.IsBypassModeEnabled) return;

		if (PerkSystem.ConsumedCooldownActivePerks.TryGetValue(ev.Player, out var cooldownPerks) && cooldownPerks.Any())
		{
			PerkSystem.HandlePerkActivation(ev, cooldownPerks.Keys.First(),
				cooldownPerks.Keys.First().Cooldown.Seconds);
			return;
		}

		if (PerkSystem.ConsumedMeteredActivePerks.TryGetValue(ev.Player, out var meteredPerks) && meteredPerks.Any())
			PerkSystem.HandlePerkActivation(ev, meteredPerks.Keys.First(), meteredPerks.Keys.First().Cooldown.Seconds);
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
					FoundationFortune.Instance.HintSystem.DropTimestamp[ev.Player.UserId] = DateTime.UtcNow;
					ev.IsAllowed = false;
					return;
				}
			}

			if (FoundationFortune.Instance.HintSystem.ConfirmSell.TryGetValue(ev.Player.UserId, out bool isConfirming) && FoundationFortune.Instance.HintSystem.DropTimestamp.TryGetValue(ev.Player.UserId, out DateTime dropTime))
			{
				if (isConfirming && (DateTime.UtcNow - dropTime).TotalSeconds <= PlayerSettingsRepository.GetSellingConfirmationTime(ev.Player.UserId))
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
					PlayerStatsRepository.ModifyMoney(ply.UserId, winningAmount);
					PlayerStatsRepository.SetExperience(ply.UserId, winningAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndWin
						.Replace("%winningFactionColor%", teamColor)
						.Replace("%winningAmount%", winningAmount.ToString())
						.Replace("%expReward%", winningAmount.ToString())
						.Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
					break;
				case PlayerTeamConditions.Losing:
					PlayerStatsRepository.ModifyMoney(ply.UserId, losingAmount);
					PlayerStatsRepository.SetExperience(ply.UserId, losingAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndLoss
						.Replace("%losingFactionColor%", teamColor)
						.Replace("%losingAmount%", losingAmount.ToString())
						.Replace("%expReward%", losingAmount.ToString())
						.Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
					break;
				case PlayerTeamConditions.Draw:
					PlayerStatsRepository.ModifyMoney(ply.UserId, drawAmount);
					PlayerStatsRepository.SetExperience(ply.UserId, drawAmount);
					FoundationFortune.Instance.HintSystem.BroadcastHint(ply, FoundationFortune.Instance.Translation.RoundEndDraw
						.Replace("%drawFactionColor%", teamColor)
						.Replace("%drawAmount%", drawAmount.ToString())
						.Replace("%expReward%", drawAmount.ToString())
						.Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ply.UserId).ToString(CultureInfo.InvariantCulture)));
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

	public void DestroyMusicBots(LeftEventArgs ev)
	{
		if (NPCHelperMethods.MusicBotPairs.Find(pair => pair.Player.Nickname == ev.Player.Nickname) != null) MusicBot.RemoveMusicBot(ev.Player.Nickname);
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
		QuestRotationRepository.IncrementQuestRotationNumber();
		IndexationMethods.ClearIndexations();
	}
	public void ThrownGhostlight(ThrownProjectileEventArgs ev) => QuestSystem.UpdateQuestProgress(ev.Player, QuestType.ThrowGhostlights, 1);
	public void UnlockingGenerator(UnlockingGeneratorEventArgs ev) => QuestSystem.UpdateQuestProgress(ev.Player, QuestType.UnlockGenerators, 1);
	#endregion
}