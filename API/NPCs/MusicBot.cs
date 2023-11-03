using Exiled.API.Features;
using UnityEngine;
using MEC;
using PlayerRoles;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features.Components;
using Mirror;
using System;
using System.Collections.Generic;
using CentralAuth;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Classes.NPCs;

namespace FoundationFortune.API.NPCs
{
    public static class MusicBot
    {
        public static readonly IReadOnlyList<string> allowedMusicBotNameColors;

        static MusicBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
            allowedMusicBotNameColors = allowedColors;
        }

        public static Npc SpawnMusicBot(Player target)
        {
            Npc spawnedBuyingBot = SpawnFix(target.Nickname, RoleTypeId.Spectator);

            PlayerMusicBotPair pair = new PlayerMusicBotPair(target, spawnedBuyingBot);
            FoundationFortune.Singleton.MusicBotPairs.Add(pair);
            Log.Debug($"Generated Music Bot for player {target.Nickname} / Bot: {spawnedBuyingBot.Nickname ?? "Null"}");

            Round.IgnoredPlayers.Add(spawnedBuyingBot.ReferenceHub);
            return spawnedBuyingBot;
        }

        public static Npc GetMusicBotByPlayer(Player player)
        {
            var pair = FoundationFortune.Singleton.MusicBotPairs.Find(p => p.Player == player);
            return pair?.MusicBot;
        }

        public static bool RemoveMusicBot(string target)
        {
            var pairToRemove = FoundationFortune.Singleton.MusicBotPairs.Find(pair => pair.Player.Nickname == target);
            if (pairToRemove == null) return false;
            Timing.CallDelayed(0.3f, () =>
            {
                CustomNetworkManager.TypedSingleton.OnServerDisconnect(pairToRemove.MusicBot.NetworkIdentity.connectionToClient);
            });
            FoundationFortune.Singleton.MusicBotPairs.Remove(pairToRemove);
            return true;
        }
        
        private static Npc SpawnFix(string name, RoleTypeId role, int id = 0)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(gameObject) { IsNPC = true };
            
            try
            {
                npc.ReferenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None, RoleSpawnFlags.None);
            }
            catch (Exception)
            {
                // ignored
            }

            if (RecyclablePlayerId.FreeIds.Contains(id)) RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
            else if (RecyclablePlayerId._autoIncrement >= id) id = ++RecyclablePlayerId._autoIncrement;

            NetworkServer.AddPlayerForConnection(new FakeConnection(id), gameObject);
            try
            {
                npc.ReferenceHub.authManager.InstanceMode = ClientInstanceMode.DedicatedServer;
            }
            catch (Exception)
            {
                // ignored
            }

            npc.ReferenceHub.nicknameSync.Network_myNickSync = $"MusicBot-{name}";
            Timing.CallDelayed(0.5f, delegate
            {
                npc.Role.Set(role);
            });
            return npc;
        }
    }
}
