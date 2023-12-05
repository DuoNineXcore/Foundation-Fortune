using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CentralAuth;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Subroutines;
using UnityEngine;

namespace FoundationFortune.API.Features.NPCs
{
    public static class NpcHelperMethods
    {
        public static bool IsFoundationFortuneNpc(ReferenceHub refHub)
        {
            Player targetPlayer = Player.Get(refHub);
            return FoundationFortune.Instance.BuyingBots.Values.Any(botAndIndexation => botAndIndexation.bot == targetPlayer)
                   || FoundationFortune.Instance.SellingBots.Values.Any(botAndIndexation => botAndIndexation.bot == targetPlayer)
                   || FoundationFortune.Instance.MusicBotPairs.Any(pair => pair.Player == targetPlayer);
        }
        
        public static void UpdateNpcProximityMessages(Player ply, ref StringBuilder hintMessage)
        {
            if (IsPlayerNearBuyingBot(ply)) hintMessage.Append($"{FoundationFortune.Instance.Translation.BuyingBot}");
            else if (IsPlayerNearSellingBot(ply))
            {
                if (!FoundationFortune.Instance.HintSystem.ConfirmSell.ContainsKey(ply.UserId)) hintMessage.Append($"{FoundationFortune.Instance.Translation.SellingBot}");
                else if (FoundationFortune.Instance.HintSystem.ConfirmSell[ply.UserId])
                {
                    hintMessage.Append($"{FoundationFortune.Instance.Translation.SellingBot}");

                    if (!FoundationFortune.Instance.HintSystem.ItemsBeingSold.TryGetValue(ply.UserId, out var soldItemData)) return;
                    
                    int price = soldItemData.price;
                    string confirmationHint = FoundationFortune.Instance.Translation.ItemConfirmation
                        .Replace("%price%", price.ToString())
                        .Replace("%time%", FoundationFortune.Instance.HintSystem.GetConfirmationTimeLeft(ply));

                    hintMessage.Append($"{confirmationHint}");
                }
            }
        }
        
        public static bool IsFoundationFortuneNpc(SubroutineBase targetTrack) => targetTrack.Role.TryGetOwner(out ReferenceHub refHub) && IsFoundationFortuneNpc(refHub);
        
        public static Npc GetNearestBuyingBot(Player player)
        {
            var botPositions = FoundationFortune.Instance.BuyingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return GetNearestFoundationFortuneBot(player, botPositions, NpcType.Buying);
        }

        public static Npc GetNearestSellingBot(Player player)
        {
            var botPositions = FoundationFortune.Instance.SellingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return GetNearestFoundationFortuneBot(player, botPositions, NpcType.Selling);
        }
        
        public static bool IsPlayerNearBuyingBot(Player player)
        {
            var botPositions = FoundationFortune.Instance.BuyingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return IsPlayerNearFoundationFortuneBot(player, botPositions, NpcType.Buying);
        }

        public static bool IsPlayerNearSellingBot(Player player)
        {
            var botPositions = FoundationFortune.Instance.SellingBots
                .Where(kvp => kvp.Value.bot != null)
                .ToDictionary(kvp => kvp.Value.bot, kvp => kvp.Value.bot.Position);

            return IsPlayerNearFoundationFortuneBot(player, botPositions, NpcType.Selling);
        }

        private static bool IsPlayerNearFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, NpcType botType)
        {
            float botRadius = FoundationFortune.FoundationFortuneNpcSettings.BuyingBotRadius;

            foreach (var bot in from kvp in botPositions let bot = kvp.Key let botPosition = kvp.Value let distance = Vector3.Distance(player.Position, botPosition) where distance <= botRadius select bot)
            {
                switch (botType)
                {
                    case NpcType.Buying when FoundationFortune.Instance.BuyingBots.Any(x => x.Value.bot == bot):
                    case NpcType.Selling when FoundationFortune.Instance.SellingBots.Any(x => x.Value.bot == bot):
                        return true;
                }
            }
            return false;
        }
        
