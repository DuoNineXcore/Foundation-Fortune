using HarmonyLib;
using Exiled.API.Features;
using PlayerRoles;
using System.Linq;
using PlayerRoles.PlayableScps.Scp939.Ripples;

namespace FoundationFortune.Patches
{
    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountTeam))]
    internal static class CountTeam
    {
        private static void Postfix(Team team, ref int __result)
        {
            __result -= Player.List.Count(player => player.IsNPC && player.Role.Team == team);
        }
    }

    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountRole))]
    internal static class CountRole
    {
        private static void Postfix(RoleTypeId role, ref int __result)
        {
            __result -= Player.List.Count(player => player.IsNPC && player.Role.Type == role);
        }
    }
}
