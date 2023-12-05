using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Classes.Hints;
using FoundationFortune.API.Core.Models.Enums.Systems.HintSystem;
using FoundationFortune.API.Features.Items.PerkItems;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.Systems.EventBasedSystems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Systems
{
	public class HintSystem
	{
		public readonly Dictionary<string, (Item item, int price)> ItemsBeingSold = new();
		public readonly Dictionary<string, bool> ConfirmSell = new();
		public readonly Dictionary<string, float> DropTimestamp = new();
		public readonly Dictionary<string, bool> ConfirmActivatePerk = new();
		public readonly Dictionary<string, float> ActivatePerkTimestamp = new();
		private readonly Dictionary<string, Queue<HintEntry>> _recentHints = new();

		public IEnumerator<float> HintSystemCoroutine()
        {
            while (true)
            {
	            foreach (Player ply in Player.List.Where(p => !p.IsDead && !p.IsNPC))
                {
                    if (!PlayerDataRepository.GetHintDisable(ply.UserId)) continue;

                    int moneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);
                    int moneySaved = PlayerDataRepository.GetMoneySaved(ply.UserId);
                    int expCounter = PlayerDataRepository.GetExperience(ply.UserId);
                    int levelCounter = PlayerDataRepository.GetLevel(ply.UserId);
                    int prestigeLevelCounter = PlayerDataRepository.GetPrestigeLevel(ply.UserId);
                    HintAlign hintAlignment = PlayerDataRepository.GetHintAlign(ply.UserId);
                    int hintSize = PlayerDataRepository.GetHintSize(ply.UserId);

                    StringBuilder hintMessageBuilder = new();
                    PerkSystem.UpdatePerkIndicator(PerkSystem.ConsumedPerks, ref hintMessageBuilder);

                    if ((!PlayerDataRepository.GetHintMinmode(ply.UserId)) || (NpcHelperMethods.IsPlayerNearSellingBot(ply) || NpcHelperMethods.IsPlayerNearBuyingBot(ply)))
                    {
	                    bool isPluginAdmin = PlayerDataRepository.GetPluginAdmin(ply.UserId);
	                    bool isHintExtended = PlayerDataRepository.GetHintExtension(ply.UserId);
	                    string moneySavedValue = isPluginAdmin ? "inf" : moneySaved.ToString();
	                    string moneyOnHoldValue = isPluginAdmin ? "inf" : moneyOnHold.ToString();
	                    string expCounterValue = isPluginAdmin ? "inf" : expCounter.ToString();
	                    string levelCounterValue = isPluginAdmin ? "inf" : levelCounter.ToString();
	                    string prestigeLevelCounterValue = isPluginAdmin ? "inf" : prestigeLevelCounter.ToString();

	                    string moneySavedString = FoundationFortune.Instance.Translation.MoneyCounterSaved
		                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
		                    .Replace("%moneySaved%", moneySavedValue);

	                    string moneyHoldString = FoundationFortune.Instance.Translation.MoneyCounterOnHold
		                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
		                    .Replace("%moneyOnHold%", moneyOnHoldValue);

	                    hintMessageBuilder.Append($"{moneySavedString}{moneyHoldString}");

	                    if (isHintExtended)
	                    {
		                    string expCounterString = FoundationFortune.Instance.Translation.ExpCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%expCounter%", expCounterValue);

		                    string levelCounterString = FoundationFortune.Instance.Translation.LevelCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%curLevel%", levelCounterValue);

		                    string prestigeCounterString = FoundationFortune.Instance.Translation.PrestigeCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%prestigelevel%", prestigeLevelCounterValue);

		                    hintMessageBuilder.Append($"{levelCounterString}{expCounterString}{prestigeCounterString}");
	                    }
                    }

                    BountySystem.UpdateBountyMessages(ply, ref hintMessageBuilder);
                    ExtractionSystem.UpdateExtractionMessages(ply, ref hintMessageBuilder);
                    NpcHelperMethods.UpdateNpcProximityMessages(ply, ref hintMessageBuilder);
                    QuestSystem.UpdateQuestMessages(ply, ref hintMessageBuilder);
                    SellingWorkstations.UpdateWorkstationMessages(ply, ref hintMessageBuilder);
                    PerkSystem.UpdateActivePerkMessages(ply, ref hintMessageBuilder);
                    PerkBottle.GetHeldBottle(ply, ref hintMessageBuilder);

                    string recentHintsText = GetRecentHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentHintsText)) hintMessageBuilder.Append(recentHintsText);

                    //string recentAnimatedHintsText = GetAnimatedHints(ply.UserId);
                    //if (!string.IsNullOrEmpty(recentAnimatedHintsText)) hintMessageBuilder.Append(recentAnimatedHintsText);

                    ply.ShowHint($"<size={hintSize}><align={hintAlignment}>{hintMessageBuilder}</align>", 2);

                    if (!ConfirmSell.ContainsKey(ply.UserId) || !(Time.time - DropTimestamp[ply.UserId] >= PlayerDataRepository.GetSellingConfirmationTime(ply.UserId))) continue;
                    ConfirmSell.Remove(ply.UserId);
                    DropTimestamp.Remove(ply.UserId);
                    
                    if (!ConfirmActivatePerk.ContainsKey(ply.UserId) || !(Time.time - ActivatePerkTimestamp[ply.UserId] >= PlayerDataRepository.GetSellingConfirmationTime(ply.UserId))) continue;
                    ConfirmActivatePerk.Remove(ply.UserId);
                    ActivatePerkTimestamp.Remove(ply.UserId);
                }
	            yield return Timing.WaitForSeconds(FoundationFortune.Instance.Config.HintSystemUpdateRate);
            }
            // ReSharper disable once IteratorNeverReturns
            // rain world ruined me i cant stop looking at this error without thinking of five pebbles
        }

        public string GetConfirmationTimeLeft(Player ply)
        {
	        if (!DropTimestamp.ContainsKey(ply.UserId)) return "0";
	        float timeLeft = PlayerDataRepository.GetSellingConfirmationTime(ply.UserId) - (Time.time - DropTimestamp[ply.UserId]);
	        return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
        }
        
        public string GetPerkActivationTimeLeft(Player ply)
        {
	        if (!ActivatePerkTimestamp.ContainsKey(ply.UserId)) return "0";
	        float timeLeft = PlayerDataRepository.GetActiveAbilityActivationTime(ply.UserId) - (Time.time - ActivatePerkTimestamp[ply.UserId]);
	        return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
        }
        
        private string GetRecentHints(string userId)
        {
	        if (!_recentHints.TryGetValue(userId, out Queue<HintEntry> hintQueue)) return string.Empty;

	        StringBuilder hintBuilder = new();
	        while (hintQueue.Count > 0)
	        {
		        HintEntry hintEntry = hintQueue.Dequeue();
		        hintBuilder.Append(hintEntry.Text);
	        }

	        return hintBuilder.ToString();
        }

        public void EnqueueHint(Player player, string hint)
		{
			try
			{
				float expirationTime = Time.time + PlayerDataRepository.GetHintAgeSeconds(player.UserId);
				if (!_recentHints.ContainsKey(player.UserId)) _recentHints[player.UserId] = new Queue<HintEntry>();
				_recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, false));
				while (_recentHints[player.UserId].Count > PlayerDataRepository.GetHintLimit(player.UserId)) _recentHints[player.UserId].Dequeue();
				DirectoryIterator.Log(hint, LogLevel.Debug);
			}
			catch (Exception ex)
			{
				DirectoryIterator.Log(ex.ToString(), LogLevel.Error);
			}
			//you break too much
		}
        
        //fuck hint anims in general for now
        /*
        private string GetAnimatedHints(string userId)
        {
	        if (!_recentHints.TryGetValue(userId, out var hint)) return string.Empty;
	        var currentHints = hint
		        .Where(entry =>
			        entry.IsAnimated && (Time.time - entry.Timestamp) <= PlayerDataRepository.GetHintAgeSeconds(userId))
		        .Select(entry => entry.Text);

	        return string.Join("\n", currentHints);
        }

		public void EnqueueHint(Player player, string hint, HintAnim align)
		{
			if (!_recentHints.TryGetValue(player.UserId, out Queue<HintEntry> hintQueue))
			{
				hintQueue = new Queue<HintEntry>();
				_recentHints[player.UserId] = hintQueue;
			}

			Timing.RunCoroutine(AnimateHintText(player.UserId, hint, PlayerDataRepository.GetHintAgeSeconds(player.UserId), align));
			while (hintQueue.Count > PlayerDataRepository.GetHintLimit(player.UserId)) hintQueue.Dequeue();
		}

		private IEnumerator<float> AnimateHintText(string userId, string hint, float duration, HintAnim animAlignment)
		{
			float startTime = Time.time;
			float endTime = startTime + duration;

			while (Time.time < endTime)
			{
				float t = (Time.time - startTime) / duration;
				int startIndex = 0;
				int endIndex = Mathf.RoundToInt(Mathf.Lerp(0, hint.Length, t));

				switch (animAlignment)
				{
					case HintAnim.Left:
						startIndex = Mathf.RoundToInt(Mathf.Lerp(0, hint.Length, t));
						break;
					case HintAnim.Right:
						endIndex = Mathf.RoundToInt(Mathf.Lerp(0, hint.Length, t));
						break;
					case HintAnim.Center:
						int center = hint.Length / 2;
						startIndex = Mathf.Clamp(center - endIndex, 0, hint.Length - endIndex);
						endIndex = Mathf.Clamp(center + endIndex, startIndex, hint.Length);
						break;
					case HintAnim.None:
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(animAlignment), animAlignment, null);
				}

				string animatedText = hint.Substring(startIndex, endIndex - startIndex);

				if (_recentHints.TryGetValue(userId, out Queue<HintEntry> hintQueue) && hintQueue.Count > 0)
				{
					var lastEntry = hintQueue.Last();
					if (lastEntry is { IsAnimated: true })
					{
						lastEntry.Text = animatedText;
						lastEntry.Timestamp = Time.time;
					}
				}
				else
				{
					if (hintQueue == null)
					{
						hintQueue = new Queue<HintEntry>();
						_recentHints[userId] = hintQueue;
					}
					hintQueue.Enqueue(new HintEntry(animatedText, Time.time, true));
				}

				yield return Timing.WaitForSeconds(FoundationFortune.Instance.Config.AnimatedHintUpdateRate);
			}
		}
		*/
	}
}
