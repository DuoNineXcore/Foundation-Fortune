using Exiled.API.Features;
using FoundationFortune.API.Database;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Exiled.API.Features.Items;
using System.Text;
using FoundationFortune.API.Items.PerkItems;
using FoundationFortune.API.Models;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private Dictionary<string, Queue<HintEntry>> recentHints = new();
		private Dictionary<string, bool> confirmSell = new();
		private Dictionary<string, float> dropTimestamp = new();
		private Dictionary<string, (Item item, int price)> itemsBeingSold = new();

        private IEnumerator<float> UpdateMoneyAndHints()
        {
            while (true)
            {
                float updateRate = recentHints.Any(entry => entry.Value.Any(hint => hint.IsAnimated)) ?
                                    FoundationFortune.Singleton.Config.AnimatedHintUpdateRate :
                                    FoundationFortune.Singleton.Config.HintSystemUpdateRate;

                foreach (Player ply in Player.List.Where(p => !p.IsDead && !p.IsNPC))
                {
                    if (!PlayerDataRepository.GetHintDisable(ply.UserId)) continue;

                    int moneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);
                    int moneySaved = PlayerDataRepository.GetMoneySaved(ply.UserId);

                    HintAlign hintAlignment = PlayerDataRepository.GetHintAlign(ply.UserId);
                    int hintAlpha = PlayerDataRepository.GetHintAlpha(ply.UserId);
                    int hintSize = PlayerDataRepository.GetHintSize(ply.UserId);

                    StringBuilder hintMessageBuilder = new();
                    UpdatePerkIndicator(FoundationFortune.Singleton.ConsumedPerks, ref hintMessageBuilder);

                    if (!PlayerDataRepository.GetHintMinmode(ply.UserId))
                    {
                        string moneySavedString;
                        string moneyHoldString;
                        
                        if (PlayerDataRepository.GetPluginAdmin(ply.UserId))
                        {
                            moneySavedString = FoundationFortune.Singleton.Translation.MoneyCounterSaved
                                .Replace("%rolecolor%", ply.Role.Color.ToHex())
                                .Replace("%moneySaved%", "inf");

                            moneyHoldString = FoundationFortune.Singleton.Translation.MoneyCounterOnHold
                                .Replace("%rolecolor%", ply.Role.Color.ToHex())
                                .Replace("%moneyOnHold%", "inf");
                        }
                        else
                        {
                            moneySavedString = FoundationFortune.Singleton.Translation.MoneyCounterSaved
                                .Replace("%rolecolor%", ply.Role.Color.ToHex())
                                .Replace("%moneySaved%", moneySaved.ToString());

                            moneyHoldString = FoundationFortune.Singleton.Translation.MoneyCounterOnHold
                                .Replace("%rolecolor%", ply.Role.Color.ToHex())
                                .Replace("%moneyOnHold%", moneyOnHold.ToString());
                        }
                        hintMessageBuilder.Append($"{moneySavedString}{moneyHoldString}");
                    }

                    UpdateExtractionMessages(ply, ref hintMessageBuilder);
                    UpdateNpcProximityMessages(ply, ref hintMessageBuilder);
                    UpdateWorkstationMessages(ply, ref hintMessageBuilder);
                    UpdateBountyMessages(ply, ref hintMessageBuilder);
                    PerkBottle.GetHeldBottle(ply, ref hintMessageBuilder);

                    string recentHintsText = GetRecentHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentHintsText)) hintMessageBuilder.Append(recentHintsText);

                    string recentAnimatedHintsText = GetAnimatedHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentAnimatedHintsText)) hintMessageBuilder.Append(recentAnimatedHintsText);

                    ply.ShowHint($"{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{hintMessageBuilder}</align>", 2);

                    if (!confirmSell.ContainsKey(ply.UserId) || !(Time.time - dropTimestamp[ply.UserId] >=
                                                                  FoundationFortune.Singleton.Config
	                                                                  .SellingConfirmationTime)) continue;
                    confirmSell.Remove(ply.UserId);
                    dropTimestamp.Remove(ply.UserId);
                }

                yield return Timing.WaitForSeconds(updateRate);
            }
        }

        public string GetRecentHints(string userId)
		{
			if (!recentHints.TryGetValue(userId, out var hint)) return string.Empty;
			var currentHints = hint
				.Where(entry => !entry.IsAnimated && (Time.time - entry.Timestamp) <= FoundationFortune.Singleton.Config.MaxHintAge)
				.Select(entry => entry.Text);

			return string.Join("\n", currentHints);
		}

		public string GetAnimatedHints(string userId)
		{
			if (!recentHints.TryGetValue(userId, out var hint)) return string.Empty;
			var currentHints = hint
				.Where(entry => entry.IsAnimated && (Time.time - entry.Timestamp) <= FoundationFortune.Singleton.Config.MaxHintAge)
				.Select(entry => entry.Text);

			return string.Join("\n", currentHints);
		}

		public void EnqueueHint(Player player, string hint, float duration)
		{
			Log.Debug($"Enqueuing non-animated hint for player {player.UserId}");

			float expirationTime = Time.time + duration;
			if (!recentHints.ContainsKey(player.UserId)) recentHints[player.UserId] = new Queue<HintEntry>();
			recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, false));
			while (recentHints[player.UserId].Count > PlayerDataRepository.GetHintLimit(player.UserId)) recentHints[player.UserId].Dequeue();
		}

		public void EnqueueHint(Player player, string hint, float duration, HintAnim align)
		{
			Log.Debug($"Enqueuing animated hint for player {player.UserId}");

			if (!recentHints.TryGetValue(player.UserId, out Queue<HintEntry> hintQueue))
			{
				hintQueue = new Queue<HintEntry>();
				recentHints[player.UserId] = hintQueue;
			}

			Timing.RunCoroutine(AnimateHintText(player.UserId, hint, duration, align));
			while (hintQueue.Count > PlayerDataRepository.GetHintLimit(player.UserId)) hintQueue.Dequeue();
		}

		private IEnumerator<float> AnimateHintText(string userId, string hint, float duration, HintAnim animAlignment)
		{
			Log.Debug($"Animating hint for user {userId}");

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
