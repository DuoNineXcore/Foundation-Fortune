using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		
		private readonly Dictionary<string, Queue<StaticHintEntry>> _recentHints = new();

		public IEnumerator<float> HintSystemCoroutine()
		{
			StringBuilder hintMessageBuilder = new StringBuilder();

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

					hintMessageBuilder.Clear();

					PerkSystem.UpdatePerkIndicator(PerkSystem.ConsumedPerks, ref hintMessageBuilder);

					if ((!PlayerDataRepository.GetHintMinmode(ply.UserId)) || (NPCHelperMethods.IsPlayerNearSellingBot(ply) || NPCHelperMethods.IsPlayerNearBuyingBot(ply)))
					{
						bool isPluginAdmin = PlayerDataRepository.GetPluginAdmin(ply.UserId);
						bool isHintExtended = PlayerDataRepository.GetHintExtension(ply.UserId);
						string moneySavedValue = isPluginAdmin ? "inf" : moneySaved.ToString();
						string moneyOnHoldValue = isPluginAdmin ? "inf" : moneyOnHold.ToString();
						string expCounterValue = isPluginAdmin ? "inf" : expCounter.ToString();
						string levelCounterValue = isPluginAdmin ? "inf" : levelCounter.ToString();
						string prestigeLevelCounterValue = isPluginAdmin ? "inf" : prestigeLevelCounter.ToString();

						hintMessageBuilder.AppendFormat("{0}{1}",
							FoundationFortune.Instance.Translation.MoneyCounterSaved
								.Replace("%rolecolor%", ply.Role.Color.ToHex())
								.Replace("%moneySaved%", moneySavedValue),
							FoundationFortune.Instance.Translation.MoneyCounterOnHold
								.Replace("%rolecolor%", ply.Role.Color.ToHex())
								.Replace("%moneyOnHold%", moneyOnHoldValue));

						if (isHintExtended)
						{
							hintMessageBuilder.AppendFormat("{0}{1}{2}",
								FoundationFortune.Instance.Translation.ExpCounter
									.Replace("%rolecolor%", ply.Role.Color.ToHex())
									.Replace("%expCounter%", expCounterValue),
								FoundationFortune.Instance.Translation.LevelCounter
									.Replace("%rolecolor%", ply.Role.Color.ToHex())
									.Replace("%curLevel%", levelCounterValue),
								FoundationFortune.Instance.Translation.PrestigeCounter
									.Replace("%rolecolor%", ply.Role.Color.ToHex())
									.Replace("%prestigelevel%", prestigeLevelCounterValue));
						}
					}

					BountySystem.UpdateBountyMessages(ply, ref hintMessageBuilder);
					ExtractionSystem.UpdateExtractionMessages(ply, ref hintMessageBuilder);
					NPCHelperMethods.UpdateNpcProximityMessages(ply, ref hintMessageBuilder);
					QuestSystem.UpdateQuestMessages(ply, ref hintMessageBuilder);
					SellingWorkstations.UpdateWorkstationMessages(ply, ref hintMessageBuilder);
					PerkSystem.UpdateActivePerkMessages(ply, ref hintMessageBuilder);
					PerkBottle.GetHeldBottle(ply, ref hintMessageBuilder);

					var recentHintsText = GetRecentHints(ply.UserId);
					if (!string.IsNullOrEmpty(recentHintsText)) hintMessageBuilder.Append(recentHintsText);

					ply.ShowHint($"<size={hintSize}><align={hintAlignment}>{hintMessageBuilder}</align>", 2);

					if (ConfirmSell.ContainsKey(ply.UserId) && Time.time - DropTimestamp[ply.UserId] >= PlayerDataRepository.GetSellingConfirmationTime(ply.UserId))
					{
						ConfirmSell.Remove(ply.UserId);
						DropTimestamp.Remove(ply.UserId);
					}

					if (ConfirmActivatePerk.ContainsKey(ply.UserId) && (Time.time - ActivatePerkTimestamp[ply.UserId] >= PlayerDataRepository.GetSellingConfirmationTime(ply.UserId)))
					{
						ConfirmActivatePerk.Remove(ply.UserId);
						ActivatePerkTimestamp.Remove(ply.UserId);
					}
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
	        if (!_recentHints.TryGetValue(userId, out Queue<StaticHintEntry> hintQueue)) return string.Empty;

	        StringBuilder hintBuilder = new();
	        while (hintQueue.Count > 0)
	        {
		        StaticHintEntry hintEntry = hintQueue.Dequeue();
		        hintBuilder.Append(hintEntry.Text);
	        }

	        return hintBuilder.ToString();
        }

        public void BroadcastHint(Player player, string hint)
        {
	        if (!_recentHints.ContainsKey(player.UserId)) _recentHints[player.UserId] = new Queue<StaticHintEntry>(); 
	        _recentHints[player.UserId].Enqueue(new StaticHintEntry(hint, Time.time + PlayerDataRepository.GetHintAgeSeconds(player.UserId))); 
	        while (_recentHints[player.UserId].Count > PlayerDataRepository.GetHintLimit(player.UserId)) _recentHints[player.UserId].Dequeue();
        }
	}
}
