﻿using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using MEC;
using UnityEngine;
using System.Collections.Generic;
using FoundationFortune.API.Database;
using System.Text;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Classes.Events;

// ReSharper disable once CheckNamespace
// STFU!!!!!!!!!!!!!!!!
namespace FoundationFortune.API
{
	public partial class FoundationFortuneAPI
	{
		private bool isExtractionPointActive;
		public bool limitReached;
		private RoomType activeExtractionRoom;
		private int extractionCount;
		private int nextExtractionTime;
		private float extractionStartTime;
		private Dictionary<Player, ExtractionTimerData> extractionTimers = new();

		private static bool IsPlayerInExtractionRoom(Player player, RoomType roomType)
		{
			if (player.CurrentRoom != null) return player.CurrentRoom.Type == roomType;
			return false;
		}

		private void StartExtractionEvent()
		{
			if (!FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem) return;
			if (extractionCount >= FoundationFortune.MoneyExtractionSystemSettings.ExtractionLimit)
			{
				limitReached = true;
				return;
			}

			extractionStartTime = Time.time;
			activeExtractionRoom = FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointRooms[UnityEngine.Random.Range(0, FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointRooms.Count)];

			isExtractionPointActive = true;
			Log.Debug($"Extraction point activated in room: {activeExtractionRoom}. It will be active for {FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration} seconds.");

			extractionCount++;
			CoroutineManager.Coroutines.Add(Timing.CallDelayed(FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration, () => DeactivateExtractionPoint(true)));
		}

		public void StartExtractionEvent(RoomType room, float duration)
		{
			extractionStartTime = Time.time;
			activeExtractionRoom = room;

			isExtractionPointActive = true;
			Log.Debug($"Extraction point activated in room: {activeExtractionRoom}. It will be active for T-{duration} seconds.");

			CoroutineManager.Coroutines.Add(Timing.CallDelayed(duration, () => DeactivateExtractionPoint(false)));
		}

		public void DeactivateExtractionPoint(bool restart)
		{
			if (!isExtractionPointActive) return;

			if (restart)
			{
				isExtractionPointActive = false;
				if (extractionCount >= FoundationFortune.MoneyExtractionSystemSettings.ExtractionLimit) return;
				nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
				Log.Debug($"Extraction point in room {activeExtractionRoom} deactivated. Next extraction in T-{nextExtractionTime} Seconds.");
				CoroutineManager.Coroutines.Add(Timing.CallDelayed(nextExtractionTime, StartExtractionEvent));
			}
            else isExtractionPointActive = false;
        }

		private void UpdateExtractionMessages(Player ply, ref StringBuilder hintMessage)
		{
			if (!isExtractionPointActive) return;
			
			if (IsPlayerInExtractionRoom(ply, activeExtractionRoom))
			{
				int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);

				if (totalMoneyOnHold <= 0) hintMessage.Append($"{FoundationFortune.Singleton.Translation.ExtractionNoMoney}");
				else
				{
					if (!extractionTimers.ContainsKey(ply)) StartExtractionTimer(ply);

					ExtractionTimerData timerData = extractionTimers[ply];
					float elapsedTime = Time.time - timerData.StartTime;
					TimeSpan timeLeft = TimeSpan.FromSeconds(10 - elapsedTime);

					string waitTimerHint = FoundationFortune.Singleton.Translation.ExtractionTimer
						.Replace("%time%", timeLeft.ToString(@"ss"));
					hintMessage.Append(waitTimerHint);
				}
			}
			else
			{
				CancelExtractionTimer(ply);

				string extractionHint = FoundationFortune.Singleton.Translation.ExtractionEvent
					.Replace("%room%", activeExtractionRoom.ToString())
					.Replace("%time%", TimeSpan.FromSeconds(FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration - (Time.time - extractionStartTime)).ToString(@"hh\:mm\:ss"));
				hintMessage.Append(extractionHint);
			}
		}

		private void StartExtractionTimer(Player player)
		{
			CoroutineHandle timerHandle = Timing.RunCoroutine(ExtractionTimerCoroutine(player));
			CoroutineManager.Coroutines.Add(timerHandle);
			ExtractionTimerData timerData = new()
			{
				CoroutineHandle = timerHandle,
				StartTime = Time.time
			};
			extractionTimers[player] = timerData;
		}

		private IEnumerator<float> ExtractionTimerCoroutine(Player player)
		{
			float startTime = Time.time;
			while (Time.time - startTime < 10f)
			{
				if (!IsPlayerInExtractionRoom(player, activeExtractionRoom))
				{
					Log.Debug($"Extraction timer canceled for player {player.UserId}");
					CancelExtractionTimer(player);
					yield break;
				}
				yield return Timing.WaitForSeconds(1f);
			}
			ExtractMoney(player);
		}
		
		private static void ExtractMoney(Player player)
		{
			int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);
			if (totalMoneyOnHold <= 0) return;
			
			PlayerDataRepository.TransferMoney(player.UserId, true);
			Log.Debug($"Transferred {totalMoneyOnHold} money to player {player.UserId}");
		}

		private void CancelExtractionTimer(Player player)
		{
			if (!extractionTimers.ContainsKey(player)) return;
			Timing.KillCoroutines(extractionTimers[player].CoroutineHandle);
			extractionTimers.Remove(player);
		}
	}
}
