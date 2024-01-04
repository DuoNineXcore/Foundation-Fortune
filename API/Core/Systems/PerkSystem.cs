using System;
using System.Collections.Generic;
using System.Text;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Perks;
using FoundationFortune.API.Features.Perks.Active.CooldownActive;
using FoundationFortune.API.Features.Perks.Active.MeteredActive;
using FoundationFortune.API.Features.Perks.Passive;
using MEC;

namespace FoundationFortune.API.Core.Systems;

public static class PerkSystem
{
    public static readonly Dictionary<PerkType, List<Player>> PerkPlayers = new()
    {
        { PerkType.ViolentImpulses, new() },
        { PerkType.ExplosiveResilience, new() },
        { PerkType.TouchOfMidas, new() },
        { PerkType.HyperactiveBehavior, new() },
        { PerkType.EthericVitality, new() },
        { PerkType.BlissfulAgony, new() },
        { PerkType.SacrificialSurge, new() },
        { PerkType.VitalitySacrifice, new() },
        { PerkType.GracefulSaint, new() }
    };

    public static readonly Dictionary<Player, Dictionary<IPerk, int>> ConsumedPerks = new();
    private static Dictionary<Player, Dictionary<IPerk, CoroutineHandle>> PerkCoroutines = new();
    public static readonly Dictionary<Player, Dictionary<ICooldownActivePerk, int>> ConsumedCooldownActivePerks = new();
    public static readonly Dictionary<Player, Dictionary<IMeteredActivePerk, int>> ConsumedMeteredActivePerks = new();

    public static void UpdateActivePerkMessages(Player ply, ref StringBuilder hintMessage)
    {
        if (!FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.ContainsKey(ply.UserId) || !FoundationFortune.Instance.HintSystem.ConfirmActivatePerk[ply.UserId]) return;

        hintMessage.Append($"{FoundationFortune.Instance.Translation.ConfirmPerkActivation
            .Replace("%time%", FoundationFortune.Instance.HintSystem.GetPerkActivationTimeLeft(ply))}");

        if (ConsumedCooldownActivePerks.TryGetValue(ply, out var cooldownPerks)) foreach (var perk in cooldownPerks.Keys) hintMessage.Append($"{perk.Alias} Cooldown: {GetCooldownTimer(perk)} ");
        if (ConsumedMeteredActivePerks.TryGetValue(ply, out var meteredPerks)) foreach (var perk in meteredPerks.Keys) hintMessage.Append($"{perk.Alias} Meter: {perk.CurrentMeterValue}/{perk.MaxMeterValue} ");
    }
    
    public static void HandlePerkActivation<T>(TogglingNoClipEventArgs ev, T activePerk, int cooldownSeconds) where T : class
    {
        if (!FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.ContainsKey(ev.Player.UserId))
        {
            FoundationFortune.Instance.HintSystem.ConfirmActivatePerk[ev.Player.UserId] = true;
            FoundationFortune.Instance.HintSystem.ActivatePerkTimestamp[ev.Player.UserId] = DateTime.UtcNow;
            ev.IsAllowed = false;
            return;
        }

        if (FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.TryGetValue(ev.Player.UserId, out bool isConfirming) &&
            FoundationFortune.Instance.HintSystem.ActivatePerkTimestamp.TryGetValue(ev.Player.UserId, out DateTime toggleTime))
        {
            if (isConfirming && DateTime.UtcNow - toggleTime <= TimeSpan.FromSeconds(cooldownSeconds))
            {
                (activePerk as ICooldownActivePerk)?.StartActivePerkAbility(ev.Player);
                (activePerk as IMeteredActivePerk)?.StartActivePerkAbility(ev.Player);
                FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.Remove(ev.Player.UserId);
            }
        }

        ev.IsAllowed = true;
    }

