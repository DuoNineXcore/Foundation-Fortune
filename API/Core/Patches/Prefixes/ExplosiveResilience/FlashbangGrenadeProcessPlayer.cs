using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Systems;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;

namespace FoundationFortune.API.Core.Patches.Prefixes.ExplosiveResilience;

[HarmonyPatch(typeof(FlashbangGrenade), nameof(FlashbangGrenade.ProcessPlayer))]
internal static class FlashbangGrenadePatch
{
    private static bool Prefix(FlashbangGrenade __instance, ReferenceHub hub) => !PerkSystem.HasPerk(Exiled.API.Features.Player.Get(hub), PerkType.ExplosiveResilience);
}