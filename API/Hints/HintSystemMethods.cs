using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using FoundationFortune.API.NPCs;
using Exiled.API.Enums;
using Random = UnityEngine.Random;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using System.Text;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private Dictionary<WorkstationController, Vector3> workstationPositions = new();

        public static readonly Vector3 WorldPos = new(124f, 988f, 24f);
		public const float RadiusSqr = 16f * 16f;

        public static void UpdatePerkIndicator(Dictionary<Player, Dictionary<PerkType, int>> consumedPerks, ref StringBuilder perkIndicator)
        {
            foreach (var consumedPerkEntry in consumedPerks)
            {
                var consumedPerkTypes = consumedPerkEntry.Value;

                foreach (var consumedPerk in consumedPerkTypes)
                {
                    if (FoundationFortune.Singleton.Config.PerkEmojis.TryGetValue(consumedPerk.Key, out var emoji))
                    {
                        perkIndicator.Append(emoji);
                        if (consumedPerk.Value > 1)
                        {
                            perkIndicator.Append($"x{consumedPerk.Value}");
                        }
                        perkIndicator.Append("");
                    }
                }
            }
        }

        public void InitializeWorkstationPositions()
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

			foreach (var workstation in allWorkstations.OrderBy(x => Random.value).Take(numWorkstationsToConsider))
			selectedWorkstations.Add(workstation);

			workstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
		}

        public string GetConfirmationTimeLeft(Player ply)
        {
            if (dropTimestamp.ContainsKey(ply.UserId))
            {
                float timeLeft = FoundationFortune.Singleton.Config.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
                if (timeLeft > 0)
                {
                    return timeLeft.ToString("F0");
                }
            }
            return "0";
        }

        private void UpdateWorkstationMessages(Player ply, ref StringBuilder hintMessage)
        {
            if (IsPlayerOnSellingWorkstation(ply))
            {
                if (!confirmSell.ContainsKey(ply.UserId))
                hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
                else if (confirmSell[ply.UserId])
                {
                    hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        hintMessage.Append($"{FoundationFortune.Singleton.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", GetConfirmationTimeLeft(ply).ToString())}");
                    }
                }
            }
        }

        public bool IsPlayerInSafeZone(Player player)
		{
			float distanceSqr = (player.Position - WorldPos).sqrMagnitude;
			return distanceSqr <= RadiusSqr;
		}

		public bool IsPlayerOnSellingWorkstation(Player player)
		{
			if (workstationPositions.Count == 0) return false;

			foreach (var workstationPosition in workstationPositions.Values)
			{
				float distance = Vector3.Distance(player.Position, workstationPosition);
				if (distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius) return true;
			}
			return false;
		}

        public static string IntToHexAlpha(int value)
        {
            int clampedValue = Mathf.Clamp(value, 0, 100);
            int alphaValue = Mathf.RoundToInt(clampedValue * 255 / 100);
            string hexValue = alphaValue.ToString("X2");
            string alphaTag = $"<alpha=#{hexValue}>";
            return alphaTag;
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
