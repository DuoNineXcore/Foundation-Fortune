using FoundationFortune.API.Database;
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
using System.Globalization;
using CameraShaking;
using Discord;
using Exiled.API.Extensions;
using Exiled.API.Enums;
using FoundationFortune.API.Events;
using FoundationFortune.API.Events.EventArgs;
using FoundationFortune.API.EventSystems;
using FoundationFortune.API.Items.PerkItems;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.Models.Enums.Player;
using FoundationFortune.API.NPCs.NpcTypes;
using FoundationFortune.API.Systems;
using PlayerStatsSystem;

// ReSharper disable once CheckNamespace
// STFU!!!!!!!!!!!!!!!!
namespace FoundationFortune.API;

/// <summary>
/// the leg
/// </summary>
public class EventHandlers
{
	private readonly Dictionary<(LeadingTeam, Team?), (PlayerTeamConditions, string)> teamConditionsMap = new()
	{
		{ (LeadingTeam.FacilityForces, Team.FoundationForces), (PlayerTeamConditions.Winning, "#0080FF") },
		{ (LeadingTeam.FacilityForces, Team.Scientists), (PlayerTeamConditions.Winning, "#0080FF") },
		{ (LeadingTeam.ChaosInsurgency, Team.ChaosInsurgency), (PlayerTeamConditions.Winning, "#00FF00") },
		{ (LeadingTeam.ChaosInsurgency, Team.ClassD), (PlayerTeamConditions.Winning, "#FF00FF") },
		{ (LeadingTeam.Anomalies, Team.SCPs), (PlayerTeamConditions.Winning, "#FF0000") },
		{ (LeadingTeam.Draw, null), (PlayerTeamConditions.Draw, "#808080") }
	};

	private RecoilSettings recoilSettings;
	
	#region foundation fortune exiled events because doing it otherwise fucking sucks
	public void UsedFoundationFortuneNPC(UsedFoundationFortuneNPCEventArgs ev)
	{
		var voiceChatUsageType = AudioPlayer.GetNpcVoiceChatUsageType(ev.Type, ev.Outcome);

		if (voiceChatUsageType != NpcVoiceChatUsageType.None)
		{
			var npcVoiceChatSettings = AudioPlayer.GetVoiceChatSettings(voiceChatUsageType);
			if (npcVoiceChatSettings != null) AudioPlayer.PlayAudio(ev.NPC, npcVoiceChatSettings.AudioFile, npcVoiceChatSettings.Volume, npcVoiceChatSettings.Loop, npcVoiceChatSettings.VoiceChat);
		}
	}
	
	public void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
	{
		string str = FoundationFortune.Singleton.Translation.DrankPerkBottle
			.Replace("%type%", ev.PerkType.ToString())
			.Replace("%xpReward%", FoundationFortune.MoneyXpRewards.UsingPerkXPRewards.ToString())
			.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
		
		if (!FoundationFortune.Singleton.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks))
		{
			playerPerks = new Dictionary<PerkType, int>();
			FoundationFortune.Singleton.ConsumedPerks[ev.Player] = playerPerks;
		}
		
		if (playerPerks.TryGetValue(ev.PerkType, out var count)) playerPerks[ev.PerkType] = count + 1;
		else playerPerks[ev.PerkType] = 1;
			
