using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Core.Common.Models.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FoundationFortune.API.Features.NPCs;

public static class NPCInitialization
{
    public static readonly Dictionary<Npc, Vector3> BuyingBotPositions = new();
    public static readonly Dictionary<Npc, Vector3> SellingBotPositions = new();
    public static readonly Dictionary<string, (Npc bot, int indexation)> BuyingBots = new();
    public static readonly Dictionary<string, (Npc bot, int indexation)> SellingBots = new();
        
    public static void Start()
    {
        try
        {
            DirectoryIterator.Log("Initializing Foundation Fortune NPCs.", LogLevel.Debug);
            if (!FoundationFortune.FoundationFortuneNpcSettings.BuyingBots) DirectoryIterator.Log("Buying bots are turned off", LogLevel.Debug);
            else
            {
                DirectoryIterator.Log("Initializing Buying Bot NPCs.", LogLevel.Debug);
                foreach (BuyingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.BuyingBotSpawnSettings)
                {
                    DirectoryIterator.Log($"Spawning Bot: {spawn.Name}", LogLevel.Debug);
                    BuyingBot.SpawnBuyingBot(spawn.Name, spawn.Badge, spawn.BadgeColor, spawn.Role, spawn.HeldItem, spawn.Scale);
                }

                if (FoundationFortune.FoundationFortuneNpcSettings.BuyingBotFixedLocation)
                {
                    var rooms = FoundationFortune.FoundationFortuneNpcSettings.BuyingBotSpawnSettings.Select(location => location.Room).ToList();

                    foreach (var kvp in BuyingBots)
                    {
                        var indexation = kvp.Value.indexation;
                        var bot = kvp.Value.bot;

                        if (indexation >= 0 && indexation < rooms.Count)
                        {
                            RoomType roomType = rooms[indexation];
                            Door door = Room.Get(roomType).Doors.First();
                            Vector3 position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);
                            Timing.CallDelayed(1f, () =>
                            {
                                bot.Teleport(position);
                                BuyingBotPositions[bot] = bot.Position;
                                bot.IsGodModeEnabled = true;
                                DirectoryIterator.Log($"Teleported Buying Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else DirectoryIterator.Log($"Invalid indexation {indexation + 1} for BuyingBot", LogLevel.Error);
                    }
                }
                else
                {
                    var rooms = Room.List.Where(r => FoundationFortune.FoundationFortuneNpcSettings.BuyingBotRandomRooms.Contains(r.Type)).ToList();
                    var availableIndexes = Enumerable.Range(0, rooms.Count).ToList();

                    availableIndexes.Clear();
                    availableIndexes.AddRange(Enumerable.Range(0, rooms.Count));

                    foreach (var bot in BuyingBots.Select(kvp => kvp.Value.bot))
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
                                BuyingBotPositions[bot] = bot.Position;
                                bot.IsGodModeEnabled = true;
                                DirectoryIterator.Log($"Teleported Buying Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else DirectoryIterator.Log("No available rooms for Buying Bots.", LogLevel.Error);
                    }
                }
            }

            if (!FoundationFortune.FoundationFortuneNpcSettings.SellingBots) DirectoryIterator.Log("Selling bots are turned off", LogLevel.Debug);
            else
            {
                DirectoryIterator.Log("Initializing Selling Bot NPCs.", LogLevel.Debug);
                SellingBotPositions.Clear();
                foreach (SellingBotSpawn spawn in FoundationFortune.FoundationFortuneNpcSettings.SellingBotSpawnSettings)
                {
                    DirectoryIterator.Log($"Selling Bot Spawned: {spawn.Name}", LogLevel.Debug);
                    SellingBot.SpawnSellingBot(spawn.Name, spawn.Badge, spawn.BadgeColor, spawn.Role, spawn.HeldItem, spawn.Scale);
                }

                if (FoundationFortune.FoundationFortuneNpcSettings.SellingBotFixedLocation)
                {
                    DirectoryIterator.Log("Bots spawned.", LogLevel.Info);
                    var rooms = FoundationFortune.FoundationFortuneNpcSettings.SellingBotSpawnSettings.Select(location => location.Room).ToList();

                    foreach (var kvp in SellingBots)
                    {
                        var indexation = kvp.Value.indexation;
                        var bot = kvp.Value.bot;

                        if (indexation >= 0 && indexation < rooms.Count)
                        {
                            RoomType roomType = rooms[indexation];
                            Door door = Room.Get(roomType).Doors.First();
                            Vector3 position = door.Position + door.Transform.rotation * new Vector3(1, 1, 1);
                            Timing.CallDelayed(1f, () =>
                            {
                                bot.Teleport(position);
                                bot.IsGodModeEnabled = true;
                                SellingBotPositions[bot] = bot.Position;
                                DirectoryIterator.Log($"Teleported Selling Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else DirectoryIterator.Log($"Invalid indexation {indexation + 1} for Selling Bot", LogLevel.Error);
                    }
                }
                else
                {
                    DirectoryIterator.Log("Bots spawned at random locations.", LogLevel.Info);
                    var rooms = Room.List.Where(r => FoundationFortune.FoundationFortuneNpcSettings.SellingBotRandomRooms.Contains(r.Type)).ToList();
                    var availableIndexes = Enumerable.Range(0, rooms.Count).ToList();
                    availableIndexes.Clear();
                    availableIndexes.AddRange(Enumerable.Range(0, rooms.Count));

                    foreach (var bot in SellingBots.Select(kvp => kvp.Value.bot))
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
                                SellingBotPositions[bot] = bot.Position;
                                DirectoryIterator.Log($"Teleported Selling Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}", LogLevel.Debug);
                            });
                        }
                        else DirectoryIterator.Log("No available rooms for Selling Bots.", LogLevel.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex);
        }
    }
}