using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Features.Systems.EventBasedSystems;
using PlayerRoles;

namespace FoundationFortune.API.Features.Items.CustomItems
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
            ActivateResurgenceBeacon(revivalInfo.RevivingPlayer, revivalInfo.PlayerToRevive.Nickname);
            revivalInfo.PlayerToRevive.RemoveItem(ev.Item, true);
            ev.IsAllowed = false;
        }
        
        public static bool GiveBeacon(Player giver, string targetPlayerUserId)
        {
            CustomItem customItem = new ResurgenceBeacon();
            Player targetPlayer = Player.Get(targetPlayerUserId);
            var item = Item.Create(ItemType.Radio, giver);
            customItem.Give(giver, item,false);
	        
            DirectoryIterator.Log($"Resurgence Beacon Given to Player: {giver.Nickname} Serial: {item.Serial} Set to revive: {targetPlayer.Nickname}", LogLevel.Debug);
            RevivalData[item.Serial] = (giver, targetPlayer);
            return true;
        }

        private static void ActivateResurgenceBeacon(Player reviver, string targetName)
        {
            var targetToRevive = Player.Get(targetName);
            if (targetToRevive is not { IsDead: true })
            {
                FoundationFortune.Instance.HintSystem.EnqueueHint(reviver, FoundationFortune.Instance.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName));
                return;
            }

            if (FoundationFortune.PerkSystemSettings.ResetRevivedInventory) targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
            else targetToRevive.Role.Set(reviver.Role);

            targetToRevive.Health = FoundationFortune.PerkSystemSettings.RevivedPlayerHealth;
            targetToRevive.Teleport(reviver.Position);

            if (FoundationFortune.PerkSystemSettings.HuntReviver) BountySystem.AddBounty(reviver, FoundationFortune.PerkSystemSettings.RevivalBountyKillReward, TimeSpan.FromSeconds(FoundationFortune.PerkSystemSettings.RevivalBountyTimeSeconds));
            
            foreach (var ply in Player.List.Where(p => !p.IsNPC))
            {
                FoundationFortune.Instance.HintSystem.EnqueueHint(ply, FoundationFortune.Instance.Translation.RevivalSuccess
                    .Replace("%rolecolor%", reviver.Role.Color.ToHex())
                    .Replace("%nickname%", reviver.Nickname)
                    .Replace("%target%", targetToRevive.Nickname));
            }
        }
    }
}
