using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Core.Common.Enums.Systems.HintSystem;
using FoundationFortune.API.Core.Common.Models.Hints;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Features.Items.PerkItems;
using FoundationFortune.API.Features.Items.World;
using FoundationFortune.API.Features.NPCs;
using MEC;
using UnityEngine;
using UnityEngine.Device;
using Screen = UnityEngine.Device.Screen;

namespace FoundationFortune.API.Core.Systems;

public class HintSystem
{
	public readonly Dictionary<string, (Item item, int price)> ItemsBeingSold = new();
	public readonly Dictionary<string, bool> ConfirmSell = new();
	public readonly Dictionary<string, DateTime> DropTimestamp = new();
	public readonly Dictionary<string, bool> ConfirmActivatePerk = new();
	public readonly Dictionary<string, DateTime> ActivatePerkTimestamp = new();
		
	private readonly Dictionary<string, Queue<StaticHintEntry>> _recentHints = new();

    public IEnumerator<float> HintSystemCoroutine()
    {
        StringBuilder hintMessageBuilder = new();

        while (true)
        {
            foreach (Player ply in Player.List.Where(p => !p.IsDead && !p.IsNPC))
            {
	            if (!PlayerSettingsRepository.GetHintDisable(ply.UserId)) continue;

                int moneyOnHold = PlayerStatsRepository.GetMoneyOnHold(ply.UserId);
                int moneySaved = PlayerStatsRepository.GetMoneySaved(ply.UserId);
                int expCounter = PlayerStatsRepository.GetExperience(ply.UserId);
                int levelCounter = PlayerStatsRepository.GetLevel(ply.UserId);
                int prestigeLevelCounter = PlayerStatsRepository.GetPrestigeLevel(ply.UserId);
                HintAlign hintAlignment = PlayerSettingsRepository.GetHintAlign(ply.UserId);
                int hintSize = PlayerSettingsRepository.GetHintSize(ply.UserId);

                hintMessageBuilder.Clear();

                PerkSystem.UpdatePerkIndicator(PerkSystem.ConsumedPerks, ref hintMessageBuilder);

                if ((!PlayerSettingsRepository.GetHintMinmode(ply.UserId)) || (NPCHelperMethods.IsPlayerNearSellingBot(ply) || NPCHelperMethods.IsPlayerNearBuyingBot(ply)))
                {
                    bool isPluginAdmin = PlayerSettingsRepository.GetPluginAdmin(ply.UserId);
                    bool isHintExtended = PlayerSettingsRepository.GetHintExtension(ply.UserId);
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

                if (ConfirmSell.ContainsKey(ply.UserId) && (DateTime.UtcNow - DropTimestamp[ply.UserId]).TotalSeconds >= PlayerSettingsRepository.GetSellingConfirmationTime(ply.UserId))
                {
                    ConfirmSell.Remove(ply.UserId);
                    DropTimestamp.Remove(ply.UserId);
                }

                if (!ConfirmActivatePerk.ContainsKey(ply.UserId) || !((DateTime.UtcNow - ActivatePerkTimestamp[ply.UserId]).TotalSeconds >= PlayerSettingsRepository.GetSellingConfirmationTime(ply.UserId))) continue;
                ConfirmActivatePerk.Remove(ply.UserId);
                ActivatePerkTimestamp.Remove(ply.UserId);
            }

            yield return Timing.WaitForSeconds(FoundationFortune.Instance.Config.HintSystemUpdateRate);
	    }
	}

	public string GetConfirmationTimeLeft(Player ply)
	{
		if (!DropTimestamp.ContainsKey(ply.UserId)) return "0";
		DateTime currentTime = DateTime.UtcNow;
		DateTime dropTime = DropTimestamp[ply.UserId];
		double timeLeft = PlayerSettingsRepository.GetSellingConfirmationTime(ply.UserId) - (currentTime - dropTime).TotalSeconds;
		return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
	}

	public string GetPerkActivationTimeLeft(Player ply)
	{
		if (!ActivatePerkTimestamp.ContainsKey(ply.UserId)) return "0";
		DateTime currentTime = DateTime.UtcNow;
		DateTime activateTime = ActivatePerkTimestamp[ply.UserId];
		double timeLeft = PlayerSettingsRepository.GetActiveAbilityActivationTime(ply.UserId) - (currentTime - activateTime).TotalSeconds;
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
		if (!_recentHints.ContainsKey(player.UserId)) _recentHints[player.UserId] = new();
		DateTime currentTime = DateTime.Now;
		double hintAgeSeconds = PlayerSettingsRepository.GetHintAgeSeconds(player.UserId);
		DateTime expirationTime = currentTime.AddSeconds(hintAgeSeconds);
		_recentHints[player.UserId].Enqueue(new(hint, expirationTime));
		while (_recentHints[player.UserId].Count > PlayerSettingsRepository.GetHintLimit(player.UserId)) _recentHints[player.UserId].Dequeue();
	}
}