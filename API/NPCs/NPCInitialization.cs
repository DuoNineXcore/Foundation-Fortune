using FoundationFortune.API.Database;
using MEC;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using FoundationFortune.API.NPCs;
using System.Linq;
using Exiled.Events.EventArgs.Server;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using PlayerRoles;
using System.Collections.Generic;
using FoundationFortune.API.Perks;
using Utf8Json.Resolvers.Internal;
using Exiled.API.Extensions;
using InventorySystem;
using Exiled.API.Enums;
using PluginAPI.Roles;
using Exiled.API.Features.Roles;
using Exiled.API.Features.Doors;
using System.Text;

namespace FoundationFortune.API.HintSystem
{
    public partial class ServerEvents
    {
        public Dictionary<Npc, Vector3> buyingBotPositions = new();
        public Dictionary<Npc, Vector3> sellingBotPositions = new();
        public Dictionary<Npc, Vector3> musicBotPositions = new();

        public Npc GetNearestBuyingBot(Player player) => GetNearestFoundationFortuneBot(player, FoundationFortune.Singleton.BuyingBots.Select(x => x.Value.bot).ToDictionary(k => k, v => v.Position), BotType.Buying);
        public Npc GetNearestSellingBot(Player player) => GetNearestFoundationFortuneBot(player, FoundationFortune.Singleton.SellingBots.Select(x => x.Value.bot).ToDictionary(k => k, v => v.Position), BotType.Selling);
        public bool IsPlayerNearBuyingBot(Player player) => IsPlayerNearFoundationFortuneBot(player, FoundationFortune.Singleton.BuyingBots.Select(x => x.Value.bot).ToDictionary(k => k, v => v.Position), BotType.Buying);
        public bool IsPlayerNearSellingBot(Player player) => IsPlayerNearFoundationFortuneBot(player, FoundationFortune.Singleton.SellingBots.Select(x => x.Value.bot).ToDictionary(k => k, v => v.Position), BotType.Selling);

        public void InitializeFoundationFortuneNPCs()
        {
            Log.Debug($"Intitializing Foundation Fortune NPCs.");
            if (!FoundationFortune.Singleton.Config.BuyingBots) Log.Debug($"Buying bots are turned off");
            else
            {
                Log.Debug($"Intitializing Buying Bot NPCs.");
                buyingBotPositions.Clear();

                foreach (BuyingBotSpawn spawn in FoundationFortune.Singleton.Config.BuyingBotSpawnSettings)
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

                    foreach (var kvp in FoundationFortune.Singleton.BuyingBots)
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

            if (!FoundationFortune.Singleton.Config.SellingBots) Log.Debug($"Selling bots are turned off");
            else
            {
                Log.Debug($"Intitializing Selling Bot NPCs.");

                buyingBotPositions.Clear();

                foreach (BuyingBotSpawn spawn in FoundationFortune.Singleton.Config.BuyingBotSpawnSettings)
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

                    foreach (var kvp in FoundationFortune.Singleton.BuyingBots)
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
        }

        private void UpdateNPCProximityMessages(Player ply, ref StringBuilder hintMessage)
        {
            Npc buyingBot = GetNearestBuyingBot(ply);
            Npc sellingBot = GetNearestSellingBot(ply);

            if (IsPlayerNearBuyingBot(ply))
            {
                NPCHelperMethods.LookAt(buyingBot, ply.Position);
                hintMessage.Append($"{FoundationFortune.Singleton.Translation.BuyingBot}");
            }
            else if (IsPlayerNearSellingBot(ply))
            {
                NPCHelperMethods.LookAt(sellingBot, ply.Position);
                if (!confirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");
                else if (confirmSell[ply.UserId])
                {
                    hintMessage.Append($"{FoundationFortune.Singleton.Translation.SellingBot}");

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
                            .Replace("%price%", price.ToString())
                            .Replace("%time%", GetConfirmationTimeLeft(ply));

                        hintMessage.Append($"{confirmationHint}");
                    }
                }
            }
        }

        public static bool IsPlayerNearFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, BotType botType)
        {
            float botRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;
            foreach (var kvp in botPositions)
            {
                var bot = kvp.Key;
                var botPosition = kvp.Value;
                float distance = Vector3.Distance(player.Position, botPosition);
                if (distance <= botRadius)
                {
                    if (botType == BotType.Buying && FoundationFortune.Singleton.BuyingBots.Any(x => x.Value.bot == bot)) return true;
                    else if (botType == BotType.Selling && FoundationFortune.Singleton.SellingBots.Any(x => x.Value.bot == bot)) return true;
                }
            }
            return false;
        }

        public static Npc GetNearestFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, BotType botType)
        {
            float botRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;
            foreach (var kvp in botPositions)
            {
                var botPosition = kvp.Value;
                float distance = Vector3.Distance(player.Position, botPosition);
                if (distance <= botRadius)
                {
                    if (botType == BotType.Buying && FoundationFortune.Singleton.BuyingBots.Any(x => x.Value.bot == kvp.Key)) return kvp.Key;
                    else if (botType == BotType.Selling && FoundationFortune.Singleton.SellingBots.Any(x => x.Value.bot == kvp.Key)) return kvp.Key;
                }
            }
            return null;
        }

        public void ClearNPCIndexations()
        {
            FoundationFortune.Singleton.BuyingBots.Clear();
            FoundationFortune.Singleton.SellingBots.Clear();
            FoundationFortune.Singleton.MusicBots.Clear();
        }
    }
}
