using Exiled.API.Features;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Exiled.API.Features.Items;
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

					Log.Debug($"Updating money and hints for player {ply.UserId}");

					int moneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);
					int moneySaved = PlayerDataRepository.GetMoneySaved(ply.UserId);

					HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);
					int hintAlpha = PlayerDataRepository.GetHintAlpha(ply.UserId);
					int hintSize = PlayerDataRepository.GetHintSize(ply.UserId);

					string hintMessage = "";

					if (!PlayerDataRepository.GetHintMinmode(ply.UserId))
					{
						string moneySavedString = FoundationFortune.Singleton.Translation.MoneyCounterSaved
						    .Replace("%rolecolor%", ply.Role.Color.ToHex())
						    .Replace("%moneySaved%", moneySaved.ToString());
						string moneyHoldString = FoundationFortune.Singleton.Translation.MoneyCounterOnHold
						    .Replace("%rolecolor%", ply.Role.Color.ToHex())
						    .Replace("%moneyOnHold%", moneyOnHold.ToString());

						hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{moneySavedString}{moneyHoldString}</align></size>\n";
					}

					HandleExtractionSystemMessages(ply, ref hintMessage);
					HandleWorkstationMessages(ply, ref hintMessage);
					HandleBuyingBotMessages(ply, ref hintMessage);
					HandleBountySystemMessages(ply, ref hintMessage);

					string recentHintsText = GetRecentHints(ply.UserId);
					//if (!string.IsNullOrEmpty(recentHintsText)) hintMessage += $"<align={hintAlignment}>\n{recentHintsText}</align>";
					//if (!string.IsNullOrEmpty(recentHintsText)) hintMessage += $"\n<alpha={IntToHexAlpha(hintAlpha)}><size={hintSize}><align={hintAlignment}>{recentHintsText}</align></size>\n";
					if (!string.IsNullOrEmpty(recentHintsText)) hintMessage += $"\n<size={hintSize}><align={hintAlignment}>{recentHintsText}</align></size>\n";


					string recentAnimatedHintsText = GetAnimatedHints(ply.UserId);
					//if (!string.IsNullOrEmpty(recentAnimatedHintsText)) hintMessage += $"<align={hintAlignment}>\n{recentAnimatedHintsText}</align>";
					if (!string.IsNullOrEmpty(recentAnimatedHintsText)) hintMessage += $"\n<alpha={IntToHexAlpha(hintAlpha)}><size={hintSize}><align={hintAlignment}>\n{recentAnimatedHintsText}</align></size>\n";

					ply.ShowHint(hintMessage, 2);

					if (confirmSell.ContainsKey(ply.UserId) && Time.time - dropTimestamp[ply.UserId] >= FoundationFortune.Singleton.Config.SellingConfirmationTime)
					{
						confirmSell.Remove(ply.UserId);
						dropTimestamp.Remove(ply.UserId);
					}
				}

				yield return Timing.WaitForSeconds(updateRate);
			}
		}

		public string GetRecentHints(string userId)
		{
			Log.Debug($"Getting recent hints for user {userId}");

			if (recentHints.ContainsKey(userId))
			{
				var currentHints = recentHints[userId]
				    .Where(entry => !entry.IsAnimated && (Time.time - entry.Timestamp) <= FoundationFortune.Singleton.Config.MaxHintAge)
				    .Select(entry => entry.Text);

				return string.Join("\n", currentHints);
			}

			return "";
		}

		public string GetAnimatedHints(string userId)
		{
			Log.Debug($"Getting animated hints for user {userId}");

			if (recentHints.ContainsKey(userId))
			{
				var currentHints = recentHints[userId]
				    .Where(entry => entry.IsAnimated && (Time.time - entry.Timestamp) <= FoundationFortune.Singleton.Config.MaxHintAge)
				    .Select(entry => entry.Text);

				return string.Join("\n", currentHints);
			}

			return "";
		}

		public void EnqueueHint(Player player, string hint, float duration)
		{
			Log.Debug($"Enqueuing non-animated hint for player {player.UserId}");

			float expirationTime = Time.time + duration;
			if (!recentHints.ContainsKey(player.UserId)) recentHints[player.UserId] = new Queue<HintEntry>();
			recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, false));
			while (recentHints[player.UserId].Count > FoundationFortune.Singleton.Config.MaxHintsToShow) recentHints[player.UserId].Dequeue();
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
			while (hintQueue.Count > FoundationFortune.Singleton.Config.MaxHintsToShow) hintQueue.Dequeue();
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
					if (lastEntry != null && lastEntry.IsAnimated)
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
