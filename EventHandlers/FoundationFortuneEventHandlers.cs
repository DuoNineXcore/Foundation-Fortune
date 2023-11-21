using System.Collections.Generic;
using System.Globalization;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Core.Models.Enums.Perks;
using FoundationFortune.API.Features;
using FoundationFortune.API.Features.Items.PerkItems;
using FoundationFortune.API.Features.Systems;

namespace FoundationFortune.EventHandlers;

public class FoundationFortuneEventHandlers
{
    public void UsedFoundationFortuneNPC(UsedFoundationFortuneNPCEventArgs ev)
    {
        var voiceChatUsageType = AudioPlayer.GetNpcVoiceChatUsageType(ev.Type, ev.Outcome);

        if (voiceChatUsageType != NpcVoiceChatUsageType.None)
        {
            var npcVoiceChatSettings = AudioPlayer.GetVoiceChatSettings(voiceChatUsageType);
            if (npcVoiceChatSettings != null) AudioPlayer.PlayAudio(ev.NPC, npcVoiceChatSettings.AudioFile, npcVoiceChatSettings.Volume, npcVoiceChatSettings.Loop, npcVoiceChatSettings.VoiceChat);
        }
    }

    public void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
    {
        string str = FoundationFortune.Singleton.Translation.DrankPerkBottle
            .Replace("%type%", ev.PerkType.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXpRewards.UsingPerkXPRewards.ToString())
            .Replace("%multiplier%",
                PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        if (!FoundationFortune.Singleton.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks))
        {
            playerPerks = new Dictionary<PerkType, int>();
            FoundationFortune.Singleton.ConsumedPerks[ev.Player] = playerPerks;
        }

        if (playerPerks.TryGetValue(ev.PerkType, out var count)) playerPerks[ev.PerkType] = count + 1;
        else playerPerks[ev.PerkType] = 1;

        PerkSystem.GrantPerk(ev.Player, ev.PerkType);
        FoundationFortune.Singleton.HintSystem.EnqueueHint(ev.Player, str);
        PerkBottle.PerkBottles.Remove(ev.Item.Serial);
    }

    public void BoughtItem(BoughtItemEventArgs ev)
    {
        string str = FoundationFortune.Singleton.Translation.BuyItemSuccess
            .Replace("%itemAlias%", ev.BuyableItem.Alias)
            .Replace("%itemPrice%", ev.BuyableItem.Price.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXpRewards.BuyingItemXPRewards.ToString())
            .Replace("%multiplier%", PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture)); //oh my god i dont fucking care about culture

        FoundationFortune.Singleton.HintSystem.EnqueueHint(ev.Player, str);

        ev.Player.AddItem(ev.BuyableItem.ItemType);

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyableItem.Price, true, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.BuyingItemXPRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyableItem);
    }

    public void BoughtPerk(BoughtPerkEventArgs ev)
    {
        string str = FoundationFortune.Singleton.Translation.BuyPerkSuccess
            .Replace("%perkAlias%", ev.BuyablePerk.Alias)
            .Replace("%perkPrice%", ev.BuyablePerk.Price.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXpRewards.BuyingPerkXPRewards.ToString())
            .Replace("%multiplier%",
                PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        FoundationFortune.Singleton.HintSystem.EnqueueHint(ev.Player, str);

        PerkBottle.GivePerkBottle(ev.Player, ev.BuyablePerk.PerkType);

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.BuyablePerk.Price, true, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.BuyingPerkXPRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.BuyablePerk);
    }

    public void SoldItem(SoldItemEventArgs ev)
    {
        var str = FoundationFortune.Singleton.Translation.SellItemSuccess
            .Replace("%price%", ev.SellableItem.Price.ToString())
            .Replace("%itemName%",
                FoundationFortune.SellableItemsList.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName)
            .Replace("%xpReward%", FoundationFortune.MoneyXpRewards.SellingXPRewards.ToString())
            .Replace("%multiplier%",
                PlayerDataRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        FoundationFortune.Singleton.HintSystem.EnqueueHint(ev.Player, str);

        ev.Player.RemoveItem(ev.Item);

        PlayerDataRepository.ModifyMoney(ev.Player.UserId, ev.SellableItem.Price, false, true, false);
        PlayerDataRepository.SetExperience(ev.Player.UserId, FoundationFortune.MoneyXpRewards.SellingXPRewards);
        IndexationMethods.AddToPlayerLimits(ev.Player, ev.SellableItem);
    }
}