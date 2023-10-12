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

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		private Dictionary<WorkstationController, Vector3> workstationPositions = new();
		public Dictionary<Npc, Vector3> buyingBotPositions = new();

		public static readonly Vector3 WorldPos = new(124f, 988f, 24f);
		public const float RadiusSqr = 16f * 16f;

		public void InitializeWorkstationPositions()
		{
			Log.Debug($"Initializing Selling workstations.");
			if (!FoundationFortune.Singleton.Config.UseBuyingBot)
			{
				Log.Debug($"no workstations they're turned off nvm");
				return;
			}

			HashSet<WorkstationController> allWorkstations = WorkstationController.AllWorkstations;
			int numWorkstationsToConsider = allWorkstations.Count / 2;
			HashSet<WorkstationController> selectedWorkstations = new();

			foreach (var workstation in allWorkstations.OrderBy(x => Random.value).Take(numWorkstationsToConsider))
			{
				selectedWorkstations.Add(workstation);
			}

			workstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
		}

		public void InitializeBuyingBots()
		{
			Log.Debug($"Initializing Buying Bots.");
			if (!FoundationFortune.Singleton.Config.UseBuyingBot)
			{
				Log.Debug($"no buying bots they're turned off nvm");
				return;
			}

			buyingBotPositions.Clear();

			foreach (NPCSpawn spawn in FoundationFortune.Singleton.Config.BuyingBotSpawnSettings)
			{
				Log.Debug($"Spawning Bot: {spawn.Name}");
				BuyingBot.SpawnBuyingBot(
					spawn.Name,
					spawn.Badge,
					spawn.BadgeColor,
					spawn.Role,
					spawn.HeldItem,
					spawn.Scale
				);
			}

			if (FoundationFortune.Singleton.Config.BuyingBotFixedLocation)
			{
				Log.Debug($"Bots spawned.");
				var rooms = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Select(location => location.Room).ToList();

				foreach (var kvp in FoundationFortune.Singleton.BuyingBotIndexation)
				{
					var indexation = kvp.Value.indexation;
					var bot = kvp.Value.bot;

					if (indexation >= 0 && indexation < rooms.Count)
					{
						RoomType roomType = rooms[indexation];
						Door door = Room.Get(roomType).Doors.First();
						Vector3 Position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);
						Timing.CallDelayed(1f, () =>
						{
							bot.Teleport(Position);
							buyingBotPositions[bot] = bot.Position;
							Log.Debug($"Teleported BuyingBot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
						});
					}
					else Log.Warn($"Invalid indexation {indexation + 1} for BuyingBot");
				}
			}
			else
			{
				Log.Debug($"Bots spawned randomly.");
				var rooms = Room.List.Where(r => FoundationFortune.Singleton.Config.BuyingBotRandomRooms.Contains(r.Type)).ToList();
				var availableIndexes = Enumerable.Range(0, rooms.Count).ToList();

				availableIndexes.Clear();
				availableIndexes.AddRange(Enumerable.Range(0, rooms.Count));

				foreach (var kvp in FoundationFortune.Singleton.BuyingBotIndexation)
				{
					var bot = kvp.Value.bot;

					if (availableIndexes.Count > 0)
					{
						int randomIndex = Random.Range(0, availableIndexes.Count);
						int indexation = availableIndexes[randomIndex];
						availableIndexes.RemoveAt(randomIndex);

						RoomType roomType = rooms[indexation].Type;
						Door door = rooms[indexation].Doors.First();
						Vector3 Position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);

						Timing.CallDelayed(1f, () =>
						{
							bot.Teleport(Position);
							buyingBotPositions[bot] = bot.Position;
							Log.Debug($"Teleported BuyingBot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
						});
					}
					else Log.Warn($"No available rooms for BuyingBot.");
				}
			}
		}

		private void HandleWorkstationMessages(Player ply, ref string hintMessage)
		{
			HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);
			int hintAlpha = PlayerDataRepository.GetHintAlpha(ply.UserId);
			int hintSize = PlayerDataRepository.GetHintSize(ply.UserId);

			if (IsPlayerOnSellingWorkstation(ply))
			{
				if (!confirmSell.ContainsKey(ply.UserId)) hintMessage += hintMessage += $"\n<alpha={IntToHexAlpha(hintAlpha)}><size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align></size>\n"; 
				else if (confirmSell[ply.UserId])
				{
					//hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align>";
					hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align></size>\n"; 

					if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
					{
						int price = soldItemData.price;
						hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", GetConfirmationTimeLeft(ply).ToString())}</align></size>\n"; 
						//hintMessage += $"<align={hintAlignment}>\n{FoundationFortune.Singleton.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", GetConfirmationTimeLeft(ply).ToString())}</align>";
					}
				}
			}
		}

		private void HandleBuyingBotMessages(Player ply, ref string hintMessage)
		{
			HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);
			int hintAlpha = PlayerDataRepository.GetHintAlpha(ply.UserId);
			int hintSize = PlayerDataRepository.GetHintSize(ply.UserId);

			Npc npc = GetBuyingBotNearPlayer(ply);

			if (IsPlayerNearBuyingBot(ply, npc))
			{
				BuyingBot.LookAt(npc, ply.Position);
				hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.BuyingBot}</align></size>\n"; 
			}
			else if (IsPlayerNearSellingBot(ply))
			{
				BuyingBot.LookAt(npc, ply.Position);
				if (!confirmSell.ContainsKey(ply.UserId)) hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingBot}</align></size>\n"; 

				else if (confirmSell[ply.UserId])
				{
					hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingBot}</align></size>\n"; 

					if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
					{
						int price = soldItemData.price;
						string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
						    .Replace("%price%", price.ToString())
						    .Replace("%time%", GetConfirmationTimeLeft(ply));

						//hintMessage += $"<align={hintAlignment}>\n{confirmationHint}</align>";
						hintMessage += $"\n{IntToHexAlpha(hintAlpha)}<size={hintSize}><align={hintAlignment}>{confirmationHint}</align></size>\n"; 
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

		public bool IsPlayerOnBuyingBotRadius(Player player)
		{
			float buyingBotRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;

			foreach (var kvp in buyingBotPositions)
			{
				var botPosition = kvp.Value;

				float distance = Vector3.Distance(player.Position, botPosition);

				if (distance <= buyingBotRadius)
				{
					BuyingBot.LookAt(kvp.Key, player.Position);
					return true;
				}
			}
			return false;
		}

		public bool IsPlayerOnBuyingBotRadius(Player player, out Npc? npc)
		{
			float buyingBotRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;

			foreach (var kvp in buyingBotPositions)
			{
				var botPosition = kvp.Value;

				float distance = Vector3.Distance(player.Position, botPosition);

				if (distance <= buyingBotRadius)
				{
					BuyingBot.LookAt(kvp.Key, player.Position);
					npc = kvp.Key;
					return true;
				}
			}
			npc = null;
			return false;
		}

		public Npc GetBuyingBotNearPlayer(Player player)
		{
			float buyingBotRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;

			foreach (var kvp in buyingBotPositions)
			{
				var botPosition = kvp.Value;

				float distance = Vector3.Distance(player.Position, botPosition);

				if (distance <= buyingBotRadius) return kvp.Key;
			}
			return null;
		}

		public bool IsPlayerNearBuyingBot(Player player)
		{
			bool isNearBot = IsPlayerOnBuyingBotRadius(player, out Npc npc);
			if (!isNearBot || npc == null) return false;
			bool isBuyingBot = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Any(c => c.Name == npc.Nickname && !c.IsSellingBot);
			return isBuyingBot;
		}

		public bool IsPlayerNearSellingBot(Player player)
		{
			bool isNearBot = IsPlayerOnBuyingBotRadius(player, out Npc npc);
			if (!isNearBot || npc == null) return false;
			bool isSellingBot = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Any(c => c.Name == npc.Nickname && c.IsSellingBot);
			return isSellingBot;
		}

		public bool IsPlayerNearBuyingBot(Player player, Npc? npc)
		{
			bool isNearBot = IsPlayerOnBuyingBotRadius(player, out npc);
			if (!isNearBot || npc == null) return false;
			bool isBuyingBot = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Any(c => c.Name == npc.Nickname && !c.IsSellingBot);
			return isBuyingBot;
		}

		public bool IsPlayerNearSellingBot(Player player, Npc? npc)
		{
			bool isNearBot = IsPlayerOnBuyingBotRadius(player, out npc);
			if (!isNearBot || npc == null) return false;
			bool isSellingBot = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Any(c => c.Name == npc.Nickname && c.IsSellingBot);
			return isSellingBot;
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
			var playerLimit = FoundationFortune.PlayerLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtPerks.ContainsKey(perkItem))
			{
				playerLimit.BoughtPerks[perkItem]++;
			}
			else
			{
				playerLimit.BoughtPerks[perkItem] = 1;
			}
		}

		public static void AddToPlayerLimits(Player player, BuyableItem buyItem)
		{
			var playerLimit = FoundationFortune.PlayerLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtItems.ContainsKey(buyItem))
			{
				playerLimit.BoughtItems[buyItem]++;
			}
			else
			{
				playerLimit.BoughtItems[buyItem] = 1;
			}
		}

		public static void AddToPlayerLimits(Player player, SellableItem sellItem)
		{
			var playerLimit = FoundationFortune.PlayerLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerLimits.Add(playerLimit);
			}

			if (playerLimit.SoldItems.ContainsKey(sellItem))
			{
				playerLimit.SoldItems[sellItem]++;
			}
			else
			{
				playerLimit.SoldItems[sellItem] = 1;
			}
		}
	}
}
