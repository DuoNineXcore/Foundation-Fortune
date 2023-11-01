using System;
using Exiled.API.Features;
using Exiled.API.Enums;
using MEC;
using UnityEngine;
using System.Collections.Generic;
using FoundationFortune.API.Database;
using System.Text;
using FoundationFortune.API.Models;

// ReSharper disable once CheckNamespace
// STFU!!!!!!!!!!!!!!!!
namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
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
			else return false;
		}

		private void StartExtractionEvent()
		{
			if (!FoundationFortune.Singleton.Config.MoneyExtractionSystem) return;
			if (extractionCount >= FoundationFortune.Singleton.Config.ExtractionLimit)
			{
				limitReached = true;
				return;
			}

			extractionStartTime = Time.time;
			activeExtractionRoom = FoundationFortune.Singleton.Config.ExtractionPointRooms[UnityEngine.Random.Range(0, FoundationFortune.Singleton.Config.ExtractionPointRooms.Count)];

			isExtractionPointActive = true;
			Log.Debug($"Extraction point activated in room: {activeExtractionRoom}. It will be active for {FoundationFortune.Singleton.Config.ExtractionPointDuration} seconds.");

			extractionCount++;
			CoroutineManager.Coroutines.Add(Timing.CallDelayed(FoundationFortune.Singleton.Config.ExtractionPointDuration, () => DeactivateExtractionPoint(true)));
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
				if (extractionCount >= FoundationFortune.Singleton.Config.ExtractionLimit) return;
				nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
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
						.Replace("%time%", TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.ExtractionPointDuration - (Time.time - extractionStartTime)).ToString(@"hh\:mm\:ss"));
					hintMessage.Append(extractionHint);
				}
		}

		private void ExtractMoney(Player player)
        {
	        try
	        {
		        while (IsPlayerInExtractionRoom(player, activeExtractionRoom))
		        {
			        int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);
			        if (totalMoneyOnHold <= 0) continue;
			        PlayerDataRepository.TransferMoney(player.UserId, true);
			        Log.Debug($"Transferred {totalMoneyOnHold} money to player {player.UserId}");
		        }

		        Log.Debug($"Money extraction finished for player {player.UserId}");
	        }
	        catch (Exception ex)
	        {
		        Log.Debug(ex);
	        }
        }

        private bool IsExtractionTimerFinished(Player player)
		{
			Log.Debug($"Extraction timer finished for player {player.UserId}");
			if (!extractionTimers.TryGetValue(player, out var timer)) return false;
			float elapsedTime = Time.time - timer.StartTime;
			TimeSpan timeLeft = TimeSpan.FromSeconds(10 - elapsedTime);

			return timeLeft.TotalSeconds > 0;
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

		private void CancelExtractionTimer(Player player)
		{
			if (!extractionTimers.ContainsKey(player)) return;
			Timing.KillCoroutines(extractionTimers[player].CoroutineHandle);
			extractionTimers.Remove(player);
		}
	}
}
