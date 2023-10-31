using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;
using FoundationFortune.API.Perks;

namespace FoundationFortune.API.Items.PerkItems
{
    [CustomItem(ItemType.Radio)]
    public class ResurgenceBeacon : CustomItem
    {
        public override uint Id { get; set; } = 332;
        public override string Name { get; set; } = "Resurgence Beacon";
        public override string Description { get; set; } = "Revive someone with it";
        public override float Weight { get; set; } = 0f;
        public override SpawnProperties SpawnProperties { get; set; }
        private static readonly Dictionary<ushort, (Player RevivingPlayer, Player PlayerToRevive)> RevivalData = new();

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingRadioBattery += RevivePlayer;
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.UsingRadioBattery -= RevivePlayer;
        }

        private void RevivePlayer(UsingRadioBatteryEventArgs ev)
        {
            if (!RevivalData.TryGetValue(ev.Item.Serial, out var revivalInfo) || !ev.Radio.IsEnabled) return;
            PerkSystem.ActivateResurgenceBeacon(revivalInfo.RevivingPlayer, revivalInfo.PlayerToRevive.Nickname);
            revivalInfo.PlayerToRevive.RemoveItem(ev.Item, true);
            ev.IsAllowed = false;
        }

        public static bool SpawnResurgenceBeacon(Player giver, string targetPlayerUserId)
        {
            Player targetPlayer = Player.Get(targetPlayerUserId);
            if (targetPlayer == null) return false;

            if (!TrySpawn(332, giver.Position, out Pickup resurgencebeacon)) return false;
            if (resurgencebeacon != null)
            {
                Log.Debug($"Spawned Resurgence Beacon at Pos:{resurgencebeacon.Position} Serial: {resurgencebeacon.Serial}, Player to be revived: {targetPlayer}");
                RevivalData[resurgencebeacon.Serial] = (giver, targetPlayer);
            }
            return true;
        }
    }
}
