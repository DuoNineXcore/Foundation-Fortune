﻿using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.EventArgs;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;
using System.Text;
using Discord;
using FoundationFortune.API.Events;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Enums.Perks;

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
		public static Dictionary<ushort, (PerkType perkType, Player player)> PerkBottles = new();

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
			if (!PerkBottles.TryGetValue(ev.Item.Serial, out var perkBottleData)) return;
			EventHelperMethods.RegisterOnUsedFoundationFortunePerk(ev.Player, perkBottleData.perkType, ev.Item);
		}
		
        public static void GivePerkBottle(Player player, PerkType perkType)
        {
	        CustomItem customItem = new PerkBottle();
	        var item = Item.Create(ItemType.AntiSCP207, player);

	        customItem.Give(player, item,false);
	        
	        FoundationFortune.Log($"Spawned Perk Bottle at {item.Serial}", LogLevel.Debug);
	        PerkBottles[item.Serial] = (perkType, player);
        }

        protected override void OnAcquired(Player player, Item item, bool displayMessage)
        {
	        base.OnAcquired(player, item, false);
        }

        public static void GetHeldBottle(Player player, ref StringBuilder stringbuilder)
		{
			if (player.CurrentItem == null) return;
			if (PerkBottles.TryGetValue(player.CurrentItem.Serial, out var perkBottleData)) stringbuilder.AppendLine(FoundationFortune.Singleton.Translation.HoldingPerkBottle.Replace("%type%", perkBottleData.perkType.ToString()));
		}
	}
}
