using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Systems;
using HarmonyLib;
using InventorySystem.Items.Usables;

namespace FoundationFortune.API.Core.Patches.Prefixes.GracefulSaint;

[HarmonyPatch(typeof(Medkit), nameof(Medkit.OnEffectsActivated))]
internal static class MedkitPatch
{
    private static bool Prefix(Medkit __instance) => !PerkSystem.HasPerk(Exiled.API.Features.Player.Get(__instance.Owner), PerkType.EthericVitality);
}