		PerkSystem.GrantPerk(ev.Player, ev.PerkType);
		FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, str);
		PerkBottle.PerkBottles.Remove(ev.Item.Serial);
	}

	public void BoughtItem(BoughtItemEventArgs ev)
	{
		string str = FoundationFortune.Singleton.Translation.BuyItemSuccess
			.Replace("%itemAlias%", ev.BuyableItem.Alias)
			.Replace("%itemPrice%", ev.BuyableItem.Price.ToString())
			.Replace("%xpReward%", FoundationFortune.MoneyXpRewards.BuyingItemXPRewards.ToString())
			.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture)); //oh my god i dont fucking care about culture

		FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, str);

		ev.Player.AddItem(ev.BuyableItem.ItemType);

		PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyableItem.Price, true, true, false);
		PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.BuyingItemXPRewards);
		FoundationFortuneAPI.AddToPlayerLimits(ev.Player, ev.BuyableItem);
	}

	public void BoughtPerk(BoughtPerkEventArgs ev)
	{
		string str = FoundationFortune.Singleton.Translation.BuyPerkSuccess
			.Replace("%perkAlias%", ev.BuyablePerk.Alias)
			.Replace("%perkPrice%", ev.BuyablePerk.Price.ToString())
			.Replace("%xpReward%", FoundationFortune.MoneyXpRewards.BuyingPerkXPRewards.ToString())
			.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

		FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, str);

		PerkBottle.GivePerkBottle(ev.Player, ev.BuyablePerk.PerkType);

		PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyablePerk.Price, true, true, false);
		PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.BuyingPerkXPRewards);
		FoundationFortuneAPI.AddToPlayerLimits(ev.Player, ev.BuyablePerk);
	}

	public void SoldItem(SoldItemEventArgs ev)
	{
		var str = FoundationFortune.Singleton.Translation.SellItemSuccess
			.Replace("%price%", ev.SellableItem.Price.ToString())
			.Replace("%itemName%", FoundationFortune.SellableItemsList.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName)
			.Replace("%xpReward%", FoundationFortune.MoneyXpRewards.SellingXPRewards.ToString())
			.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

		FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, str);

		ev.Player.RemoveItem(ev.Item);

		PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.SellableItem.Price, false, true, false);
		PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.SellingXPRewards);
		FoundationFortuneAPI.AddToPlayerLimits(ev.Player, ev.SellableItem);
	}

	#endregion
	
	#region EXILED Events
	public void RoundStart()
	{
		recoilSettings = new(
			FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilAnimationTime,
			FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilZAxis,
			FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilFovKick,
			FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilUpKick,
			FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilSideKick);

		if (FoundationFortune.FoundationFortuneNpcSettings.FoundationFortuneNPCs) FoundationFortuneAPI.InitializeFoundationFortuneNPCs();
		if (FoundationFortune.Singleton.Config.UseSellingWorkstation) FoundationFortuneAPI.InitializeWorkstationPositions();
		
		CoroutineManager.StopAllCoroutines();
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(FoundationFortune.Singleton.FoundationFortuneAPI.HintSystemCoroutine()));
		CoroutineManager.Coroutines.Add(Timing.RunCoroutine(NPCHelperMethods.UpdateNpcDirection()));

		if (FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem)
		{
			int extractionTime = Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
			Timing.CallDelayed(extractionTime, ServerExtractionSystem.StartExtractionEvent);
			FoundationFortune.Log($"Round Started. The first extraction will commence at T-{extractionTime} Seconds.", LogLevel.Info);
		}
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
				HintSeconds = 3,
				HintAge = 4,
				HintMinmode = true,
				HintSystem = true,
				HintAdmin = false,
				HintSize = 20,
				HintAnim = HintAnim.None,
				HintAlign = HintAlign.Center
			};
			PlayerDataRepository.InsertPlayer(newPlayer);
		}
	}

	public void DyingEvent(DyingEventArgs ev)
	{
		if (!ev.Player.IsNPC) AudioPlayer.StopAudio(ev.Player);
		var bountiedPlayer = ServerBountySystem.BountiedPlayers.FirstOrDefault(bounty => bounty.Player == ev.Player && bounty.IsBountied);

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
				FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, globalKillHint);
			}

			if (ev.Attacker != null && ev.Attacker != ev.Player)
			{
				var killHint = FoundationFortune.Singleton.Translation.BountyKill
					.Replace("%victim%", ev.Player.Nickname)
					.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());

				FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Attacker, killHint);
				PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, bountiedPlayer.Value, false, true, false);
			}

			ServerBountySystem.StopBounty(ev.Player);
		}
		
		else if (bountiedPlayer != null && ev.Attacker == null)
		{
			var killHint = FoundationFortune.Singleton.Translation.BountyPlayerDied
				.Replace("%victim%", ev.Player.Nickname);

			foreach (Player ply in Player.List.Where(p => !p.IsNPC)) FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, killHint);
			ServerBountySystem.StopBounty(ev.Player);
		}
		
		if (PerkSystem.HasPerk(ev.Player, PerkType.EtherealIntervention))
		{
			if (FoundationFortune.Singleton.ConsumedPerks.TryGetValue(ev.Player, out var perks))
			{
				if (perks.ContainsKey(PerkType.BlissfulUnawareness)) PerkSystem.RemovePerk(ev.Player, PerkType.BlissfulUnawareness);
				if (perks.ContainsKey(PerkType.EtherealIntervention)) PerkSystem.RemovePerk(ev.Player, PerkType.EtherealIntervention);
			}
			
			ev.IsAllowed = false;
			RoleTypeId role = ev.Player.Role.Type;
			Room room = Room.List.Where(r => r.Zone == ev.Player.Zone & !FoundationFortune.PerkSystemSettings.ForbiddenEtherealInterventionRoomTypes.Contains(r.Type)).GetRandomValue();

			Timing.CallDelayed(0.1f, delegate
			{
				ev.Player.Role.Set(role, SpawnReason.LateJoin, RoleSpawnFlags.None);
				ev.Player.Teleport(room);
			});
		}
	}

	public void EtherealInterventionSpawn(SpawnedEventArgs ev)
	{
		if (PerkSystem.HasPerk(ev.Player, PerkType.EtherealIntervention) && ev.Reason == SpawnReason.LateJoin)
		{
			Timing.CallDelayed(0.2f, delegate
			{
				var etherealInterventionAudio = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EtherealIntervention);
				if (etherealInterventionAudio != null) AudioPlayer.PlaySpecialAudio(ev.Player, etherealInterventionAudio.AudioFile, etherealInterventionAudio.Volume, etherealInterventionAudio.Loop, etherealInterventionAudio.VoiceChat);
			});
			PerkSystem.RemovePerk(ev.Player, PerkType.EtherealIntervention);	
		}

		if (NPCHelperMethods.IsFoundationFortuneNPC(ev.Player.ReferenceHub)) Round.ChaosTargetCount -= 1;
	}

	public void KillingReward(DiedEventArgs ev)
	{
		PerkSystem.ClearConsumedPerks(ev.Player);
		
		if (PerkSystem.HasPerk(ev.Player, PerkType.EthericVitality)) PerkSystem.RemovePerk(ev.Player, PerkType.EthericVitality);
		if (PerkSystem.HasPerk(ev.Player, PerkType.HyperactiveBehavior)) PerkSystem.RemovePerk(ev.Player, PerkType.HyperactiveBehavior);
		if (PerkSystem.HasPerk(ev.Attacker, PerkType.ViolentImpulses)) ev.Attacker.ReferenceHub.playerStats.GetModule<AhpStat>().ServerAddProcess(15f).DecayRate = 2.0f;

		if (ev.Attacker != null && ev.Attacker != ev.Player && FoundationFortune.MoneyXpRewards.KillEvent)
		{
			var killHint = FoundationFortune.Singleton.Translation.Kill
				.Replace("%victim%", ev.Player.Nickname)
				.Replace("%attacker%", ev.Attacker.Nickname)
				.Replace("%killMoneyReward%", FoundationFortune.MoneyXpRewards.KillEventMoneyRewards.ToString())
				.Replace("%killXPReward%", FoundationFortune.MoneyXpRewards.KillEventXPRewards.ToString())
				.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, FoundationFortune.MoneyXpRewards.KillEventMoneyRewards, false, true, false);
			PlayerDataRepository.SetExperience(ev.Attacker.UserId, FoundationFortune.MoneyXpRewards.KillEventXPRewards);
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Attacker, killHint);
		}

		if (ev.Player.IsNPC) RoundSummary.singleton.ChaosTargetCount += 1;
	}

	public void EscapingReward(EscapingEventArgs ev)
	{
		if (!ev.IsAllowed) return;
		if (FoundationFortune.MoneyXpRewards.EscapeEvent)
		{
			var escapeHint = FoundationFortune.Singleton.Translation.Escape
				.Replace("%escapeMoneyReward%", FoundationFortune.MoneyXpRewards.KillEventMoneyRewards.ToString())
				.Replace("%escapeXPReward%", FoundationFortune.MoneyXpRewards.KillEventXPRewards.ToString())
				.Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));
			
			PlayerDataRepository.ModifyMoney(ev.Player.UserId, FoundationFortune.MoneyXpRewards.EscapeEventMoneyRewards);
			PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.EscapeEventXPRewards);
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, escapeHint);
		}
	}

	public void SellingItem(DroppingItemEventArgs ev)
	{
		if (NPCHelperMethods.IsPlayerNearSellingBot(ev.Player))
		{
			if (!FoundationFortune.Singleton.FoundationFortuneAPI.confirmSell.ContainsKey(ev.Player.UserId))
			{
				foreach (var sellableItem in FoundationFortune.SellableItemsList.SellableItems.Where(sellableItem => ev.Item.Type == sellableItem.ItemType))
				{
					FoundationFortune.Singleton.FoundationFortuneAPI.itemsBeingSold[ev.Player.UserId] = (ev.Item, sellableItem.Price);
					FoundationFortune.Singleton.FoundationFortuneAPI.confirmSell[ev.Player.UserId] = true;
					FoundationFortune.Singleton.FoundationFortuneAPI.dropTimestamp[ev.Player.UserId] = Time.time;
					ev.IsAllowed = false;
					return;
				}
			}

			if (FoundationFortune.Singleton.FoundationFortuneAPI.confirmSell.TryGetValue(ev.Player.UserId, out bool isConfirming) && FoundationFortune.Singleton.FoundationFortuneAPI.dropTimestamp.TryGetValue(ev.Player.UserId, out float dropTime))
			{
				if (isConfirming && Time.time - dropTime <= FoundationFortune.SellableItemsList.SellingConfirmationTime)
				{
					if (FoundationFortune.Singleton.FoundationFortuneAPI.itemsBeingSold.TryGetValue(ev.Player.UserId, out var soldItemData))
					{
						Item soldItem = soldItemData.item;

						if (soldItem == ev.Item)
						{
							SellableItem sellableItem = FoundationFortune.SellableItemsList.SellableItems.Find(x => x.ItemType == ev.Item.Type);
							EventHelperMethods.RegisterOnSoldItem(ev.Player, sellableItem, ev.Item);
							EventHelperMethods.RegisterOnUsedFoundationFortuneNPC(ev.Player, NpcType.Selling, NpcUsageOutcome.SellSuccess);
						}
						else FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.SaleCancelled);


						FoundationFortune.Singleton.FoundationFortuneAPI.itemsBeingSold.Remove(ev.Player.UserId);
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
			FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.WrongBot);
		}

		ev.IsAllowed = true;
	}

	public void RoundEnded(RoundEndedEventArgs ev)
	{
		var config = FoundationFortune.MoneyXpRewards;
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
					FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndWin
						.Replace("%winningFactionColor%", teamColor)
						.Replace("%winningAmount%", winningAmount.ToString()));
					break;
				case PlayerTeamConditions.Losing:
					PlayerDataRepository.ModifyMoney(ply.UserId, losingAmount);
					FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndLoss
						.Replace("%losingFactionColor%", teamColor)
						.Replace("%losingAmount%", losingAmount.ToString()));
					break;
				case PlayerTeamConditions.Draw:
					PlayerDataRepository.ModifyMoney(ply.UserId, drawAmount);
					FoundationFortune.Singleton.FoundationFortuneAPI.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndDraw
						.Replace("%drawFactionColor%", teamColor)
						.Replace("%drawAmount%", drawAmount.ToString()));
					break;
			}
		}
	}

	public void ShootingWeapon(ShootingEventArgs ev)
	{
		if (PerkSystem.HasPerk(ev.Player, PerkType.ViolentImpulses)) ev.Firearm.Recoil = recoilSettings;
	}

	public void DestroyMusicBots(LeftEventArgs ev)
	{
		if (FoundationFortune.Singleton.MusicBotPairs.Find(pair => pair.Player.Nickname == ev.Player.Nickname) != null) MusicBot.RemoveMusicBot(ev.Player.Nickname);
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
		foreach (Player player in ev.Players.Where(p => p.IsNPC)) ev.Players.Remove(player);
	}
	
	public void RoundRestart() => FoundationFortuneAPI.ClearIndexations();
	#endregion
}

