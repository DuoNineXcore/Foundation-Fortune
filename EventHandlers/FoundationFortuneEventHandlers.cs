using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CameraShaking;
using FoundationFortune.API.Common.Enums.NPCs;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneNPCs;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.PerkItems;
using FoundationFortune.API.Features.Perks;

namespace FoundationFortune.EventHandlers;

/// <summary>
/// the other leg
/// </summary>
public class FoundationFortuneEventHandlers
{
    public RecoilSettings RecoilSettings;

    public void UsedFoundationFortuneNpc(UsedFoundationFortuneNpcEventArgs ev)
    {
        var voiceChatUsageType = AudioPlayer.GetNpcVoiceChatUsageType(ev.Type, ev.Outcome);
        if (voiceChatUsageType == NpcVoiceChatUsageType.None) return;
        var npcVoiceChatSettings = AudioPlayer.GetVoiceChatSettings(voiceChatUsageType);
        if (npcVoiceChatSettings != null) AudioPlayer.PlayAudio(ev.Npc, npcVoiceChatSettings.AudioFile, npcVoiceChatSettings.Volume, npcVoiceChatSettings.Loop, npcVoiceChatSettings.VoiceChat);
    }

    public void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
    {
        string str = FoundationFortune.Instance.Translation.DrankPerkBottle
            .Replace("%type%", ev.Perk.PerkType.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.UsingPerkXpRewards.ToString())
            .Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        if (!PerkSystem.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks))
        {
            playerPerks = new Dictionary<IPerk, int>();
            PerkSystem.ConsumedPerks[ev.Player] = playerPerks;
        }
            
        if (ev.Perk is IActivePerk && playerPerks.Keys.Any(p => p is IActivePerk))
        {
            FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, "You already have an active perk.");
            return;
        }
            
        if (playerPerks.ContainsKey(ev.Perk))
        {
            FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, $"You have already consumed {ev.Perk.Alias}");
            return;
        }

        if (ev.Perk.PerkType == PerkType.ViolentImpulses) RecoilSettings = new(
            FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilAnimationTime, 
            FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilZAxis, 
            FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilFovKick, 
            FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilUpKick, 
            FoundationFortune.PerkSystemSettings.ViolentImpulsesRecoilSideKick);

        if (playerPerks.TryGetValue(ev.Perk, out var count)) playerPerks[ev.Perk] = count + 1;
        else playerPerks[ev.Perk] = 1;

        PerkSystem.GrantPerk(ev.Player, ev.Perk);
        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        PerkBottle.PerkBottles.Remove(ev.Item.Serial);

        if (ev.Perk is IActivePerk activePerk) activePerk.ApplyPassiveEffect(ev.Player);
        if (ev.Perk is IPassivePerk passivePerk) passivePerk.ApplyPassiveEffect(ev.Player);
    }

    public void BoughtItem(BoughtItemEventArgs ev)
    {
        string str = FoundationFortune.Instance.Translation.BuyItemSuccess
            .Replace("%itemAlias%", ev.BuyableItem.Alias)
            .Replace("%itemPrice%", ev.BuyableItem.Price.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.BuyingItemXpRewards.ToString())
            .Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture)); //oh my god i dont fucking care about culture

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        QuestSystem.UpdateQuestProgress(ev.Player, QuestType.BuyItems, 1);
        ev.Player.AddItem(ev.BuyableItem.ItemType);

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyableItem.Price, true, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.BuyingItemXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyableItem);
    }

    public void BoughtPerk(BoughtPerkEventArgs ev)
    {
        string str = FoundationFortune.Instance.Translation.BuyPerkSuccess
            .Replace("%perkAlias%", ev.BuyablePerk.Alias)
            .Replace("%perkPrice%", ev.BuyablePerk.Price.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.BuyingPerkXpRewards.ToString())
            .Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        QuestSystem.UpdateQuestProgress(ev.Player, QuestType.BuyItems, 1);
        PerkBottle.GivePerkBottle(ev.Player, ev.BuyablePerk.PerkType.ToPerk());

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyablePerk.Price, true, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.BuyingPerkXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyablePerk);
    }

    public void SoldItem(SoldItemEventArgs ev)
    {
        var str = FoundationFortune.Instance.Translation.SellItemSuccess
            .Replace("%price%", ev.SellableItem.Price.ToString())
            .Replace("%itemName%", FoundationFortune.SellableItemsList.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName)
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.SellingXpRewards.ToString())
            .Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        ev.Player.RemoveItem(ev.Item);

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.SellableItem.Price, false, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXPRewards.SellingXpRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.SellableItem);
    }
}