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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.API.Items
{
    [CustomItem(ItemType.AntiSCP207)]
    public class PerkBottle : CustomItem
    {
        public override uint Id { get; set; } = 134;
        public override string Name { get; set; } = "Perk Bottle";
        public override string Description { get; set; } = "This is just, cod zombies.";
        public override float Weight { get; set; } = 0f;
        public override SpawnProperties SpawnProperties { get; set; }
        private Dictionary<int, (PerkType perkType, Player player)> droppedPerkBottles = new();
        private Perks perks = new();

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
            if (droppedPerkBottles.TryGetValue(ev.Item.Serial, out var perkBottleData))
            {
                PerkType perkType = perkBottleData.perkType;
                perks.GrantPerk(ev.Player, perkType);
                FoundationFortune.Singleton.serverEvents.EnqueueHint(ev.Player, $"You drank a {perkType} Perk bottle.", 5f);
                droppedPerkBottles.Remove(ev.Item.Serial);
            }
            ev.Player.DisableEffect<AntiScp207>();
            ev.Player.RemoveItem(ev.Item);
        }

        public void GivePerkBottle(Player player, PerkType perkType)
        {
            if (TrySpawn(Id, player.Position, out Pickup perkBottle))
            {
                Log.Debug($"Spawned Perk Bottle at Pos:{perkBottle.Position} Serial: {perkBottle.Serial}, PerkType: {perkType}");
                droppedPerkBottles[perkBottle.Serial] = (perkType, player);
            }
        }

        protected override void OnAcquired(Player player, Item item, bool displayMessage)
        {
            base.OnAcquired(player, item, false);
        }
    }
}
