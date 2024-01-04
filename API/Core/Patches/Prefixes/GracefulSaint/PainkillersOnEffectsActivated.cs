using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Systems;
using HarmonyLib;
using InventorySystem.Items.Usables;

namespace FoundationFortune.API.Core.Patches.Prefixes.GracefulSaint;

[HarmonyPatch(typeof(Painkillers), nameof(Painkillers.OnEffectsActivated))]
internal static class PainkillersPatch
{
    private static bool Prefix(Painkillers __instance) => !PerkSystem.HasPerk(Exiled.API.Features.Player.Get(__instance.Owner), PerkType.EthericVitality);
}