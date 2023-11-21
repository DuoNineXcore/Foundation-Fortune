using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using FoundationFortune.API.Core.Models.Classes.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FoundationFortune.API.Features.NPCs;

public static class NPCInitialization
{
    public static readonly Dictionary<Npc, Vector3> buyingBotPositions = new();
    public static readonly Dictionary<Npc, Vector3> sellingBotPositions = new();

    public static void Start()
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
}