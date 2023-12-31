﻿using System.Collections.Generic;
using System.Globalization;
using Discord;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Enums.Systems.QuestSystem;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Systems;

namespace FoundationFortune.API.Features.Items.CustomItems;

[CustomItem(ItemType.Coin)]
public class DeathCoin : CustomItem
{
	public override uint Id { get; set; } = 330;
	public override string Name { get; set; } = "Death Coin";
	public override string Description { get; set; } = "A dead man's wealth.";
	public override float Weight { get; set; } = 0f;
	public override SpawnProperties SpawnProperties { get; set; }
	private readonly Dictionary<int, (int coinValue, Player player)> _droppedCoins = new();

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
		if (PerkSystem.HasPerk(ev.Player, PerkType.GracefulSaint)) return;

		int moneyBeforeDeath = PlayerStatsRepository.GetMoneyOnHold(ev.Player.UserId);
		if (moneyBeforeDeath <= 0 || PlayerSettingsRepository.GetPluginAdmin(ev.Player.UserId)) return;
		FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, FoundationFortune.Instance.Translation.Death.Replace("%moneyBeforeDeath%", moneyBeforeDeath.ToString()));
		PlayerStatsRepository.EmptyMoney(ev.Player.UserId, true);
		int coinValue = moneyBeforeDeath / FoundationFortune.SellableItemsList.DeathCoinsToDrop;
		for (int i = 0; i < FoundationFortune.SellableItemsList.DeathCoinsToDrop; i++)
		{
			if (!TrySpawn(Id, ev.Player.Position, out Pickup coin)) continue;
			if (coin == null) continue;
			DirectoryIterator.Log($"Spawned coin at Pos:{coin.Position} Rotation:{coin.Rotation}, Serial: {coin.Serial}, Value: {coinValue}", LogLevel.Debug);
			_droppedCoins[coin.Serial] = (coinValue, ev.Player);
		}
	}

	protected override void OnAcquired(Player player, Item item, bool displayMessage)
	{
		base.OnAcquired(player, item, false);
		if (item.Type != ItemType.Coin) return;
		if (_droppedCoins.TryGetValue(item.Serial, out var coinData))
		{
			int coinValue = coinData.coinValue;
			int xpReward = coinValue / 2;
			double prestigeMultiplier = PlayerStatsRepository.GetPrestigeMultiplier(player.UserId);
			FoundationFortune.Instance.HintSystem.BroadcastHint(player, FoundationFortune.Instance.Translation.DeathCoinPickup
				.Replace("%coinValue%", coinValue.ToString())
				.Replace("%xpReward%", xpReward.ToString())
				.Replace("%multiplier%", prestigeMultiplier.ToString(CultureInfo.InvariantCulture)));
			PlayerStatsRepository.ModifyMoney(player.UserId, coinValue, false, true, false, prestigeMultiplier);
			QuestSystem.UpdateQuestProgress(player, QuestType.CollectMoneyFromDeathCoins, coinData.coinValue);
			PlayerStatsRepository.SetExperience(player.UserId, xpReward);
			_droppedCoins.Remove(item.Serial);
		}
		item.Destroy();
	}
}