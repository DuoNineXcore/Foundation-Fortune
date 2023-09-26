using Exiled.API.Features;
using MEC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using InventorySystem.Items.Firearms.Attachments;
using FoundationFortune.API.Database;
using Exiled.API.Features.Items;
using FoundationFortune.API.NPCs;
using Exiled.API.Enums;
using Random = UnityEngine.Random;
using Exiled.API.Features.Doors;
using FoundationFortune.Configs;

namespace FoundationFortune.Events
{
    public partial class ServerEvents
    {
        private Dictionary<string, Queue<HintEntry>> recentHints = new();
        private Dictionary<WorkstationController, Vector3> workstationPositions = new();
        private Dictionary<string, bool> confirmSell = new();
        private Dictionary<string, float> dropTimestamp = new();
        private Dictionary<string, (Item item, int price)> itemsBeingSold = new();
        public Dictionary<Npc, Vector3> buyingBotPositions = new();

        public static readonly Vector3 WorldPos = new(124f, 988f, 24f);
        public const float RadiusSqr = 16f * 16f;

        private IEnumerator<float> UpdateMoneyAndHints()
        {
            while (true)
            {
                foreach (Player ply in Player.List.Where(p => !p.IsDead && !p.IsNPC))
                {
                    int moneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);

                    if (IsPlayerInSafeZone(ply) && moneyOnHold != 0)
                    {
                        PlayerDataRepository.ModifyMoney(ply.UserId, moneyOnHold / 10, true, true, false);
                        PlayerDataRepository.ModifyMoney(ply.UserId, moneyOnHold / 10, false, false, true);
                    }
                    int moneySaved = PlayerDataRepository.GetMoneySaved(ply.UserId);
                    HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

                    string hintMessage = "";

                    if (!PlayerDataRepository.GetHintMinmode(ply.UserId))
                    {
                        hintMessage += $"\n<align={hintAlignment}><b><size=24>Money On Hold: <color={ply.ReferenceHub.roleManager.CurrentRole.RoleColor.ToHex()}>${moneyOnHold}</color></size>\n<size=24>Money Saved: <color={ply.ReferenceHub.roleManager.CurrentRole.RoleColor.ToHex()}>${moneySaved}</color></size><align=left></b>\n";
                    }

                    HandleWorkstationMessages(ply, ref hintMessage);
                    HandleBuyingBotMessages(ply, ref hintMessage);

                    string recentHintsText = GetRecentHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentHintsText))
                    {
                        hintMessage += $"<align={hintAlignment}>\n{recentHintsText}</align>";
                    }

                    ply.ShowHint(hintMessage, 2);

                    if (confirmSell.ContainsKey(ply.UserId) && Time.time - dropTimestamp[ply.UserId] >= FoundationFortune.Singleton.Config.SellingConfirmationTime)
                    {
                        confirmSell.Remove(ply.UserId);
                        dropTimestamp.Remove(ply.UserId);
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private void HandleWorkstationMessages(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

            if (IsPlayerOnSellingWorkstation(ply))
            {
                if (!confirmSell.ContainsKey(ply.UserId))
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align>";
                }
                else if (confirmSell[ply.UserId])
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align>";

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        hintMessage += $"<align={hintAlignment}>\n{FoundationFortune.Singleton.Translation.ItemConfirmation}</align>";
                    }
                }
            }
        }

        private void HandleBuyingBotMessages(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

            if (IsPlayerOnBuyingBotRadius(ply, out Npc npc))
            {
                BuyingBot.LookAt(npc, ply.Position);
                if (!confirmSell.ContainsKey(ply.UserId))
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.BuyingBot}</align>";
                }
                else if (confirmSell[ply.UserId])
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.BuyingBot}</align>";

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        hintMessage += $"<align={hintAlignment}>\n{FoundationFortune.Singleton.Translation.ItemConfirmation}</align>";
                    }
                }
            }
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

        public string GetRecentHints(string userId)
        {
            if (recentHints.ContainsKey(userId))
            {
                var currentHints = recentHints[userId];
                while (currentHints.Count > 0 && (Time.time - currentHints.Peek().Timestamp) > FoundationFortune.Singleton.Config.MaxHintAge)
                {
                    currentHints.Dequeue();
                }

                return string.Join("\n", currentHints.Select(entry => entry.Text));
            }

            return "";
        }

        public void EnqueueHint(Player player, string hint, int reward, float duration, bool transferToSavings, bool transferAllToSavings)
        {
            float expirationTime = Time.time + duration;

            if (!recentHints.ContainsKey(player.UserId))
            {
                recentHints[player.UserId] = new Queue<HintEntry>();
            }

            if (transferAllToSavings)
            {
                PlayerDataRepository.TransferMoney(player.UserId, true);
            }
            else if (transferToSavings)
            {
                PlayerDataRepository.ModifyMoney(player.UserId, reward, false, false, true);
            }
            else
            {
                PlayerDataRepository.ModifyMoney(player.UserId, reward, false, true, false);
            }

            recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, reward));

            while (recentHints[player.UserId].Count > FoundationFortune.Singleton.Config.MaxHintsToShow)
            {
                recentHints[player.UserId].Dequeue();
            }
        }

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
                    else
                    {
                        Log.Warn($"Invalid indexation {indexation + 1} for BuyingBot");
                    }
                }
            }
        }

        public bool IsPlayerInSafeZone(Player player)
        {
            float distanceSqr = (player.Position - WorldPos).sqrMagnitude;
            return distanceSqr <= RadiusSqr;
        }

        private List<Vector3> GetRandomSpawnPositions(int count)
        {
            List<Vector3> spawnPositions = new();
            for (int i = 0; i < count; i++)
            {
                Vector3 position = new(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
                spawnPositions.Add(position);
            }
            return spawnPositions;
        }

        public bool IsPlayerOnSellingWorkstation(Player player)
        {
            if (workstationPositions.Count == 0)
            {
                return false;
            }

            foreach (var workstationPosition in workstationPositions.Values)
            {
                float distance = Vector3.Distance(player.Position, workstationPosition);
                if (distance <= FoundationFortune.Singleton.Config.SellingWorkstationRadius)
                {
                    return true;
                }
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

                if (distance <= buyingBotRadius)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public bool IsPlayerNearSellingBot(Player player)
        {
            bool isNearBot = IsPlayerOnBuyingBotRadius(player, out Npc npc);
            if (!isNearBot || npc == null) return false;

            bool isSellingBot = FoundationFortune.Singleton.Config.BuyingBotSpawnSettings.Any(c => c.Name == npc.Nickname && c.IsSellingBot);
            if (isSellingBot) return true;

            return false;
        }


        private class HintEntry
        {
            public string Text { get; }
            public float Timestamp { get; }
            public int Reward { get; }

            public HintEntry(string text, float timestamp, int reward)
            {
                Text = text;
                Timestamp = timestamp;
                Reward = reward;
            }
        }
    }
}
