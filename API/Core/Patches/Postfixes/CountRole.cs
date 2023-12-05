using System.Linq;
using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles;

namespace FoundationFortune.API.Core.Patches.Postfixes;

[HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.CountRole))]
internal static class CountRole
{
    private static void Postfix(RoleTypeId role, ref int __result) => __result -= Player.List.Count(player => player.IsNPC && player.Role.Type == role);
}