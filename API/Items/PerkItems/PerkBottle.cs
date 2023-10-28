using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.Commands.BuyCommand;
using System.Collections.Generic;
using FoundationFortune.API.Perks;
using System.Text;
using FoundationFortune.API.Models.Classes;

namespace FoundationFortune.API.Items.PerkItems
{
	/// <summary>
	/// SODA!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
	/// </summary>
	[CustomItem(ItemType.AntiSCP207)]
	public class PerkBottle : CustomItem
	{
		public override uint Id { get; set; } = 331;
		public override string Name { get; set; } = "Perk Bottle";
		public override string Description { get; set; } = "";
		public override float Weight { get; set; } = 0f;
		public override SpawnProperties SpawnProperties { get; set; }
		public static Dictionary<ushort, (PerkType perkType, Player player)> DroppedPerkBottles = new();

		protected override void SubscribeEvents()
		{
			Exiled.Events.Handlers.Player.UsedItem += UsedPerkBottle;
			base.SubscribeEvents();
		}

		protected override void UnsubscribeEvents()
		{
			Exiled.Events.Handlers.Player.UsedItem -= UsedPerkBottle;
			base.UnsubscribeEvents();
		}

		private void UsedPerkBottle(UsedItemEventArgs ev)
		{
			if (!DroppedPerkBottles.TryGetValue(ev.Item.Serial, out var perkBottleData)) return;
			PerkType perkType = perkBottleData.perkType;
			PerkSystem.GrantPerk(ev.Player, perkType);

			if (!FoundationFortune.Singleton.ConsumedPerks.TryGetValue(ev.Player, out var playerPerks))
			{
				playerPerks = new Dictionary<PerkType, int>();
				FoundationFortune.Singleton.ConsumedPerks[ev.Player] = playerPerks;
			}

			if (playerPerks.TryGetValue(perkType, out var count)) playerPerks[perkType] = count + 1;
			else playerPerks[perkType] = 1;

			FoundationFortune.Singleton.ServerEvents.EnqueueHint(ev.Player, $"<b>You drank a <color=#FFC0CB>{perkType}</color> Perk bottle.</b>", 2f);
			DroppedPerkBottles.Remove(ev.Item.Serial);
		}


        public static void GivePerkBottle(Player player, PerkType perkType)
        {
	        if (!TrySpawn(331, player.Position, out Pickup perkBottle)) return;
	        Log.Debug($"Spawned Perk Bottle at Pos:{perkBottle.Position} Serial: {perkBottle.Serial}, PerkType: {perkType}");
	        DroppedPerkBottles[perkBottle.Serial] = (perkType, player);
        }

        protected override void OnAcquired(Player player, Item item, bool displayMessage)
        {
	        base.OnAcquired(player, item, false);
        }

        public static void GetHeldBottle(Player player, ref StringBuilder stringbuilder)
		{
			if (player.CurrentItem == null) return;
			if (DroppedPerkBottles.TryGetValue(player.CurrentItem.Serial, out var perkBottleData)) 
				stringbuilder.AppendLine($"<b>You are holding a <color=#FFC0CB>{perkBottleData.perkType}</color> perk bottle.</b>");
		}
	}
}
