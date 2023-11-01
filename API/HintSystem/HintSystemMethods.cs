﻿using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using Random = UnityEngine.Random;
using System.Text;
using FoundationFortune.API.Models;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private Dictionary<WorkstationController, Vector3> workstationPositions = new();

		public bool IsPlayerOnSellingWorkstation(Player player) => workstationPositions.Count != 0 && workstationPositions.Values.Select(workstationPosition => 
			Vector3.Distance(player.Position, workstationPosition)).Any(distance => distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius);

		private static void UpdatePerkIndicator(Dictionary<Player, Dictionary<PerkType, int>> consumedPerks, ref StringBuilder perkIndicator)
		{
			foreach (var perkEntry in consumedPerks.SelectMany(playerPerks => playerPerks.Value))
			{
				var (perkType, count) = (perkEntry.Key, perkEntry.Value);
				if (!FoundationFortune.Singleton.Translation.PerkCounterEmojis.TryGetValue(perkType, out var emoji)) continue;
				perkIndicator.Append(count > 1 ? $"{emoji}x{count} " : $"{emoji} "); 
			}
			perkIndicator.AppendLine();
		}
		
		private void InitializeWorkstationPositions()
        {
	        Log.Debug($"Initializing Selling workstations.");
	        if (!FoundationFortune.Singleton.Config.UseSellingWorkstation)
	        {
		        Log.Debug($"no workstations they're turned off nvm");
		        return;
	        }

	        HashSet<WorkstationController> allWorkstations = WorkstationController.AllWorkstations;
	        int numWorkstationsToConsider = allWorkstations.Count / 2;
	        HashSet<WorkstationController> selectedWorkstations = new();

	        foreach (var workstation in allWorkstations.OrderBy(_ => Random.value).Take(numWorkstationsToConsider))
		        selectedWorkstations.Add(workstation);

	        workstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
        }

        private string GetConfirmationTimeLeft(Player ply)
        {
	        if (!dropTimestamp.ContainsKey(ply.UserId)) return "0";
	        float timeLeft = FoundationFortune.Singleton.Config.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
            return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
        }

        private void UpdateWorkstationMessages(Player ply, ref StringBuilder hintMessage)
        {
	        if (!IsPlayerOnSellingWorkstation(ply)) return;
	        
	        if (!confirmSell.ContainsKey(ply.UserId))
		        hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
	        else if (confirmSell[ply.UserId])
	        {
		        hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
		        if (!itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
		        int price = soldItemData.price;
		        hintMessage.Append($"{FoundationFortune.Singleton.Translation.ItemConfirmation
			        .Replace("%price%", price.ToString()).Replace("%time%", GetConfirmationTimeLeft(ply))}");
	        }
        }

        public static void AddToPlayerLimits(Player player, PerkItem perkItem)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtPerks.ContainsKey(perkItem)) playerLimit.BoughtPerks[perkItem]++;
			else playerLimit.BoughtPerks[perkItem] = 1;
		}

		public static void AddToPlayerLimits(Player player, BuyableItem buyItem)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtItems.ContainsKey(buyItem)) playerLimit.BoughtItems[buyItem]++;
            else playerLimit.BoughtItems[buyItem] = 1;
        }

        public static void AddToPlayerLimits(Player player, SellableItem sellItem)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.SoldItems.ContainsKey(sellItem)) playerLimit.SoldItems[sellItem]++;
			else playerLimit.SoldItems[sellItem] = 1;
		}
	}
}
