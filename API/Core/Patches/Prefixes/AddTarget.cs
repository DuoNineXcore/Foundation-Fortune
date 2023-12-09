using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;

namespace FoundationFortune.API.Core.Patches.Prefixes;

[HarmonyPatch(typeof(Scp079ScannerTracker), nameof(Scp079ScannerTracker.AddTarget))]
internal static class Scp079TargetAddPatch
{
    private static bool Prefix(Scp079ScannerTracker __instance, ReferenceHub hub) => hub is not null && NPCHelperMethods.IsFoundationFortuneNpc(hub);
}