        private static Npc GetNearestFoundationFortuneBot(Player player, Dictionary<Npc, Vector3> botPositions, NpcType botType)
        {
            float botRadius = FoundationFortune.FoundationFortuneNpcSettings.BuyingBotRadius;
            foreach (var kvp in from kvp in botPositions let botPosition = kvp.Value let distance = Vector3.Distance(player.Position, botPosition) where distance <= botRadius select kvp)
            {
                switch (botType)
                {
                    case NpcType.Buying when FoundationFortune.Instance.BuyingBots.Any(x => x.Value.bot == kvp.Key):
                        return kvp.Key;
                    case NpcType.Selling when FoundationFortune.Instance.SellingBots.Any(x => x.Value.bot == kvp.Key):
                        return kvp.Key;
                }
            }
            return null;
        }

        public static Npc SpawnFix(string name, RoleTypeId role, int id = 0, Vector3? position = null)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(gameObject) { IsNPC = true };
            
            try
            {
                npc.ReferenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None, RoleSpawnFlags.None);
            }
            catch (Exception) { /* no */ }

            if (RecyclablePlayerId.FreeIds.Contains(id)) RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
            else if (RecyclablePlayerId._autoIncrement >= id) id = ++RecyclablePlayerId._autoIncrement;

            NetworkServer.AddPlayerForConnection(new FakeConnection(id), gameObject);
            try
            {
                npc.ReferenceHub.authManager.InstanceMode = ClientInstanceMode.DedicatedServer;
            }
            catch (Exception) { /* no */}

            npc.ReferenceHub.nicknameSync.Network_myNickSync = name;
            Timing.CallDelayed(0.25f, delegate { npc.Role.Set(role, SpawnReason.None); });
            if (position.HasValue) Timing.CallDelayed(0.5f, delegate { npc.Position = position.Value; });
            return npc;
        }

        private static (ushort horizontal, ushort vertical) ToClientUShorts(Quaternion rotation)
        {
            if (rotation.eulerAngles.z != 0f) rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);

            float outfHorizontal = rotation.eulerAngles.y;
            float outfVertical = -rotation.eulerAngles.x;

            switch (outfVertical)
            {
                case < -90f:
                    outfVertical += 360f;
                    break;
                case > 270f:
                    outfVertical -= 360f;
                    break;
            }
            return (ToHorizontal(outfHorizontal), ToVertical(outfVertical));

            static ushort ToHorizontal(float horizontal)
            {
                const float toHorizontal = 65535f / 360f;
                horizontal = Mathf.Clamp(horizontal, 0f, 360f);
                return (ushort)Mathf.RoundToInt(horizontal * toHorizontal);
            }

            static ushort ToVertical(float vertical)
            {
                const float toVertical = 65535f / 176f;
                vertical = Mathf.Clamp(vertical, -88f, 88f) + 88f;
                return (ushort)Mathf.RoundToInt(vertical * toVertical);
            }
        }

        private static void LookAt(Npc npc, Vector3 position)
        {
            Vector3 direction = position - npc.Position;
            Quaternion quat = Quaternion.LookRotation(direction, Vector3.up);
            var mouseLook = ((IFpcRole)npc.ReferenceHub.roleManager.CurrentRole).FpcModule.MouseLook;
            (ushort horizontal, ushort vertical) = ToClientUShorts(quat);
            mouseLook.ApplySyncValues(horizontal, vertical);
        }

        public static IEnumerator<float> UpdateNpcDirection()
        {
            while (true)
            {
                foreach (Player player in Player.List)
                {
                    if (!player.GetEffect(EffectType.Invisible)) continue;
                    if (!player.IsAlive) continue;
                    
                    Npc buyingBot = GetNearestBuyingBot(player);
                    Npc sellingBot = GetNearestSellingBot(player);
                    
                    if(buyingBot != null) LookAt(buyingBot, player.Position);
                    else if(sellingBot != null) LookAt(sellingBot, player.Position);
                }

                yield return Timing.WaitForSeconds(FoundationFortune.Instance.Config.NpcLookatUpdateRate);
            }
            // ReSharper disable once IteratorNeverReturns
            // sorry five pebbles
        }
    }
}
