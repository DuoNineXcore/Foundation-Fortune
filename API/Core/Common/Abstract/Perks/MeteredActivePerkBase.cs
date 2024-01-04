using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using Exiled.API.Features;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.API.Features.Items.PerkItems;

namespace FoundationFortune.API.Core.Common.Abstract.Perks;

public abstract class MeteredActivePerkBase : IMeteredActivePerk
{
    public abstract PerkType PerkType { get; }
    public abstract string Alias { get; }
    public abstract float MaxMeterValue { get; }
    public abstract float MeterDepletionRate { get; }
    public abstract DateTime LastActivationTime { get; protected set; }
    public TimeSpan Cooldown { get; protected set; }
    public virtual bool IsOnCooldown => GetRemainingCooldown() > TimeSpan.Zero;
    public abstract bool IsCurrentlyActive { get; protected set; }

    public virtual TimeSpan GetRemainingCooldown()
    {
        DateTime currentTime = DateTime.UtcNow;
        TimeSpan remainingCooldown = LastActivationTime.Add(Cooldown) - currentTime;

        return remainingCooldown > TimeSpan.Zero ? remainingCooldown : TimeSpan.Zero;
    }

    public float CurrentMeterValue { get; private set; }

    public bool IsMeterFull => CurrentMeterValue >= MaxMeterValue;

    public abstract void ApplyPassiveEffect(Player player);
    public abstract void StartActivePerkAbility(Player player);

    public void FillMeter(float amount)
    {
        if (!IsMeterFull) CurrentMeterValue = Math.Min(MaxMeterValue, CurrentMeterValue + amount);
    }

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

        if (ev.Perk is ICooldownActivePerk or IMeteredActivePerk && playerPerks.Keys.Any(p => p is ICooldownActivePerk or IMeteredActivePerk))
        {
            FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, "You already have an active perk.");
            DirectoryIterator.Log($"Player {ev.Player.Nickname} attempted to use an active perk while already having one.", LogLevel.Warn);
            return;
        }

        if (playerPerks.ContainsKey(ev.Perk))
        {
            FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, $"You have already consumed {ev.Perk.Alias}");
            DirectoryIterator.Log($"Player {ev.Player.Nickname} attempted to consume the same perk ({ev.Perk.Alias}) multiple times.", LogLevel.Warn);
            return;
        }

        if (playerPerks.TryGetValue(ev.Perk, out var count)) playerPerks[ev.Perk] = count + 1;
        else playerPerks[ev.Perk] = 1;

        PerkSystem.GrantPerk(ev.Player, ev.Perk);
        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, str);
        PerkBottle.PerkBottles.Remove(ev.Item.Serial);

        ApplyPassiveEffect(ev.Player);

        DirectoryIterator.Log($"Player {ev.Player.Nickname} used perk {ev.Perk.Alias}.", LogLevel.Info);
    }
}