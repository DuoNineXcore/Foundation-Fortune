using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Database;
using Exiled.API.Features.Items;
using System.Collections.Generic;
using FoundationFortune.API.Perks;

namespace FoundationFortune.API.Items
{
	[CustomItem(ItemType.Coin)]
	public class DeathCoin : CustomItem
	{
		public override uint Id { get; set; } = 330;
		public override string Name { get; set; } = "Death Coin";
		public override string Description { get; set; } = "A dead man's wealth.";
		public override float Weight { get; set; } = 0f;
		public override SpawnProperties SpawnProperties { get; set; }
		private Dictionary<int, (int coinValue, Player player)> droppedCoins = new();

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
			if (PerkSystem.EtherealInterventionPlayers.Contains(ev.Player)) return;

			int moneyBeforeDeath = PlayerDataRepository.GetMoneyOnHold(ev.Player.UserId);
			if (moneyBeforeDeath > 0 && !PlayerDataRepository.GetPluginAdmin(ev.Player.UserId))
			{
				FoundationFortune.Singleton.ServerEvents.EnqueueHint(ev.Player, FoundationFortune.Singleton.Translation.Death.Replace("%moneyBeforeDeath%", moneyBeforeDeath.ToString()), 5f);
				PlayerDataRepository.EmptyMoney(ev.Player.UserId, true, false);
				int coinValue = moneyBeforeDeath / FoundationFortune.Singleton.Config.DeathCoinsToDrop;
				for (int i = 0; i < FoundationFortune.Singleton.Config.DeathCoinsToDrop; i++)
				{
					if (TrySpawn(Id, ev.Player.Position, out Pickup coin))
					{
						Log.Debug($"Spawned coin at Pos:{coin.Position} Rotation:{coin.Rotation}, Serial: {coin.Serial}, Value: {coinValue}");
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
					FoundationFortune.Singleton.ServerEvents.EnqueueHint(player, FoundationFortune.Singleton.Translation.DeathCoinPickup.Replace("%coinValue%", coinValue.ToString()), 3f);
					PlayerDataRepository.ModifyMoney(player.UserId, coinValue, false, true, false);
					droppedCoins.Remove(item.Serial);
				}
				item.Destroy();
			}
		}
	}
}
