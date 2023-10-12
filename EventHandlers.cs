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

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private CoroutineHandle moneyHintCoroutine;

		public void RoundStart()
		{
            moneyHintCoroutine = Timing.RunCoroutine(UpdateMoneyAndHints());

			FoundationFortune.PlayerLimits.Clear();
			FoundationFortune.Singleton.BuyingBotIndexation.Clear();
			buyingBotPositions.Clear();

			if (FoundationFortune.Singleton.Config.MoneyExtractionSystem)
			{
                int nextExtractionTime = Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
                Timing.CallDelayed(nextExtractionTime, () => StartExtractionEvent());
                Log.Info($"Round Started. The first extraction will commence at T-{nextExtractionTime} Seconds.");
            }

            InitializeWorkstationPositions();
			InitializeBuyingBots();
		}

        public void RoundEnding(EndingRoundEventArgs ev)
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
					HintMinmode = false,
					DisabledHintSystem = false,
					HintSize = 100,
					HintOpacity = 100,
					HintAnim = HintAnim.None,
					HintAlign = HintAlign.Center
				};
				PlayerDataRepository.InsertPlayer(newPlayer);
			}
		}

		public void KillingReward(DiedEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			var bountiedPlayer = BountiedPlayers.FirstOrDefault(bounty => bounty.Player == ev.Player && bounty.IsBountied);

			if (bountiedPlayer != null && ev.Attacker != null)
			{
				foreach (Player ply in Player.List.Where(p => p != ev.Attacker && !p.IsDead))
				{
					var globalKillHint = FoundationFortune.Singleton.Translation.BountyFinished
						.Replace("%victim%", ev.Player.Nickname ?? "Null Hunted Player")
						.Replace("%attacker%", ev.Attacker.Nickname)
						.Replace("%victimcolor%", ev.Player.Role.Color.ToHex() ?? "#FFFFFF")
						.Replace("%attackercolor%", ev.Attacker.Role.Color.ToHex() ?? "#FFFFFF")
						.Replace("%bountyPrice%", bountiedPlayer.Value.ToString());
					EnqueueHint(ply, globalKillHint, config.MaxHintAge);
				}

				if (ev.Attacker != null && ev.Attacker != ev.Player)
				{
					var killHint = FoundationFortune.Singleton.Translation.BountyKill
						.Replace("%victim%", ev.Player?.Nickname)
						.Replace("%bountyPrice%", bountiedPlayer?.Value.ToString());

					EnqueueHint(ev.Attacker, killHint, config.MaxHintAge);
				}
				StopBounty(ev.Player);
			}
			else if (bountiedPlayer != null && ev.Attacker == null)
			{
				var killHint = FoundationFortune.Singleton.Translation.BountyPlayerDied
					.Replace("%victim%", ev.Player?.Nickname);

				foreach (Player ply in Player.List.Where(p => !p.IsNPC)) EnqueueHint(ply, killHint, config.MaxHintAge);
				StopBounty(ev.Player);
			}

			if (ev.Attacker != null && ev.Attacker != ev.Player && config.KillRewardScpOnly)
			{
				var killHint = FoundationFortune.Singleton.Translation.Kill.Replace("%victim%", ev.Player?.Nickname);
				EnqueueHint(ev.Attacker, killHint, config.MaxHintAge);
			}

			if (ev.Player?.IsNPC == true) RoundSummary.singleton.Network_chaosTargetCount += 2;
		}

		public void EscapingReward(EscapingEventArgs ev)
		{
			if (!ev.IsAllowed) return;
			var config = FoundationFortune.Singleton.Config;
			EnqueueHint(ev.Player, $"{FoundationFortune.Singleton.Translation.Escape}", config.MaxHintAge, HintAnim.Right);
			PlayerDataRepository.ModifyMoney(ev.Player.UserId, config.EscapeReward, false, false, true);
		}

		public void SellingItem(DroppingItemEventArgs ev)
		{
			var translation = FoundationFortune.Singleton.Translation;
			if (!IsPlayerOnSellingWorkstation(ev.Player) && !IsPlayerOnBuyingBotRadius(ev.Player))
			{
				ev.IsAllowed = true;
				return;
			}

			Npc buyingbot = GetBuyingBotNearPlayer(ev.Player);

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
								VoiceChatSettings buyVoiceChatSettings = FoundationFortune.Singleton.Config.VoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == VoiceChatUsageType.Buying);
								BuyingBot.PlayAudio(buyingbot, buyVoiceChatSettings.AudioFile, buyVoiceChatSettings.Volume, buyVoiceChatSettings.Loop, buyVoiceChatSettings.VoiceChat);
								EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.SellSuccess.Replace("%price%", price.ToString()).Replace("%itemName%", FoundationFortune.Singleton.Config.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName), 3f);
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
				VoiceChatSettings wrongBotSettings = FoundationFortune.Singleton.Config.VoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == VoiceChatUsageType.WrongBuyingBot);
				BuyingBot.PlayAudio(buyingbot, wrongBotSettings.AudioFile, wrongBotSettings.Volume, wrongBotSettings.Loop, wrongBotSettings.VoiceChat);
				EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.WrongBot, 3f);
			}
			ev.IsAllowed = true;
		}

        public string GetConfirmationTimeLeft(Player ply)
        {
            if (dropTimestamp.ContainsKey(ply.UserId))
            {
                float timeLeft = FoundationFortune.Singleton.Config.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
                if (timeLeft > 0)
                {
                    return timeLeft.ToString("F0");
                }
            }
            return "0";
        }

        public void SpawningNpc(SpawningEventArgs ev) {if (ev.Player.IsNPC) RoundSummary.singleton.Network_chaosTargetCount -= 1;}
		public void RoundRestart() {if (moneyHintCoroutine.IsRunning) Timing.KillCoroutines(moneyHintCoroutine);}
		public void FuckYourAbility(ActivatingSenseEventArgs ev) {if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false;}
		public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev) {if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false;}
	}
}
