using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Classes.Events;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Systems.EventSystems
{
	public static class ServerExtractionSystem
	{
		public static bool isExtractionPointActive;
		public static bool limitReached;
		public static RoomType activeExtractionRoom;
		public static int extractionCount;
		public static int nextExtractionTime;
		public static float extractionStartTime;
		public static Dictionary<Player, ExtractionTimerData> extractionTimers = new();

		private static bool IsPlayerInExtractionRoom(Player player, RoomType roomType)
		{
			if (player.CurrentRoom != null) return player.CurrentRoom.Type == roomType;
			return false;
		}

		public static void StartExtractionEvent()
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
			FoundationFortune.Log($"Extraction point activated in room: {activeExtractionRoom}. It will be active for {FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration} seconds.", LogLevel.Debug);

			extractionCount++;
			CoroutineManager.Coroutines.Add(Timing.CallDelayed(FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration, () => DeactivateExtractionPoint(true)));
		}

		public static void StartExtractionEvent(RoomType room, float duration)
		{
			extractionStartTime = Time.time;
			activeExtractionRoom = room;

			isExtractionPointActive = true;
			FoundationFortune.Log($"Extraction point activated in room: {activeExtractionRoom}. It will be active for T-{duration} seconds.", LogLevel.Debug);

			CoroutineManager.Coroutines.Add(Timing.CallDelayed(duration, () => DeactivateExtractionPoint(false)));
		}

		public static void DeactivateExtractionPoint(bool restart)
		{
			if (!isExtractionPointActive) return;

			if (restart)
			{
				isExtractionPointActive = false;
				if (extractionCount >= FoundationFortune.MoneyExtractionSystemSettings.ExtractionLimit) return;
				nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
				FoundationFortune.Log($"Extraction point in room {activeExtractionRoom} deactivated. Next extraction in T-{nextExtractionTime} Seconds.", LogLevel.Debug);
				CoroutineManager.Coroutines.Add(Timing.CallDelayed(nextExtractionTime, StartExtractionEvent));
			}
            else isExtractionPointActive = false;
        }

		public static void UpdateExtractionMessages(Player ply, ref StringBuilder hintMessage)
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

		private static void StartExtractionTimer(Player player)
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

		private static IEnumerator<float> ExtractionTimerCoroutine(Player player)
		{
			float startTime = Time.time;
			while (Time.time - startTime < 10f)
			{
				if (!IsPlayerInExtractionRoom(player, activeExtractionRoom))
				{
					FoundationFortune.Log($"Extraction timer canceled for player {player.UserId}", LogLevel.Debug);
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
			FoundationFortune.Log($"Transferred {totalMoneyOnHold} money to player {player.UserId}", LogLevel.Debug);
		}

		private static void CancelExtractionTimer(Player player)
		{
			if (!extractionTimers.ContainsKey(player)) return;
			Timing.KillCoroutines(extractionTimers[player].CoroutineHandle);
			extractionTimers.Remove(player);
		}
	}
}
