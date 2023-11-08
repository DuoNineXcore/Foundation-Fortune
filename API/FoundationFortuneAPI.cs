using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using Random = UnityEngine.Random;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.NPCs;
using FoundationFortune.API.NPCs;
using MEC;

// ReSharper disable once CheckNamespace
namespace FoundationFortune.API
{
	public partial class FoundationFortuneAPI
	{
        #region NPCs
        public readonly Dictionary<Npc, Vector3> buyingBotPositions = new();
        public readonly Dictionary<Npc, Vector3> sellingBotPositions = new();

        private void InitializeFoundationFortuneNPCs()
        {
            Log.Debug($"Initializing Foundation Fortune NPCs.");
            if (!FoundationFortune.FoundationFortuneNpcSettings.BuyingBots) Log.Debug("Buying bots are turned off");
            else
            {
                Log.Debug($"Initializing Buying Bot NPCs.");
                foreach (BuyingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.BuyingBotSpawnSettings)
                {
                    Log.Debug($"Spawning Bot: {spawn.Name}");
                    BuyingBot.SpawnBuyingBot(spawn.Name, spawn.Badge, spawn.BadgeColor, spawn.Role, spawn.HeldItem, spawn.Scale);
                }

                if (FoundationFortune.FoundationFortuneNpcSettings.BuyingBotFixedLocation)
                {
                    var rooms = FoundationFortune.FoundationFortuneNpcSettings.BuyingBotSpawnSettings.Select(location => location.Room).ToList();

                    foreach (var kvp in FoundationFortune.Singleton.BuyingBots)
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
                                bot.IsGodModeEnabled = true;
                                Log.Debug($"Teleported Buying Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
                            });
                        }
                        else Log.Warn($"Invalid indexation {indexation + 1} for BuyingBot");
                    }
                }
                else
                {
                    var rooms = Room.List.Where(r => FoundationFortune.FoundationFortuneNpcSettings.BuyingBotRandomRooms.Contains(r.Type)).ToList();
                    var availableIndexes = Enumerable.Range(0, rooms.Count).ToList();

                    availableIndexes.Clear();
                    availableIndexes.AddRange(Enumerable.Range(0, rooms.Count));

                    foreach (var bot in FoundationFortune.Singleton.BuyingBots.Select(kvp => kvp.Value.bot))
                    {
                        if (availableIndexes.Count > 0)
                        {
                            int randomIndex = Random.Range(0, availableIndexes.Count);
                            int indexation = availableIndexes[randomIndex];
                            availableIndexes.RemoveAt(randomIndex);

                            RoomType roomType = rooms[indexation].Type;
                            Door door = rooms[indexation].Doors.First();
                            Vector3 position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);

                            Timing.CallDelayed(1f, () =>
                            {
                                bot.Teleport(position);
                                buyingBotPositions[bot] = bot.Position;
                                bot.IsGodModeEnabled = true;
                                Log.Debug($"Teleported Buying Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
                            });
                        }
                        else Log.Warn($"No available rooms for Buying Bots.");
                    }
                }
            }

            if (!FoundationFortune.FoundationFortuneNpcSettings.SellingBots) Log.Debug($"Selling bots are turned off");
            else
            {
                Log.Debug($"Initializing Selling Bot NPCs.");

                sellingBotPositions.Clear();

                foreach (SellingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.SellingBotSpawnSettings)
                {
                    Log.Debug($"Selling Bot Spawned: {spawn.Name}");
                    SellingBot.SpawnSellingBot(spawn.Name, spawn.Badge, spawn.BadgeColor, spawn.Role, spawn.HeldItem, spawn.Scale);
                }

                if (FoundationFortune.FoundationFortuneNpcSettings.SellingBotFixedLocation)
                {
                    Log.Debug($"Bots spawned.");
                    var rooms = FoundationFortune.FoundationFortuneNpcSettings.SellingBotSpawnSettings.Select(location => location.Room).ToList();

                    foreach (var kvp in FoundationFortune.Singleton.SellingBots)
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
                                bot.IsGodModeEnabled = true;
                                sellingBotPositions[bot] = bot.Position;
                                Log.Debug($"Teleported Selling Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
                            });
                        }
                        else Log.Warn($"Invalid indexation {indexation + 1} for Selling Bot");
                    }
                }
                else
                {
                    Log.Debug($"Bots spawned randomly.");
                    var rooms = Room.List.Where(r => FoundationFortune.FoundationFortuneNpcSettings.SellingBotRandomRooms.Contains(r.Type)).ToList();
                    var availableIndexes = Enumerable.Range(0, rooms.Count).ToList();

                    availableIndexes.Clear();
                    availableIndexes.AddRange(Enumerable.Range(0, rooms.Count));

                    foreach (var bot in FoundationFortune.Singleton.SellingBots.Select(kvp => kvp.Value.bot))
                    {
                        if (availableIndexes.Count > 0)
                        {
                            int randomIndex = Random.Range(0, availableIndexes.Count);
                            int indexation = availableIndexes[randomIndex];
                            availableIndexes.RemoveAt(randomIndex);

                            RoomType roomType = rooms[indexation].Type;
                            Door door = rooms[indexation].Doors.First();
                            Vector3 position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);

                            Timing.CallDelayed(1f, () =>
                            {
                                bot.Teleport(position);
                                bot.IsGodModeEnabled = true;
                                sellingBotPositions[bot] = bot.Position;
                                Log.Debug($"Teleported Selling Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
                            });
                        }
                        else Log.Warn($"No available rooms for Selling Bots.");
                    }
                }
            }
        }

        #endregion

        #region Hint System
        private void UpdateNpcProximityMessages(Player ply, ref StringBuilder hintMessage)
        {
            if (NPCHelperMethods.IsPlayerNearBuyingBot(ply)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.BuyingBot}");
            else if (NPCHelperMethods.IsPlayerNearSellingBot(ply))
            {
                if (!confirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");
                else if (confirmSell[ply.UserId])
                {
                    hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");

                    if (!itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
                    
                    int price = soldItemData.price;
                    string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
                        .Replace("%price%", price.ToString())
                        .Replace("%time%", GetConfirmationTimeLeft(ply));

                    hintMessage.Append($"{confirmationHint}");
                }
            }
        }
        
        private string GetConfirmationTimeLeft(Player ply)
        {
            if (!dropTimestamp.ContainsKey(ply.UserId)) return "0";
            float timeLeft = FoundationFortune.SellableItemsList.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
            return timeLeft > 0 ? timeLeft.ToString("F0") : "0";
        }
        #endregion
        
        private void ClearIndexations()
        {
            foreach (var botData in FoundationFortune.Singleton.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            PerkSystem.PerkPlayers.Clear();
            FoundationFortune.Singleton.ConsumedPerks.Clear();
            FoundationFortune.PlayerPurchaseLimits.Clear();
            buyingBotPositions.Clear();
            workstationPositions.Clear();
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

        private static void AddToPlayerLimits(Player player, SellableItem sellItem)
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
        
        #region Workstations
        private Dictionary<WorkstationController, Vector3> workstationPositions = new();
        
        public bool IsPlayerOnSellingWorkstation(Player player) => workstationPositions.Count != 0 && workstationPositions.Values.Select(workstationPosition => 
            Vector3.Distance(player.Position, workstationPosition)).Any(distance => distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius);
        
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

            foreach (var workstation in allWorkstations.OrderBy(_ => Random.value).Take(numWorkstationsToConsider)) selectedWorkstations.Add(workstation);
            workstationPositions = selectedWorkstations.ToDictionary(workstation => workstation, workstation => workstation.transform.position);
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
        
        #endregion
	}
}
