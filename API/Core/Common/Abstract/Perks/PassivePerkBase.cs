using System.Collections.Generic;
using System.Globalization;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features.Items.PerkItems;

namespace FoundationFortune.API.Core.Common.Abstract.Perks;

public abstract class PassivePerkBase : IPassivePerk
{
    public abstract PerkType PerkType { get; }
    public abstract string Alias { get; }
    public abstract void ApplyPassiveEffect(Player player);

    public virtual void SubscribeEvents()
    {
        FoundationFortunePerkEvents.UsedFoundationFortunePerk += UsedFoundationFortunePerk;
    }

    public virtual void UnsubscribeEvents()
    {
        FoundationFortunePerkEvents.UsedFoundationFortunePerk -= UsedFoundationFortunePerk;
    }

    protected virtual void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
    {
        string str = FoundationFortune.Instance.Translation.DrankPerkBottle
            .Replace("%type%", ev.Perk.PerkType.ToString())
            .Replace("%xpReward%", FoundationFortune.MoneyXPRewards.UsingPerkXpRewards.ToString())
            .Replace("%multiplier%", PlayerStatsRepository.GetPrestigeMultiplier(ev.Player.UserId).ToString(CultureInfo.InvariantCulture));

        if (!PerkSystem.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks))
        {
            playerPerks = new();
            PerkSystem.ConsumedPerks[ev.Player] = playerPerks;
        }

        if (playerPerks.ContainsKey(ev.Perk))
        {
            FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, $"You have already consumed {ev.Perk.Alias}");
            return;
        }

        if (playerPerks.TryGetValue(ev.Perk, out var count)) playerPerks[ev.Perk] = count + 1;
        else playerPerks[ev.Perk] = 1;

        PerkSystem.GrantPerk(ev.Player, ev.Perk);
        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        PerkBottle.PerkBottles.Remove(ev.Item.Serial);

        ApplyPassiveEffect(ev.Player);
    }
}