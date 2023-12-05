using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;

namespace FoundationFortune.API.Core.Patches.Postfixes;

[HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountTeam))]
internal static class CountTeam
{
    private static void Postfix(Team team, ref int __result) => __result -= Player.List.Count(player => player.IsNPC && player.Role.Team == team);
}