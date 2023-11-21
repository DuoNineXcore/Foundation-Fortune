using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Classes.Hints;
using FoundationFortune.API.Core.Models.Enums.Hints;
using FoundationFortune.API.Features.Items.PerkItems;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.Systems.EventSystems;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Features.Systems
{
	public class HintSystem
	{
		private Dictionary<string, Queue<HintEntry>> recentHints = new();
		public Dictionary<string, bool> confirmSell = new();
		public Dictionary<string, float> dropTimestamp = new();
		public Dictionary<string, (Item item, int price)> itemsBeingSold = new();

        public IEnumerator<float> HintSystemCoroutine()
        {
            while (true)
            {
                float updateRate = recentHints.Any(entry => entry.Value.Any(hint => hint.IsAnimated)) ? FoundationFortune.Singleton.Config.AnimatedHintUpdateRate : FoundationFortune.Singleton.Config.HintSystemUpdateRate;

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
                    PerkSystem.UpdatePerkIndicator(FoundationFortune.Singleton.ConsumedPerks, ref hintMessageBuilder);

                    if ((!PlayerDataRepository.GetHintMinmode(ply.UserId)) || (NPCHelperMethods.IsPlayerNearSellingBot(ply) || NPCHelperMethods.IsPlayerNearBuyingBot(ply)))
                    {
	                    bool isPluginAdmin = PlayerDataRepository.GetPluginAdmin(ply.UserId);
	                    bool isHintExtended = PlayerDataRepository.GetHintExtension(ply.UserId);
	                    string moneySavedValue = isPluginAdmin ? "inf" : moneySaved.ToString();
	                    string moneyOnHoldValue = isPluginAdmin ? "inf" : moneyOnHold.ToString();
	                    string expCounterValue = isPluginAdmin ? "inf" : expCounter.ToString();
	                    string levelCounterValue = isPluginAdmin ? "inf" : levelCounter.ToString();
	                    string prestigeLevelCounterValue = isPluginAdmin ? "inf" : prestigeLevelCounter.ToString();

	                    string moneySavedString = FoundationFortune.Singleton.Translation.MoneyCounterSaved
		                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
		                    .Replace("%moneySaved%", moneySavedValue);

	                    string moneyHoldString = FoundationFortune.Singleton.Translation.MoneyCounterOnHold
		                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
		                    .Replace("%moneyOnHold%", moneyOnHoldValue);

	                    hintMessageBuilder.Append($"{moneySavedString}{moneyHoldString}");

	                    if (isHintExtended)
	                    {
		                    string expCounterString = FoundationFortune.Singleton.Translation.EXPCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%expCounter%", expCounterValue);

		                    string levelCounterString = FoundationFortune.Singleton.Translation.LevelCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%curLevel%", levelCounterValue);

		                    string prestigeCounterString = FoundationFortune.Singleton.Translation.PrestigeCounter
			                    .Replace("%rolecolor%", ply.Role.Color.ToHex())
			                    .Replace("%prestigelevel%", prestigeLevelCounterValue);

		                    hintMessageBuilder.Append($"{levelCounterString}{expCounterString}{prestigeCounterString}");
	                    }
                    }

                    ServerBountySystem.UpdateBountyMessages(ply, ref hintMessageBuilder);
                    ServerExtractionSystem.UpdateExtractionMessages(ply, ref hintMessageBuilder);
                    UpdateNpcProximityMessages(ply, ref hintMessageBuilder);
                    SellingWorkstations.UpdateWorkstationMessages(ply, ref hintMessageBuilder);
                    PerkBottle.GetHeldBottle(ply, ref hintMessageBuilder);

                    string recentHintsText = GetRecentHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentHintsText)) hintMessageBuilder.Append(recentHintsText);

                    string recentAnimatedHintsText = GetAnimatedHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentAnimatedHintsText)) hintMessageBuilder.Append(recentAnimatedHintsText);

                    ply.ShowHint($"<size={hintSize}><align={hintAlignment}>{hintMessageBuilder}</align>", 2);

                    if (!confirmSell.ContainsKey(ply.UserId) || !(Time.time - dropTimestamp[ply.UserId] >= FoundationFortune.SellableItemsList.SellingConfirmationTime)) continue;
                    confirmSell.Remove(ply.UserId);
                    dropTimestamp.Remove(ply.UserId);
                }

                yield return Timing.WaitForSeconds(updateRate);
            }
        }
        
        private void UpdateNpcProximityMessages(Player ply, ref StringBuilder hintMessage)
        {
	        if (NPCHelperMethods.IsPlayerNearBuyingBot(ply)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.BuyingBot}");
	        else if (NPCHelperMethods.IsPlayerNearSellingBot(ply))
	        {
		        if (!confirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");
		        else if (confirmSell[ply.UserId])
		        {
			        hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");

			        if (!itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
                    
			        int price = soldItemData.price;
			        string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
				        .Replace("%price%", price.ToString())
				        .Replace("%time%", GetConfirmationTimeLeft(ply));

			        hintMessage.Append($"{confirmationHint}");
		        }
	        }
        }
    
        public string GetConfirmationTimeLeft(Player ply)
        {
	        if (!dropTimestamp.ContainsKey(ply.UserId)) return "0";
	        float timeLeft = FoundationFortune.SellableItemsList.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
	        return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
        }

        private string GetRecentHints(string userId)
		{
			if (!recentHints.TryGetValue(userId, out var hint)) return string.Empty;
			var currentHints = hint
				.Where(entry => !entry.IsAnimated && (Time.time - entry.Timestamp) <= PlayerDataRepository.GetHintSeconds(userId))
				.Select(entry => entry.Text);

			return string.Join("\n", currentHints);
		}

        private string GetAnimatedHints(string userId)
		{
			if (!recentHints.TryGetValue(userId, out var hint)) return string.Empty;
			var currentHints = hint
				.Where(entry => entry.IsAnimated && (Time.time - entry.Timestamp) <= PlayerDataRepository.GetHintSeconds(userId))
				.Select(entry => entry.Text);

			return string.Join("\n", currentHints);
		}

		public void EnqueueHint(Player player, string hint)
		{
			float expirationTime = Time.time + PlayerDataRepository.GetHintSeconds(player.UserId);
			if (!recentHints.ContainsKey(player.UserId)) recentHints[player.UserId] = new Queue<HintEntry>();
			recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, false));
			while (recentHints[player.UserId].Count > PlayerDataRepository.GetHintLimit(player.UserId)) recentHints[player.UserId].Dequeue();
		}
		
		public void EnqueueHint(Player player, string hint, int duration)
		{
			float expirationTime = Time.time + duration;
			if (!recentHints.ContainsKey(player.UserId)) recentHints[player.UserId] = new Queue<HintEntry>();
			recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, false));
			while (recentHints[player.UserId].Count > PlayerDataRepository.GetHintLimit(player.UserId)) recentHints[player.UserId].Dequeue();
		}

		public void EnqueueHint(Player player, string hint, int duration, HintAnim align)
		{
			if (!recentHints.TryGetValue(player.UserId, out Queue<HintEntry> hintQueue))
			{
				hintQueue = new Queue<HintEntry>();
				recentHints[player.UserId] = hintQueue;
			}

			Timing.RunCoroutine(AnimateHintText(player.UserId, hint, duration, align));
			while (hintQueue.Count > PlayerDataRepository.GetHintLimit(player.UserId)) hintQueue.Dequeue();
		}
		
		public void EnqueueHint(Player player, string hint, HintAnim align)
		{
			if (!recentHints.TryGetValue(player.UserId, out Queue<HintEntry> hintQueue))
			{
				hintQueue = new Queue<HintEntry>();
				recentHints[player.UserId] = hintQueue;
			}

			Timing.RunCoroutine(AnimateHintText(player.UserId, hint, PlayerDataRepository.GetHintSeconds(player.UserId), align));
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
				}

				string animatedText = hint.Substring(startIndex, endIndex - startIndex);

				if (recentHints.TryGetValue(userId, out Queue<HintEntry> hintQueue) && hintQueue.Count > 0)
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
						recentHints[userId] = hintQueue;
					}
					hintQueue.Enqueue(new HintEntry(animatedText, Time.time, true));
				}

				yield return Timing.WaitForSeconds(FoundationFortune.Singleton.Config.AnimatedHintUpdateRate);
			}
		}
	}
}
