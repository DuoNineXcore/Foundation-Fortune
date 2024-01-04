using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Models;
using FoundationFortune.API.Core.Database;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Core.Systems;

public static class ExtractionSystem
{
	public static bool LimitReached;
	private static bool _isExtractionPointActive;
	private static RoomType _activeExtractionRoom;
	private static int _extractionCount;
	private static int _nextExtractionTime;
	private static float _extractionStartTime;
	private static readonly Dictionary<Player, ExtractionTimerData> _extractionTimers = new();
	private static readonly Vector3 _worldPos = new(42f, 12f, 51f);
	private const float _radiusSqr = 16f * 16f;

	private static bool IsPlayerInExtractionRoom(Player player, RoomType roomType)
	{
		if (player.CurrentRoom != null) return player.CurrentRoom.Type == roomType;
		return false;
	}
	
	private static bool IsPlayerInExtractionZone(Player player)
	{
		float distanceSqr = (player.Position - _worldPos).sqrMagnitude;
		return distanceSqr <= _radiusSqr;
	}

	public static void StartExtractionEvent()
	{
		if (!FoundationFortune.MoneyExtractionSystemSettings.MoneyExtractionSystem) return;
		if (_extractionCount >= FoundationFortune.MoneyExtractionSystemSettings.ExtractionLimit)
		{
			LimitReached = true;
			return;
		}

		_extractionStartTime = Time.time;
		_activeExtractionRoom = FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointRooms[UnityEngine.Random.Range(0, FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointRooms.Count)];

		_isExtractionPointActive = true;
		DirectoryIterator.Log($"Extraction point activated in room: {_activeExtractionRoom}. It will be active for {FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration} seconds.", LogLevel.Debug);

		_extractionCount++;
		CoroutineManager.Coroutines.Add(Timing.CallDelayed(FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration, () => DeactivateExtractionPoint(true)));
	}

	public static void StartExtractionEvent(RoomType room, float duration)
	{
		_extractionStartTime = Time.time;
		_activeExtractionRoom = room;

		_isExtractionPointActive = true;
		DirectoryIterator.Log($"Extraction point activated in room: {_activeExtractionRoom}. It will be active for T-{duration} seconds.", LogLevel.Debug);

		CoroutineManager.Coroutines.Add(Timing.CallDelayed(duration, () => DeactivateExtractionPoint(false)));
	}

	public static void DeactivateExtractionPoint(bool restart)
	{
		if (!_isExtractionPointActive) return;

		if (restart)
		{
			_isExtractionPointActive = false;
			if (_extractionCount >= FoundationFortune.MoneyExtractionSystemSettings.ExtractionLimit) return;
			_nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.MoneyExtractionSystemSettings.MinExtractionPointGenerationTime, FoundationFortune.MoneyExtractionSystemSettings.MaxExtractionPointGenerationTime + 1);
			DirectoryIterator.Log($"Extraction point in room {_activeExtractionRoom} deactivated. Next extraction in T-{_nextExtractionTime} Seconds.", LogLevel.Debug);
			CoroutineManager.Coroutines.Add(Timing.CallDelayed(_nextExtractionTime, StartExtractionEvent));
		}
		else _isExtractionPointActive = false;
	}

	public static void UpdateExtractionMessages(Player ply, ref StringBuilder hintMessage)
	{
		if (!_isExtractionPointActive) return;
			
		if (IsPlayerInExtractionRoom(ply, _activeExtractionRoom) || IsPlayerInExtractionZone(ply))
		{
			int totalMoneyOnHold = PlayerStatsRepository.GetMoneyOnHold(ply.UserId);

			if (totalMoneyOnHold <= 0) hintMessage.Append($"{FoundationFortune.Instance.Translation.ExtractionNoMoney}");
			else
			{
				if (!_extractionTimers.ContainsKey(ply)) StartExtractionTimer(ply);

				ExtractionTimerData timerData = _extractionTimers[ply];
				float elapsedTime = Time.time - timerData.StartTime;
				TimeSpan timeLeft = TimeSpan.FromSeconds(10 - elapsedTime);

				string waitTimerHint = FoundationFortune.Instance.Translation.ExtractionTimer
					.Replace("%time%", timeLeft.ToString(@"ss"));
				hintMessage.Append(waitTimerHint);
			}
		}
		else
		{
			if (_extractionTimers.ContainsKey(ply)) CancelExtractionTimer(ply);

			string extractionHint = FoundationFortune.Instance.Translation.ExtractionEvent
				.Replace("%room%", _activeExtractionRoom.ToString())
				.Replace("%time%", TimeSpan.FromSeconds(FoundationFortune.MoneyExtractionSystemSettings.ExtractionPointDuration - (Time.time - _extractionStartTime)).ToString(@"hh\:mm\:ss"));
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
		_extractionTimers[player] = timerData;
	}

	private static IEnumerator<float> ExtractionTimerCoroutine(Player player)
	{
		float startTime = Time.time;
		while (Time.time - startTime < 10f)
		{
			if (!IsPlayerInExtractionRoom(player, _activeExtractionRoom))
			{
				DirectoryIterator.Log($"Extraction timer canceled for player {player.UserId}", LogLevel.Debug);
				CancelExtractionTimer(player);
				yield break;
			}
			yield return Timing.WaitForSeconds(1f);
		}
		int totalMoneyOnHold = PlayerStatsRepository.GetMoneyOnHold(player.UserId);
		if (totalMoneyOnHold <= 0) yield break;
			
		PlayerStatsRepository.TransferMoney(player.UserId, true);
		DirectoryIterator.Log($"Transferred {totalMoneyOnHold} money to player {player.UserId}", LogLevel.Debug);
		
	}

	private static void CancelExtractionTimer(Player player)
	{
		if (!_extractionTimers.ContainsKey(player)) return;
		Timing.KillCoroutines(_extractionTimers[player].CoroutineHandle);
		_extractionTimers.Remove(player);
	}
}