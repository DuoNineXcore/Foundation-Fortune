using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Features;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using FoundationFortune.API.Features.Perks.Active;
using FoundationFortune.API.Features.Perks.Passive;
using MEC;

namespace FoundationFortune.API.Features.Systems
{
    public static class PerkSystem
    {
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
                .Replace("%perkType%", activePerks.FirstOrDefault().Key.PerkType.ToString());
        }

        public static void ClearConsumedPerks(Player player)
        {
            if (!ConsumedPerks.TryGetValue(player, out var perks) || perks == null) return;
            foreach (var kvp in perks.ToList()) RemovePerk(player, kvp.Key);
        }

        public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<IPerk, int>> consumedPerks, ref StringBuilder perkIndicator)
        {
            foreach (var perkEntry in consumedPerks.SelectMany(playerPerks => playerPerks.Value))
            {
                var (perk, count) = (perkEntry.Key, perkEntry.Value);
                if (FoundationFortune.PerkSystemSettings.PerkCounterEmojis.TryGetValue(perk.PerkType, out var emoji)) perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} ");
            }
            perkIndicator.AppendLine();
        }

        public static IPerk ToPerk(this PerkType perkType)
        {
            return perkType switch
            {
                PerkType.EthericVitality => new EthericVitality(),
                PerkType.HyperactiveBehavior => new HyperactiveBehavior(),
                PerkType.ViolentImpulses => new ViolentImpulses(),
                PerkType.BlissfulUnawareness => new BlissfulUnawareness(),
                PerkType.EtherealIntervention => new EtherealIntervention(),
                _ => throw new ArgumentException($"Unknown perk type: {perkType}")
            };
        }

        public static bool HasPerk(Player player, PerkType perkType)
        {
            return perkType switch
            {
                PerkType.EthericVitality => EthericVitality.EthericVitalityPlayers.Contains(player),
                PerkType.HyperactiveBehavior => HyperactiveBehavior.HyperactiveBehaviorPlayers.Contains(player),
                PerkType.ViolentImpulses => ViolentImpulses.ViolentImpulsesPlayers.Contains(player),
                PerkType.BlissfulUnawareness => BlissfulUnawareness.BlissfulUnawarenessPlayers.Contains(player),
                PerkType.EtherealIntervention => EtherealIntervention.EtherealInterventionPlayers.Contains(player),
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
        }
    }
}
