using FoundationFortune.API.Database;
using MEC;
using PlayerRoles;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using FoundationFortune.API.NPCs;
using System.Linq;
using Exiled.Events.EventArgs.Server;
using System.Collections.Generic;
using FoundationFortune.Commands.Buy;

namespace FoundationFortune.Events
{
	public enum HintAlign
	{
		Center,
		Right,
		Left
	}

	public partial class ServerEvents
	{
		private CoroutineHandle moneyHintCoroutine;

		public void RoundStart()
		{
			BuyCommand.PlayerLimits.Clear();

			moneyHintCoroutine = Timing.RunCoroutine(UpdateMoneyAndHints());

			FoundationFortune.Singleton.BuyingBotIndexation.Clear();
			buyingBotPositions.Clear();

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

		public void RoundEnded(RoundEndedEventArgs ev)
		{
			Log.Debug("Round End");

			if (moneyHintCoroutine.IsRunning) Timing.KillCoroutines(moneyHintCoroutine);
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
					HintAlign = HintAlign.Center
				};
				PlayerDataRepository.InsertPlayer(newPlayer);
			}
		}

		public void SpawningNpc(SpawningEventArgs ev)
		{
			if (ev.Player.IsNPC)
			{
				RoundSummary.singleton.Network_chaosTargetCount -= 1;
			}
		}

		public void KillingReward(DiedEventArgs ev)
		{
			if (ev.Attacker != null && ev.Attacker != ev.Player && ev.Attacker.IsScp)
			{
				var config = FoundationFortune.Singleton.Config;
				var killHint = config.KillHint.Replace("[victim]", ev.Player.Nickname);
				EnqueueHint(ev.Attacker, killHint, config.KillReward, config.MaxHintAge, config.KillRewardTransfer, config.KillRewardTransferAll);
			}
			if (ev.Player.IsNPC)
			{
				RoundSummary.singleton.Network_chaosTargetCount += 2;
			}
		}

		public void EscapingReward(EscapingEventArgs ev)
		{
			var config = FoundationFortune.Singleton.Config;
			EnqueueHint(ev.Player, $"{config.EscapeHint}", config.EscapeReward, config.MaxHintAge, config.EscapeRewardTransfer, config.EscapeRewardTransferAll);
		}

		public void SellingItem(DroppingItemEventArgs ev)
		{
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
								EnqueueHint(ev.Player, $"<b><size=24><color=green>+{price}$</color> Sold {FoundationFortune.Singleton.Config.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName}.</color></b></size>", price, 3, false, false);
								ev.Player.RemoveItem(ev.Item);
							}
							else
							{
								EnqueueHint(ev.Player, "<b><size=24><color=red>Item changed. Sale canceled.</color></b></size>", 0, 3, false, false);
							}

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
                EnqueueHint(ev.Player, "<b><size=24><color=red>This is not a selling bot.</color></b></size>", 0, 3, false, false);
			}
			ev.IsAllowed = true;
		}

		public void FuckYourAbility(ActivatingSenseEventArgs ev) {if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false;}
		public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev) {if (ev.Target != null && ev.Target.IsNPC) ev.IsAllowed = false;}
    }
}
