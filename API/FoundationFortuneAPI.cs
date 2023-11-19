using System;
using Exiled.API.Features;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using Random = UnityEngine.Random;
using System.Text;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.NPCs;
using FoundationFortune.API.NPCs;
using FoundationFortune.API.NPCs.NpcTypes;
using FoundationFortune.API.Systems;
using MEC;

// ReSharper disable once CheckNamespace
namespace FoundationFortune.API
{
    public partial class FoundationFortuneAPI
	{
        #region NPCs
        public static readonly Dictionary<Npc, Vector3> buyingBotPositions = new();
        public static readonly Dictionary<Npc, Vector3> sellingBotPositions = new();

        public static void InitializeFoundationFortuneNPCs()
        {
            try
            { 
                FoundationFortune.Log($"Initializing Foundation Fortune NPCs.", LogLevel.Debug);
            if (!FoundationFortune.FoundationFortuneNpcSettings.BuyingBots) FoundationFortune.Log("Buying bots are turned off", LogLevel.Debug);
            else
            {
                FoundationFortune.Log($"Initializing Buying Bot NPCs.", LogLevel.Debug);
                foreach (BuyingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.BuyingBotSpawnSettings)
                {
                    FoundationFortune.Log($"Spawning Bot: {spawn.Name}", LogLevel.Debug);
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
                                FoundationFortune.Log($"Teleported Buying Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else FoundationFortune.Log($"Invalid indexation {indexation + 1} for BuyingBot", LogLevel.Error);
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
                                FoundationFortune.Log($"Teleported Buying Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else FoundationFortune.Log($"No available rooms for Buying Bots.", LogLevel.Error);
                    }
                }
            }

            if (!FoundationFortune.FoundationFortuneNpcSettings.SellingBots) FoundationFortune.Log($"Selling bots are turned off", LogLevel.Debug);
            else
            {
                FoundationFortune.Log($"Initializing Selling Bot NPCs.", LogLevel.Debug);

                sellingBotPositions.Clear();

                foreach (SellingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.SellingBotSpawnSettings)
                {
                    FoundationFortune.Log($"Selling Bot Spawned: {spawn.Name}", LogLevel.Debug);
                    SellingBot.SpawnSellingBot(spawn.Name, spawn.Badge, spawn.BadgeColor, spawn.Role, spawn.HeldItem, spawn.Scale);
                }

                if (FoundationFortune.FoundationFortuneNpcSettings.SellingBotFixedLocation)
                {
                    FoundationFortune.Log($"Bots spawned.", LogLevel.Info);
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
                                FoundationFortune.Log($"Teleported Selling Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else FoundationFortune.Log($"Invalid indexation {indexation + 1} for Selling Bot", LogLevel.Error);
                    }
                }
                else
                {
                    FoundationFortune.Log($"Bots spawned randomly.", LogLevel.Info);
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
                                FoundationFortune.Log($"Teleported Selling Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else FoundationFortune.Log($"No available rooms for Selling Bots.", LogLevel.Error);
                    }
                }
            }
            }
            catch (Exception ex)
            {
                Log.Debug(ex);
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
        
        public static void ClearIndexations()
        {
            foreach (var botData in FoundationFortune.Singleton.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            PerkSystem.PerkPlayers.Clear();
            FoundationFortune.Singleton.ConsumedPerks.Clear();
            FoundationFortune.PlayerPurchaseLimits.Clear();
            sellingBotPositions.Clear();
            buyingBotPositions.Clear();
            workstationPositions.Clear();
        }

        public static void AddToPlayerLimits(Player player, BuyablePerk buyablePerk)
		{
			var playerLimit = FoundationFortune.PlayerPurchaseLimits.FirstOrDefault(p => p.Player.UserId == player.UserId);
			if (playerLimit == null)
			{
				playerLimit = new ObjectInteractions(player);
				FoundationFortune.PlayerPurchaseLimits.Add(playerLimit);
			}

			if (playerLimit.BoughtPerks.ContainsKey(buyablePerk)) playerLimit.BoughtPerks[buyablePerk]++;
			else playerLimit.BoughtPerks[buyablePerk] = 1;
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
        
        #region Workstations
        private static Dictionary<WorkstationController, Vector3> workstationPositions = new();
        
        public bool IsPlayerOnSellingWorkstation(Player player) => workstationPositions.Count != 0 && workstationPositions.Values.Select(workstationPosition => Vector3.Distance(player.Position, workstationPosition)).Any(distance => distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius);
        
        public static void InitializeWorkstationPositions()
        {
            FoundationFortune.Log($"Initializing Selling workstations.", LogLevel.Debug);
            if (!FoundationFortune.Singleton.Config.UseSellingWorkstation)
            {
                FoundationFortune.Log($"no workstations they're turned off nvm", LogLevel.Debug);
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
	        
            if (!confirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingWorkstation}");
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
