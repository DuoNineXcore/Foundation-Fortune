using HarmonyLib;
using Exiled.API.Features;
using FoundationFortune.API.Items.PerkItems;
using FoundationFortune.API.NPCs;
using PlayerRoles.PlayableScps.Scp079;
using InventorySystem.Items.Usables;
using PlayerRoles.PlayableScps.Scp079.Map;

namespace FoundationFortune.Patches
{
	[HarmonyPatch(typeof(AntiScp207), nameof(AntiScp207.OnEffectsActivated))]
	internal static class Anti207Patch
	{
		private static bool Prefix(AntiScp207 __instance) => !PerkBottle.PerkBottles.ContainsKey(__instance.ItemSerial);
	}

    [HarmonyPatch(typeof(Scp079ScannerTracker), nameof(Scp079ScannerTracker.AddTarget))]
	internal static class Scp079TargetAddPatch
	{
		private static bool Prefix(Scp079ScannerTracker __instance, ReferenceHub hub) => hub is not null && NPCHelperMethods.IsFoundationFortuneNPC(hub);
	}

	[HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
	internal static class Scp079RecontainPatch
	{
		private static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub)
		{
			if (hub == null) return true;
			return !NPCHelperMethods.IsFoundationFortuneNPC(hub);
		}
	}
	
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
	internal static class TerminationPatch
	{
		private static bool Prefix(ReferenceHub scp)
		{
			var BuyingBot = Player.Get(scp);
			if (BuyingBot == null) return false;
			return !BuyingBot.IsNPC;
		}
	}
}
