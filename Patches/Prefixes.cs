using HarmonyLib;
using Exiled.API.Features;
using PlayerRoles.PlayableScps.Scp079;
using InventorySystem.Items.Usables;
using FoundationFortune.API.Items;

namespace FoundationFortune.Patches
{
	[HarmonyPatch(typeof(AntiScp207), nameof(AntiScp207.OnEffectsActivated))]
	internal static class Anti207Patch
	{
		private static bool Prefix(AntiScp207 __instance)
		{
			if (PerkBottle.DroppedPerkBottles.ContainsKey(__instance.ItemSerial)) return false;
			return true;
		}
	}

	[HarmonyPatch(typeof(Scp079ScannerTracker), nameof(Scp079ScannerTracker.AddTarget))]
	internal static class Scp079TargetAddPatch
	{
		private static bool Prefix(Scp079ScannerTracker __instance, ReferenceHub hub)
		{
			Player player = Player.Get(hub);
			if (player is null || player is not null && player.IsNPC) return false;
			return true;
		}
	}

	[HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
	internal static class Scp079RecontainPatch
	{
		private static bool Prefix(Scp079Recontainer __instance, ReferenceHub hub)
		{
			Player player = Player.Get(hub);
			if (player == null) return true;
			if (player.IsNPC) return false;

			return true;
		}
	}

	[HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
	internal static class TerminationPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(ReferenceHub scp)
		{
			var BuyingBot = Player.Get(scp);
			if (BuyingBot == null) return false;
			if (BuyingBot.IsNPC) return false;
			return true;
		}
	}
}
