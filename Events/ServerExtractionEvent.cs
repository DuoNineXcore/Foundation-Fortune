using System;
using Exiled.API.Features;
using System.Linq;
using Exiled.API.Enums;
using MEC;
using UnityEngine;
using System.Collections.Generic;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;

namespace FoundationFortune.Events
{
    public partial class ServerEvents
    {
        private bool isExtractionPointActive = false;
        private RoomType activeExtractionRoom;
        private int extractionCount = 0;
        private int nextExtractionTime = 0;
        private float extractionStartTime = 0f;
        private Dictionary<Player, CoroutineHandle> extractionTimers = new();
        private bool extracting = false;

        private bool IsPlayerInExtractionRoom(Player player, RoomType roomType)
        {
            if (player.CurrentRoom != null) return player.CurrentRoom.Type == roomType;
            else return false;
        }

        private void StartExtractionEvent()
        {
            if (extractionCount >= FoundationFortune.Singleton.Config.ExtractionLimit)
            {
                Log.Info("Extraction limit reached. No more extractions will occur.");
                return;
            }

            extractionStartTime = Time.time;
            activeExtractionRoom = FoundationFortune.Singleton.Config.ExtractionPointRooms[UnityEngine.Random.Range(0, FoundationFortune.Singleton.Config.ExtractionPointRooms.Count)];

            isExtractionPointActive = true;
            Log.Info($"Extraction point activated in room: {activeExtractionRoom}. It will be active for {FoundationFortune.Singleton.Config.ExtractionPointDuration} seconds.");

            extractionCount++;
            Timing.CallDelayed(FoundationFortune.Singleton.Config.ExtractionPointDuration, () => DeactivateExtractionPoint());
        }

        private void DeactivateExtractionPoint()
        {
            if (!isExtractionPointActive) return;

            isExtractionPointActive = false;
            if (extractionCount < FoundationFortune.Singleton.Config.ExtractionLimit)
            {
                nextExtractionTime = UnityEngine.Random.Range(FoundationFortune.Singleton.Config.MinExtractionPointGenerationTime, FoundationFortune.Singleton.Config.MaxExtractionPointGenerationTime + 1);
                Log.Info($"Extraction point in room {activeExtractionRoom} deactivated. Next extraction in T-{nextExtractionTime} Seconds.");
                Timing.CallDelayed(nextExtractionTime, () => StartExtractionEvent());
            }
        }

        private IEnumerator<float> ExtractionTimer(Player player, TimeSpan waitDuration)
        {
            float startTime = Time.time;
            float elapsedTime = 0f;

            while (IsPlayerInExtractionRoom(player, activeExtractionRoom) && elapsedTime < waitDuration.TotalSeconds)
            {
                elapsedTime = Time.time - startTime;
                yield return Timing.WaitForSeconds(1f);
            }

            if (IsPlayerInExtractionRoom(player, activeExtractionRoom))
            {
                MoveMoney(player);
            }
            else
            {
                CancelExtractionTimer(player);
            }
        }

        private void MoveMoney(Player player)
        {
            int totalMoneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);
            float transferAmount = totalMoneyOnHold * 0.1f;

            PlayerDataRepository.ModifyMoney(player.UserId, (int)transferAmount, true, true, false);
            PlayerDataRepository.ModifyMoney(player.UserId, (int)transferAmount, false, false, true);
        }

        private void UpdateExtractionEventHint(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

            if (isExtractionPointActive)
            {
                if (IsPlayerInExtractionRoom(ply, activeExtractionRoom))
                {
                    if (!extractionTimers.ContainsKey(ply))
                    {
                        if (!extracting)
                        {
                            StartExtractionTimer(ply, TimeSpan.FromSeconds(10));
                            extracting = true;
                        }
                        string waitTimerHint = FoundationFortune.Singleton.Translation.ExtractionTimer
                            .Replace("%time%", "10");
                        hintMessage += $"<align={hintAlignment}>{waitTimerHint}</align>";
                        return;
                    }
                    else
                    {
                        string extractingHint = FoundationFortune.Singleton.Translation.ExtractingHint;
                        hintMessage += $"<align={hintAlignment}>{extractingHint}</align>";
                    }
                }
                else
                {
                    string extractionHint = FoundationFortune.Singleton.Translation.ExtractionZoneStart
                        .Replace("%room%", activeExtractionRoom.ToString())
                        .Replace("%time%", TimeSpan.FromSeconds(FoundationFortune.Singleton.Config.ExtractionPointDuration - (Time.time - extractionStartTime)).ToString(@"hh\:mm\:ss"));
                    hintMessage += $"<align={hintAlignment}>{extractionHint}</align>";
                }
            }
        }

        private void StartExtractionTimer(Player player, TimeSpan duration)
        {
            if (!extractionTimers.ContainsKey(player))
            {
                CoroutineHandle timerHandle = Timing.RunCoroutine(ExtractionTimer(player, duration));
                extractionTimers[player] = timerHandle;
            }
        }

        private void CancelExtractionTimer(Player player)
        {
            if (extractionTimers.ContainsKey(player))
            {
                Timing.KillCoroutines(extractionTimers[player]);
                extractionTimers.Remove(player);
            }
        }
    }
}
