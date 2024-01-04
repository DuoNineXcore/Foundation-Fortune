using System.Globalization;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneNPCs;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.PerkItems;

namespace FoundationFortune.EventHandlers;

/// <summary>
/// the other leg
/// </summary>
public class FoundationFortuneEventHandlers
{
    public void UsedFoundationFortuneNpc(UsedFoundationFortuneNpcEventArgs ev)
    {
        var voiceChatUsageType = AudioPlayer.GetNpcVoiceChatUsageType(ev.Type, ev.Outcome);
        if (voiceChatUsageType == NpcVoiceChatUsageType.None) return;
        var npcVoiceChatSettings = AudioPlayer.GetVoiceChatSettings(voiceChatUsageType);
        if (npcVoiceChatSettings != null) AudioPlayer.PlayAudio(ev.Npc, npcVoiceChatSettings.AudioFile, npcVoiceChatSettings.Volume, npcVoiceChatSettings.Loop, npcVoiceChatSettings.VoiceChat);
    }

    public void BoughtItem(BoughtItemEventArgs ev)
    {
        var touchOfMidasMultiplier = PerkSystem.HasPerk(ev.Player, PerkType.TouchOfMidas) ? 1.5 : 1;
        var prestigeMultiplier = PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId);
        var prestigeNegativeMultiplier = 1 / prestigeMultiplier;

        var modifiedPrice = ev.BuyableItem.Price * touchOfMidasMultiplier * prestigeNegativeMultiplier;

        string str = FoundationFortune.Instance.Translation.BuyItemSuccess
            .Replace("%itemAlias%", ev.BuyableItem.Alias)
            .Replace("%itemPrice%", $"{modifiedPrice:F2}")
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.BuyingItemXpRewards.ToString())
            .Replace("%multiplier%", $"{touchOfMidasMultiplier:F2} [Touch of Midas] * {prestigeNegativeMultiplier:F2} [Prestige Negative]");

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        QuestSystem.UpdateQuestProgress(ev.Player, QuestType.BuyItems, 1);
        ev.Player.AddItem(ev.BuyableItem.ItemType);

        PlayerStatsRepository.ModifyMoney(ev.Player.UserId, (int)modifiedPrice, true, true, false);
        PlayerStatsRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.BuyingItemXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyableItem);
    }

    public void BoughtPerk(BoughtPerkEventArgs ev)
    {
        var touchOfMidasMultiplier = PerkSystem.HasPerk(ev.Player, PerkType.TouchOfMidas) ? 1.5 : 1;
        var prestigeMultiplier = PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId);
        var prestigeNegativeMultiplier = 1 / prestigeMultiplier;

        var modifiedPrice = ev.BuyablePerk.Price * touchOfMidasMultiplier * prestigeNegativeMultiplier;

        string str = FoundationFortune.Instance.Translation.BuyPerkSuccess
            .Replace("%perkAlias%", ev.BuyablePerk.Alias)
            .Replace("%perkPrice%", $"{modifiedPrice:F2}")
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.BuyingPerkXpRewards.ToString())
            .Replace("%multiplier%", $"{touchOfMidasMultiplier:F2} [Touch of Midas] * {prestigeNegativeMultiplier:F2} [Prestige Multiplier]");

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        QuestSystem.UpdateQuestProgress(ev.Player, QuestType.BuyItems, 1);
        PerkBottle.GivePerkBottle(ev.Player, ev.BuyablePerk.PerkType.ToPerk());

        PlayerStatsRepository.ModifyMoney(ev.Player.UserId, (int)modifiedPrice, true, true, false);
        PlayerStatsRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.BuyingPerkXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyablePerk);
    }

    public void SoldItem(SoldItemEventArgs ev)
    {
        var prestigeMultiplier = PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId);
        var touchOfMidasMultiplier = PerkSystem.HasPerk(ev.Player, PerkType.TouchOfMidas) ? 2 : 1;
        var modifiedPrice = ev.SellableItem.Price * touchOfMidasMultiplier * prestigeMultiplier;

        var str = FoundationFortune.Instance.Translation.SellItemSuccess
            .Replace("%price%", $"+{modifiedPrice:F2}")
            .Replace("%moneyMultiplier%", $"*{touchOfMidasMultiplier} [Touch of Midas]*{prestigeMultiplier:F2} [Prestige Multiplier]")
            .Replace("%itemName%", ev.SellableItem.DisplayName)
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.SellingXpRewards.ToString())
            .Replace("%xpMultiplier%", prestigeMultiplier.ToString("F2", CultureInfo.InvariantCulture));

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        PlayerStatsRepository.ModifyMoney(ev.Player.UserId, (int)modifiedPrice, false, true, false);
        PlayerStatsRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.SellingXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.SellableItem);
    }
}