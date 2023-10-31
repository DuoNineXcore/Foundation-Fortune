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
using FoundationFortune.API.Models;

namespace FoundationFortune.API.HintSystem
{
    public partial class ServerEvents
    {
        public Dictionary<Npc, Vector3> buyingBotPositions = new();
        public Dictionary<Npc, Vector3> sellingBotPositions = new();
        public Dictionary<Npc, Vector3> musicBotPositions = new();

        private static Npc GetNearestBuyingBot(Player player)
        {
            var botPositions = FoundationFortune.Singleton.BuyingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return GetNearestFoundationFortuneBot(player, botPositions, BotType.Buying);
        }

        private static Npc GetNearestSellingBot(Player player)
        {
            var botPositions = FoundationFortune.Singleton.SellingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return GetNearestFoundationFortuneBot(player, botPositions, BotType.Selling);
        }

        public static bool IsPlayerNearBuyingBot(Player player)
        {
            var botPositions = FoundationFortune.Singleton.BuyingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return IsPlayerNearFoundationFortuneBot(player, botPositions, BotType.Buying);
        }

        private static bool IsPlayerNearSellingBot(Player player)
        {
            var botPositions = FoundationFortune.Singleton.SellingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return IsPlayerNearFoundationFortuneBot(player, botPositions, BotType.Selling);
        }

        private void InitializeFoundationFortuneNPCs()
        {
            Log.Debug($"Initializing Foundation Fortune NPCs.");
            if (!FoundationFortune.Singleton.Config.BuyingBots) Log.Debug($"Buying bots are turned off");
            else
            {
                Log.Debug($"Initializing Buying Bot NPCs.");
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
                                bot.IsGodModeEnabled = true;
                                Log.Debug($"Teleported Buying Bot with indexation {indexation} to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
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

            if (!FoundationFortune.Singleton.Config.SellingBots) Log.Debug($"Selling bots are turned off");
            else
            {
                Log.Debug($"Initializing Selling Bot NPCs.");

                sellingBotPositions.Clear();

                foreach (SellingBotSpawn spawn in FoundationFortune.Singleton.Config.SellingBotSpawnSettings)
                {
                    Log.Debug($"Spawning Bot: {spawn.Name}");
                    SellingBot.SpawnSellingBot(
                        spawn.Name,
                        spawn.Badge,
                        spawn.BadgeColor,
                        spawn.Role,
                        spawn.HeldItem,
                        spawn.Scale
                    );
                }

                if (FoundationFortune.Singleton.Config.SellingBotFixedLocation)
                {
                    Log.Debug($"Bots spawned.");
                    var rooms = FoundationFortune.Singleton.Config.SellingBotSpawnSettings.Select(location => location.Room).ToList();

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
                    var rooms = Room.List.Where(r => FoundationFortune.Singleton.Config.SellingBotRandomRooms.Contains(r.Type)).ToList();
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
                                sellingBotPositions[bot] = bot.Position;
                                Log.Debug($"Teleported Selling Bot to room {roomType}, Pos: {bot.Position}, Rot: {bot.Rotation}");
                            });
                        }
                        else Log.Warn($"No available rooms for Selling Bots.");
                    }
                }
            }
        }

        private void UpdateNpcProximityMessages(Player ply, ref StringBuilder hintMessage)
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

                    if (!itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
                    
                    int price = soldItemData.price;
                    string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
                        .Replace("%price%", price.ToString())
                        .Replace("%time%", GetConfirmationTimeLeft(ply));

                    hintMessage.Append($"{confirmationHint}");
                }
            }
        }

        private static bool IsPlayerNearFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, BotType botType)
        {
            float botRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;
            foreach (var bot in from kvp in botPositions let bot = kvp.Key let botPosition = kvp.Value let distance = Vector3.Distance(player.Position, botPosition) where distance <= botRadius select bot)
            {
                switch (botType)
                {
                    case BotType.Buying when FoundationFortune.Singleton.BuyingBots.Any(x => x.Value.bot == bot):
                    case BotType.Selling when FoundationFortune.Singleton.SellingBots.Any(x => x.Value.bot == bot):
                        return true;
                }
            }
            return false;
        }

        private static Npc GetNearestFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, BotType botType)
        {
            float botRadius = FoundationFortune.Singleton.Config.BuyingBotRadius;
            foreach (var kvp in from kvp in botPositions let botPosition = kvp.Value let distance = Vector3.Distance(player.Position, botPosition) where distance <= botRadius select kvp)
            {
                switch (botType)
                {
                    case BotType.Buying when FoundationFortune.Singleton.BuyingBots.Any(x => x.Value.bot == kvp.Key):
                        return kvp.Key;
                    case BotType.Selling when FoundationFortune.Singleton.SellingBots.Any(x => x.Value.bot == kvp.Key):
                        return kvp.Key;
                }
            }
            return null;
        }

        private void ClearIndexations()
        {
            foreach (var botData in FoundationFortune.Singleton.BuyingBots.Values.ToList()) BuyingBot.RemoveBuyingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.SellingBots.Values.ToList()) SellingBot.RemoveSellingBot(botData.bot.Nickname);
            foreach (var botData in FoundationFortune.Singleton.MusicBotPairs.ToList()) MusicBot.RemoveMusicBot(botData.MusicBot.Nickname);
            buyingBotPositions.Clear();
            workstationPositions.Clear();
            PerkSystem.EtherealInterventionPlayers.Clear();
            PerkSystem.ViolentImpulsesPlayers.Clear();
            FoundationFortune.Singleton.ConsumedPerks.Clear();
            FoundationFortune.PlayerPurchaseLimits.Clear();
        }
    }
}
