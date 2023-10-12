using System;
using Exiled.API.Features;
using System.Linq;
using Exiled.API.Enums;
using MEC;
using UnityEngine;
using System.Collections.Generic;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Classes;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
        private bool isExtractionPointActive = false;
        public bool limitReached = false;
		private RoomType activeExtractionRoom;
		private int extractionCount = 0;
		private int nextExtractionTime = 0;
        private float extractionStartTime;
        private Dictionary<Player, ExtractionTimerData> extractionTimers = new();

        private bool IsPlayerInExtractionRoom(Player player, RoomType roomType)
		{
			if (player.CurrentRoom != null) return player.CurrentRoom.Type == roomType;
			else return false;
		}

		public void StartExtractionEvent()
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
			Timing.CallDelayed(FoundationFortune.Singleton.Config.ExtractionPointDuration, () => DeactivateExtractionPoint(true));
		}

        public void StartExtractionEvent(RoomType room, float duration)
        {
            extractionStartTime = Time.time;
            activeExtractionRoom = room;

            isExtractionPointActive = true;
            Log.Debug($"Extraction point activated in room: {activeExtractionRoom}. It will be active for T-{duration} seconds.");

            Timing.CallDelayed(duration, () => DeactivateExtractionPoint(false));
        }

        public void DeactivateExtractionPoint(bool restart)
		{
			if (!isExtractionPointActive) return;

            if (restart)
            {
                isExtractionPointActive = false;
                if (extractionCount < FoundationFortune.Singleton.Config.ExtractionLimit)
                {
                    nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
                    Log.Debug($"Extraction point in room {activeExtractionRoom} deactivated. Next extraction in T-{nextExtractionTime} Seconds.");
                    Timing.CallDelayed(nextExtractionTime, () => StartExtractionEvent());
                }
            }
            else
            {
                isExtractionPointActive = false;
            }
        }

        private void HandleExtractionSystemMessages(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

            if (isExtractionPointActive)
            {
                if (IsPlayerInExtractionRoom(ply, activeExtractionRoom))
                {
                    int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);

                    if (totalMoneyOnHold <= 0)
                    {
                        hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.ExtractionNoMoney}</align>";
                        return;
                    }

                    if (!extractionTimers.ContainsKey(ply)) StartExtractionTimer(ply, TimeSpan.FromSeconds(10));

                    ExtractionTimerData timerData = extractionTimers[ply];
                    float elapsedTime = Time.time - timerData.StartTime;
                    TimeSpan timeLeft = TimeSpan.FromSeconds(10 - elapsedTime);

                    string waitTimerHint = FoundationFortune.Singleton.Translation.ExtractionTimer
                        .Replace("%time%", timeLeft.ToString(@"ss"));
                    hintMessage += $"<align={hintAlignment}>{waitTimerHint}</align>";

                    if (IsExtractionTimerFinished(ply) && totalMoneyOnHold > 0)
                    {
                        CancelExtractionTimer(ply);
                        StartMoneyExtractionTimer(ply);
                        hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.ExtractionStart}</align>";
                    }
                }
                else
                {
                    CancelExtractionTimer(ply);

                    string extractionHint = FoundationFortune.Singleton.Translation.ExtractionEvent
                        .Replace("%room%", activeExtractionRoom.ToString())
                        .Replace("%time%", TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.ExtractionPointDuration - (Time.time - extractionStartTime)).ToString(@"hh\:mm\:ss"));
                    hintMessage += $"<align={hintAlignment}>{extractionHint}</align>";
                }
            }
        }

        private IEnumerator<float> ExtractMoneyCoroutine(Player player)
        {
            Log.Debug($"Extraction coroutine started for player {player.UserId}");

            while (IsPlayerInExtractionRoom(player, activeExtractionRoom) && IsExtractionTimerFinished(player))
            {
                int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);

                if (totalMoneyOnHold > 0)
                {
                    float transferAmount = Mathf.Max(totalMoneyOnHold * 0.1f, 1f);
                    PlayerDataRepository.ModifyMoney(player.UserId, (int)transferAmount, true, true, false);
                    PlayerDataRepository.ModifyMoney(player.UserId, (int)transferAmount, false, false, true);
                    Log.Debug($"Transferred {transferAmount} money to player {player.UserId}");
                }
                yield return Timing.WaitForSeconds(0.5f);
            }

            Log.Debug($"Extraction coroutine finished for player {player.UserId}");
        }

        private bool IsExtractionTimerFinished(Player player)
        {
            Log.Debug($"Extraction timer finished for player {player.UserId}");
            if (extractionTimers.ContainsKey(player))
            {
                ExtractionTimerData timerData = extractionTimers[player];
                float elapsedTime = Time.time - timerData.StartTime;
                TimeSpan timeLeft = TimeSpan.FromSeconds(10 - elapsedTime);

                if (timeLeft.TotalSeconds > 0) return true;
            }
            return false;
        }

        public void StartExtractionTimer(Player player, TimeSpan duration)
        {
            CoroutineHandle timerHandle = Timing.RunCoroutine(ExtractionTimerCoroutine(player, duration));
            ExtractionTimerData timerData = new()
            {
                CoroutineHandle = timerHandle,
                StartTime = Time.time
            };
            extractionTimers[player] = timerData;
        }

        public void CancelExtractionTimer(Player player)
        {
            if (extractionTimers.ContainsKey(player))
            {
                Timing.KillCoroutines(extractionTimers[player].CoroutineHandle);
                extractionTimers.Remove(player);
            }
        }

        private void StartMoneyExtractionTimer(Player player)
        {
            CoroutineHandle timerHandle = Timing.RunCoroutine(ExtractMoneyCoroutine(player));
            ExtractionTimerData timerData = new()
            {
                CoroutineHandle = timerHandle,
                StartTime = Time.time
            };
            extractionTimers[player] = timerData;
        }

        private void CancelMoneyExtractionTimer(Player player)
        {
            if (extractionTimers.ContainsKey(player))
            {
                Timing.KillCoroutines(extractionTimers[player].CoroutineHandle);
                extractionTimers.Remove(player);
            }
        }

        private IEnumerator<float> ExtractionTimerCoroutine(Player player, TimeSpan duration)
        {
            float startTime = Time.time;
            while (Time.time - startTime < duration.TotalSeconds)
            {
                yield return Timing.WaitForSeconds(1f);
            }
            CancelExtractionTimer(player);
        }
    }
}
