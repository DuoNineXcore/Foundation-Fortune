using FoundationFortune.API.Features.NPCs;
using HarmonyLib;

namespace FoundationFortune.API.Core.Patches.Prefixes;

[HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
internal static class TerminationPatch
{
    private static bool Prefix(ReferenceHub scp)
    {
        if (scp == null) return false;
        return !NPCHelperMethods.IsFoundationFortuneNpc(scp);
    }
}