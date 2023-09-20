using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Database;
using FoundationFortune.Events;
using Exiled.API.Features.Items;
using System.Collections.Generic;
using System.Linq;
using InventorySystem;
using Exiled.CustomItems;

namespace FoundationFortune.API.Items
{
    [CustomItem(ItemType.Coin)]
    public class Money : CustomItem
    {
        public override uint Id { get; set; } = 132;
        public override string Name { get; set; } = "Money";
        public override string Description { get; set; } = "It's just money.";
        public override float Weight { get; set; } = 0f;
        public override SpawnProperties SpawnProperties { get; set; }
        private Dictionary<int, (int coinValue, Player player)> droppedCoins = new Dictionary<int, (int coinValue, Player player)>();

        protected override void SubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Dying += OnDeath;
            base.SubscribeEvents();
        }

        protected override void UnsubscribeEvents()
        {
            Exiled.Events.Handlers.Player.Dying -= OnDeath;
            base.UnsubscribeEvents();
        }

        private void OnDeath(DyingEventArgs ev)
        {
            int moneyBeforeDeath = PlayerDataRepository.GetMoneyOnHold(ev.Player.UserId);
            if (moneyBeforeDeath > 0)
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(ev.Player, $"<size=24><color=red>$-{moneyBeforeDeath}.</color> You died.</size>", 0, 5f, false, false);

                PlayerDataRepository.EmptyMoneyOnHold(ev.Player.UserId);

                int coinValue = moneyBeforeDeath / FoundationFortune.Singleton.Config.CoinsToDrop;

                for (int i = 0; i < FoundationFortune.Singleton.Config.CoinsToDrop; i++)
                {
                    if (TrySpawn(Id, ev.Player.Position, out Pickup coin))
                    {
                        Log.Debug($"Spawned coin at Pos:{coin.Position} Rot:{coin.Rotation}, Serial: {coin.Serial}, Value: {coinValue}");
                        droppedCoins[coin.Serial] = (coinValue, ev.Player);
                    }
                }
            }
        }

        protected override void OnAcquired(Player player, Item item, bool displayMessage)
        {
            base.OnAcquired(player, item, false);
            if (item.Type == ItemType.Coin)
            {
                if (droppedCoins.TryGetValue(item.Serial, out var coinData))
                {
                    int coinValue = coinData.coinValue;
                    FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"<b><size=24><color=green>+{coinValue}$</color> Picked up Death Coin.</b></size>", coinValue, 3, false, false);
                    droppedCoins.Remove(item.Serial);
                }
                item.Destroy();
            }
        }
    }
}