    public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<IPerk, int>> consumedPerks, ref StringBuilder perkIndicator)
    {
        foreach (var playerPerks in consumedPerks)
        {
            var player = playerPerks.Key;
            var meteredActivePerks = new List<(IMeteredActivePerk, int)>();
            var cooldownActivePerks = new List<(ICooldownActivePerk, int)>();
            var passivePerks = new List<(IPerk, int)>();

            foreach (var perkEntry in playerPerks.Value)
            {
                var (perk, count) = (perkEntry.Key, perkEntry.Value);

                switch (perk)
                {
                    case IMeteredActivePerk meteredPerk: meteredActivePerks.Add((meteredPerk, count)); break;
                    case ICooldownActivePerk cooldownPerk: cooldownActivePerks.Add((cooldownPerk, count)); break;
                    default: passivePerks.Add((perk, count)); break;
                }
            }

            foreach (var (perk, count) in passivePerks) 
                if (FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(perk.PerkType, out var emoji))
                    perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} ");

            if (passivePerks.Count > 0 && (meteredActivePerks.Count > 0 || cooldownActivePerks.Count > 0)) perkIndicator.Append(" / ");

            foreach (var (meteredPerk, count) in meteredActivePerks)
            {
                if (!FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(meteredPerk.PerkType, out var emoji)) continue;
                string color;
                string meterOrCooldownInfo;
                if (meteredPerk.IsCurrentlyActive)
                {
                    color = "white";
                    meterOrCooldownInfo = $"{meteredPerk.CurrentMeterValue}/{meteredPerk.MaxMeterValue}";
                }
                else if (meteredPerk.IsOnCooldown)
                {
                    color = "red";
                    meterOrCooldownInfo = GetCooldownTimer(meteredPerk);
                }
                else
                {
                    color = player.Role.Color.ToHex();
                    meterOrCooldownInfo = $"{meteredPerk.CurrentMeterValue}/{meteredPerk.MaxMeterValue}";
                }

                string meterBar = GetMeterBar(meteredPerk);
                perkIndicator.Append(count > 1
                    ? $"<color={color}>{emoji}x{count} {meterBar} {meterOrCooldownInfo}</color> "
                    : $"<color={color}>{emoji} {meterBar} {meterOrCooldownInfo}</color> ");
            }

            foreach (var (cooldownPerk, count) in cooldownActivePerks)
            {
                if (!FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(cooldownPerk.PerkType,
                        out var emoji)) continue;
                var color = cooldownPerk.IsOnCooldown ? "red" : player.Role.Color.ToHex();
                perkIndicator.Append(count > 1
                    ? $"<color={color}>{emoji}x{count} ({GetCooldownTimer(cooldownPerk)})</color> "
                    : $"<color={color}>{emoji} ({GetCooldownTimer(cooldownPerk)})</color> ");
            }

            perkIndicator.AppendLine();
        }
    }
    
    private static string GetMeterBar(IMeteredActivePerk meteredPerk)
    {
        float meterFillPercentage = meteredPerk.CurrentMeterValue / meteredPerk.MaxMeterValue;
        const int barLength = 10;
        int filledLength = (int)(barLength * meterFillPercentage);
        int emptyLength = barLength - filledLength;

        string filledBar = new('\u2588', filledLength);
        string emptyBar = new(' ', emptyLength);

        return $"[{filledBar}{emptyBar}]";
    }
        
    private static string GetCooldownTimer(ICooldownActivePerk activePerk)
    {
        var remainingCooldown = activePerk.GetRemainingCooldown();
        var minutes = (int)remainingCooldown.TotalMinutes;
        var seconds = (int)remainingCooldown.TotalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
    
    private static string GetCooldownTimer(IMeteredActivePerk meteredPerk)
    {
        var remainingCooldown = meteredPerk.GetRemainingCooldown();
        var minutes = (int)remainingCooldown.TotalMinutes;
        var seconds = (int)remainingCooldown.TotalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    public static IPerk ToPerk(this PerkType perkType)
    {
        return perkType switch
        {
            PerkType.EthericVitality => new EthericVitality(),
            PerkType.HyperactiveBehavior => new HyperactiveBehavior(),
            PerkType.ViolentImpulses => new ViolentImpulses(),
            PerkType.BlissfulAgony => new BlissfulAgony(),
            PerkType.SacrificialSurge => new SacrificialSurge(),
            PerkType.VitalitySacrifice => new VitalitySacrifice(),
            PerkType.ExplosiveResilience => new ExplosiveResilience(),
            PerkType.GracefulSaint => new GracefulSaint(),
            PerkType.TouchOfMidas => new TouchOfMidas(),
            _ => throw new ArgumentException($"Unknown perk type: {perkType}")
        };
    }

    public static bool HasPerk(Player player, PerkType perkType)
    {
        return perkType switch
        {
            PerkType.EthericVitality => PerkPlayers[PerkType.EthericVitality].Contains(player),
            PerkType.HyperactiveBehavior => PerkPlayers[PerkType.HyperactiveBehavior].Contains(player),
            PerkType.ViolentImpulses => PerkPlayers[PerkType.ViolentImpulses].Contains(player),
            PerkType.BlissfulAgony => PerkPlayers[PerkType.BlissfulAgony].Contains(player),
            PerkType.SacrificialSurge => PerkPlayers[PerkType.SacrificialSurge].Contains(player),
            PerkType.VitalitySacrifice => PerkPlayers[PerkType.VitalitySacrifice].Contains(player),
            PerkType.ExplosiveResilience => PerkPlayers[PerkType.ExplosiveResilience].Contains(player),
            PerkType.GracefulSaint => PerkPlayers[PerkType.GracefulSaint].Contains(player),
            PerkType.TouchOfMidas => PerkPlayers[PerkType.TouchOfMidas].Contains(player),
            _ => false
        };
    }
        
    public static void GrantPerk(Player ply, IPerk perk)
    {
        var coroutineHandle = new CoroutineHandle();
        if (!PerkCoroutines.TryGetValue(ply, out var coroutineDict))
        {
            coroutineDict = new();
            PerkCoroutines[ply] = coroutineDict;
        }

        coroutineDict[perk] = coroutineHandle;
    }

    public static void RemovePerk(Player player, IPerk perk)
    {
        if (!ConsumedPerks.TryGetValue(player, out var playerPerks) || playerPerks == null) return;
        if (PerkCoroutines.TryGetValue(player, out var coroutineHandle) && coroutineHandle.TryGetValue(perk, out var specificCoroutine))
        {
            Timing.KillCoroutines(specificCoroutine);
            coroutineHandle.Remove(perk);
        }

        switch (perk)
        {
            case PassivePerkBase passivePerk: passivePerk.UnsubscribeEvents(); break;
            case CooldownActivePerkBase activePerk: activePerk.UnsubscribeEvents(); break;
        }

        playerPerks.Remove(perk);
        PerkPlayers[perk.PerkType].Remove(player);
    }
}