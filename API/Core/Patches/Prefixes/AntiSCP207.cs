using FoundationFortune.API.Features.Items.PerkItems;
using HarmonyLib;
using InventorySystem.Items.Usables;

namespace FoundationFortune.API.Core.Patches.Prefixes;

[HarmonyPatch(typeof(AntiScp207), nameof(AntiScp207.OnEffectsActivated))]
internal static class Anti207Patch
{
    private static bool Prefix(AntiScp207 __instance) => !PerkBottle.PerkBottles.ContainsKey(__instance.ItemSerial);
}