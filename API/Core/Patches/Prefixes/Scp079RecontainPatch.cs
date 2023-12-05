using FoundationFortune.API.Features.NPCs;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;

namespace FoundationFortune.API.Core.Patches.Prefixes;

[HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
internal static class Scp079RecontainPatch
{
    private static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub)
    {
        if (hub == null) return true;
        return !NpcHelperMethods.IsFoundationFortuneNpc(hub);
    }
}