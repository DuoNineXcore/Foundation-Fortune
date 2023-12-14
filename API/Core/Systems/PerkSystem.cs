using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Features.Perks.Active;
using FoundationFortune.API.Features.Perks.Passive;
using MEC;
using UnityEngine;

namespace FoundationFortune.API.Core.Systems;

public static class PerkSystem
{
    public static readonly Dictionary<PerkType, List<Player>> PerkPlayers = new Dictionary<PerkType, List<Player>>
    {
        { PerkType.ViolentImpulses, new List<Player>() },
        { PerkType.ExplosiveResilience, new List<Player>() },
        { PerkType.TouchOfMidas, new List<Player>() },
        { PerkType.EtherealIntervention, new List<Player>() },
        { PerkType.HyperactiveBehavior, new List<Player>() },
        { PerkType.EthericVitality, new List<Player>() },
        { PerkType.BlissfulAgony, new List<Player>() },
        { PerkType.SacrificialSurge, new List<Player>() },
        { PerkType.VitalitySacrifice, new List<Player>() },
        { PerkType.GuardiansGrace, new List<Player>() }
    };

    public static readonly Dictionary<Player, Dictionary<IPerk, int>> ConsumedPerks = new();
    public static readonly Dictionary<Player, Dictionary<IActivePerk, int>> ConsumedActivePerks = new();
    private static Dictionary<Player, Dictionary<IPerk, CoroutineHandle>> PerkCoroutines = new();
        
    public static void GrantPerk(Player ply, IPerk perk)
    {
        var coroutineHandle = new CoroutineHandle();
        if (!PerkCoroutines.TryGetValue(ply, out var coroutineDict))
        {
            coroutineDict = new Dictionary<IPerk, CoroutineHandle>();
            PerkCoroutines[ply] = coroutineDict;
        }

        coroutineDict[perk] = coroutineHandle;
    }

    public static void UpdateActivePerkMessages(Player ply, ref StringBuilder hintMessage)
    {
        if (!FoundationFortune.Instance.HintSystem.ConfirmActivatePerk.ContainsKey(ply.UserId) || !FoundationFortune.Instance.HintSystem.ConfirmActivatePerk[ply.UserId]) return;
        if (!ConsumedActivePerks.TryGetValue(ply, out var activePerks)) return;

        hintMessage.Append($"{FoundationFortune.Instance.Translation.ConfirmPerkActivation
            .Replace("%time%", FoundationFortune.Instance.HintSystem.GetPerkActivationTimeLeft(ply))}")
            .Replace("%perkAlias%", activePerks.FirstOrDefault().Key.Alias);
    }

    public static void ClearConsumedPerks(Player player)
    {
        if (!ConsumedPerks.TryGetValue(player, out var perks) || perks == null) return;
        foreach (var kvp in perks.ToList()) RemovePerk(player, kvp.Key);
    }
        
    public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<IPerk, int>> consumedPerks, ref StringBuilder perkIndicator)
    {
        var activePerks = new List<(IActivePerk, int)>();
        var passivePerks = new List<(IPerk, int)>();

        foreach (var perkEntry in consumedPerks.SelectMany(playerPerks => playerPerks.Value))
        {
            var (perk, count) = (perkEntry.Key, perkEntry.Value);

            if (perk is IActivePerk activePerk) activePerks.Add((activePerk, count));
            else passivePerks.Add((perk, count));
        }

        foreach (var (perk, count) in passivePerks)
        {
            if (FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(perk.PerkType, out var emoji)) perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} ");
        }

        if (passivePerks.Count > 0 && activePerks.Count > 0) perkIndicator.Append(" / ");

        foreach (var (activePerk, count) in activePerks)
        {
            if (!FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(activePerk.PerkType, out var emoji)) continue;
    
            string color;
    
            if (activePerk.isCurrentlyActive) color = "white";
            else color = activePerk.IsOnCooldown ? "red" : "green";
            
            perkIndicator.Append(count > 1 ? $"<color={color}>{emoji}x{count} ({GetCooldownTimer(activePerk)})</color> " : $"<color={color}>{emoji} ({GetCooldownTimer(activePerk)})</color> ");
        }

        perkIndicator.AppendLine();
    }
        
    private static string GetCooldownTimer(IActivePerk activePerk)
    {
        var remainingCooldown = activePerk.GetRemainingCooldown();
        var minutes = Mathf.FloorToInt(remainingCooldown / 60);
        var seconds = Mathf.FloorToInt(remainingCooldown % 60);
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
            PerkType.EtherealIntervention => new EtherealIntervention(),
            PerkType.SacrificialSurge => new SacrificialSurge(),
            PerkType.VitalitySacrifice => new VitalitySacrifice(),
            PerkType.ExplosiveResilience => new ExplosiveResilience(),
            PerkType.GuardiansGrace => new GuardiansGrace(),
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
            PerkType.EtherealIntervention => PerkPlayers[PerkType.EtherealIntervention].Contains(player),
            _ => false
        };
    }
        
    public static void RemovePerk(Player player, IPerk perk)
    {
        if (!ConsumedPerks.TryGetValue(player, out var playerPerks) || playerPerks == null) return;
        if (PerkCoroutines.TryGetValue(player, out var coroutineHandle) && coroutineHandle.TryGetValue(perk, out var specificCoroutine))
        {
            Timing.KillCoroutines(specificCoroutine);
            coroutineHandle.Remove(perk);
        }
        playerPerks.Remove(perk);
        PerkPlayers[perk.PerkType].Remove(player);
    }
}