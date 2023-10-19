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
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using PlayerRoles;
using System.Collections.Generic;
using FoundationFortune.API.Perks;
using Utf8Json.Resolvers.Internal;
using Exiled.API.Extensions;
using InventorySystem;
using Exiled.API.Enums;
using PluginAPI.Roles;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Doors;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private CoroutineHandle moneyHintCoroutine;
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
			moneyHintCoroutine = Timing.RunCoroutine(UpdateMoneyAndHints());

			if (FoundationFortune.Singleton.Config.MoneyExtractionSystem)
			{
				int nextExtractionTime = Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
				Timing.CallDelayed(nextExtractionTime, () => StartExtractionEvent());
				Log.Info($"Round Started. The first extraction will commence at T-{nextExtractionTime} Seconds.");
			}

			ClearIndexations();
			InitializeWorkstationPositions();
			InitializeFoundationFortuneNPCs();
		}

		public void RoundEnding(EndingRoundEventArgs ev) //what is this
		{
			IEnumerable<Player> players = Player.List.Where(p => !p.IsNPC);
			IEnumerable<Player> alivePlayers = players.Where(p => p.IsAlive);
			int chaos = alivePlayers.Count(p => p.IsCHI || p.Role.Type == RoleTypeId.ClassD);
			int mtf = alivePlayers.Count(p => p.IsNTF || p.Role.Type == RoleTypeId.Scientist);
			int scps = alivePlayers.Count(p => p.IsScp);

			if (!Round.IsLocked && !Round.IsEnded)
			{
				if (alivePlayers.Count() <= 1) ev.IsRoundEnded = true;
				if (chaos >= 1 && mtf == 0 && scps == 0 || mtf >= 1 && chaos == 0 && scps == 0) ev.IsRoundEnded = true;
			}
		}

		public void RegisterInDatabase(VerifiedEventArgs ev)
		{
			var existingPlayer = PlayerDataRepository.GetPlayerById(ev.Player.UserId);
			if (existingPlayer == null && !ev.Player.IsNPC)
			{
				var newPlayer = new PlayerData
				{
					Username = ev.Player.DisplayNickname,
					UserId = ev.Player.UserId,
					MoneyOnHold = 0,
					MoneySaved = 0,
					HintMinmode = true,
					DisabledHintSystem = true,
					IsAdmin = false,
					HintSize = 25,
					HintOpacity = 100,
					HintAnim = HintAnim.None,
					HintAlign = HintAlign.Center
				};
				PlayerDataRepository.InsertPlayer(newPlayer);
			}
		}

		public void EtherealInterventionHandler(DyingEventArgs ev)
		{
			if (FoundationFortune.Singleton.ConsumedPerks.ContainsKey(ev.Player)) FoundationFortune.Singleton.ConsumedPerks[ev.Player].Clear();

			if (!PerkSystem.EtherealInterventionPlayers.Contains(ev.Player)) return;
			else
			{
				ev.IsAllowed = false;
				RoleTypeId role = ev.Player.Role.Type;
				Room room = Room.List.Where(r => r.Zone == ev.Player.Zone & !FoundationFortune.Singleton.Config.ForbiddenRooms.Contains(r.Type)).GetRandomValue();
				List<Item> items = new();
				Dictionary<AmmoType, ushort> ammos = new();
				ev.ItemsToDrop = new List<Item>();

				foreach (Item item in ev.Player.Items) items.Add(item);
				foreach (var kvp in ev.Player.Ammo) ammos.Add(kvp.Key.GetAmmoType(), kvp.Value);

				Timing.CallDelayed(0.1f, delegate
				{
					ev.Player.Role.Set(role, RoleSpawnFlags.None);
					ev.Player.Teleport(room);
				});
			}
		}

		public void EtherealInterventionSpawn(SpawnedEventArgs ev)
		{
			if (!PerkSystem.EtherealInterventionPlayers.Contains(ev.Player)) return;
			else
			{
				Timing.CallDelayed(0.2f, delegate
				{
					PlayerVoiceChatSettings EtherealInterventionAudio = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
								 .FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.EtherealIntervention);
					AudioPlayer.PlaySpecialAudio(ev.Player, EtherealInterventionAudio.AudioFile, EtherealInterventionAudio.Volume, EtherealInterventionAudio.Loop, EtherealInterventionAudio.VoiceChat);
				});
				PerkSystem.EtherealInterventionPlayers.Remove(ev.Player);
			}
		}

		public void KillingReward(DiedEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			var bountiedPlayer = BountiedPlayers.FirstOrDefault(bounty => bounty.Player == ev.Player && bounty.IsBountied);

			PerkSystem.ClearConsumedPerks(ev.Player);

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
						.Replace("%bountyPrice%", bountiedPlayer?.Value.ToString());

					EnqueueHint(ev.Attacker, killHint, config.MaxHintAge);
					PlayerDataRepository.ModifyMoney(ev.Attacker.UserId, bountiedPlayer.Value);
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

			if (ev.Player?.IsNPC == true) RoundSummary.singleton.Network_chaosTargetCount += 2;
		}

		public void EscapingReward(EscapingEventArgs ev)
		{
			if (!ev.IsAllowed) return;
			var config = FoundationFortune.Singleton.Config;
			if (config.EscapeEvent)
			{
				EnqueueHint(ev.Player, $"{FoundationFortune.Singleton.Translation.Escape}", config.MaxHintAge, HintAnim.Right);
				PlayerDataRepository.ModifyMoney(ev.Player.UserId, config.EscapeRewards, false, false, true);
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

			Npc buyingbot = GetNearestSellingBot(ev.Player);

			if (IsPlayerNearSellingBot(ev.Player))
			{
				if (!confirmSell.ContainsKey(ev.Player.UserId))
				{
					foreach (var sellableItem in FoundationFortune.Singleton.Config.SellableItems)
					{
						if (ev.Item.Type == sellableItem.ItemType)
						{
							itemsBeingSold[ev.Player.UserId] = (ev.Item, sellableItem.Price);

							confirmSell[ev.Player.UserId] = true;
							dropTimestamp[ev.Player.UserId] = Time.time;
							ev.IsAllowed = false;
							return;
						}
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
								NPCVoiceChatSettings buyVoiceChatSettings = FoundationFortune.Singleton.Config.FFNPCVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == NPCVoiceChatUsageType.Buying);
								AudioPlayer.PlayAudio(buyingbot, buyVoiceChatSettings.AudioFile, buyVoiceChatSettings.Volume, buyVoiceChatSettings.Loop, buyVoiceChatSettings.VoiceChat);
								string str = FoundationFortune.Singleton.Translation.SellSuccess
									.Replace("%price%", price.ToString())
									.Replace("%itemName%", FoundationFortune.Singleton.Config.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName);

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
				AudioPlayer.PlayAudio(buyingbot, wrongBotSettings.AudioFile, wrongBotSettings.Volume, wrongBotSettings.Loop, wrongBotSettings.VoiceChat);
				EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.WrongBot, 3f);
			}
			ev.IsAllowed = true;
		}

		public void RoundEnded(RoundEndedEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			if (config.RoundEndEvent)
			{
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

					int reward = config.RoundEndRewards.TryGetValue(teamCondition, out var value) ? value : 0;

					switch (teamCondition)
					{
						case PlayerTeamConditions.Winning:
							PlayerDataRepository.ModifyMoney(ply.UserId, winningAmount, false, false, true);
							EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndWin
							    .Replace("%winningFactionColor%", teamColor)
							    .Replace("%winningAmount%", winningAmount.ToString()), config.MaxHintAge);
							break;
						case PlayerTeamConditions.Losing:
							PlayerDataRepository.ModifyMoney(ply.UserId, losingAmount, false, false, true);
							EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndLoss
							    .Replace("%losingFactionColor%", teamColor)
							    .Replace("%losingAmount%", losingAmount.ToString()), config.MaxHintAge);
							break;
						case PlayerTeamConditions.Draw:
							PlayerDataRepository.ModifyMoney(ply.UserId, drawAmount, false, false, true);
							EnqueueHint(ply, FoundationFortune.Singleton.Translation.RoundEndDraw
							    .Replace("%drawFactionColor%", teamColor)
							    .Replace("%drawAmount%", drawAmount.ToString()), config.MaxHintAge);
							break;
					}
				}
			}
		}

		public void SpawningNpc(SpawningEventArgs ev) { if (ev.Player.IsNPC) RoundSummary.singleton.Network_chaosTargetCount -= 1; }
		public void RoundRestart() { if (moneyHintCoroutine.IsRunning) Timing.KillCoroutines(moneyHintCoroutine); }
		public void FuckYourAbility(ActivatingSenseEventArgs ev) { if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false; }
		public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev) { if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false; }
	}
}
