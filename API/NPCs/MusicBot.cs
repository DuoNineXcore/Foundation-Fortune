using Exiled.API.Features;
using UnityEngine;
using MEC;
using PlayerRoles;
using System.Linq;
using Exiled.API.Features.Items;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features.Components;
using Mirror;
using System;
using System.Collections.Generic;
using VoiceChat;

namespace FoundationFortune.API.NPCs
{
    public static class MusicBot
    {
        public static readonly IReadOnlyList<string> allowedMusicBotNameColors;

        static MusicBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            foreach (ServerRoles.NamedColor namedColor in serverRoles.NamedColors)
            {
                if (namedColor.Restricted) continue;
                allowedColors.Add(namedColor.Name);
            }
            allowedMusicBotNameColors = allowedColors;
        }

        public static Npc SpawnMusicBot(Player target)
        {
            int indexation = GetNextMusicBotIndexation(target.UserId);
            Npc spawnedMusicBot = SpawnFix(target.Nickname, RoleTypeId.Spectator, indexation);
            FoundationFortune.Singleton.MusicBots[target.UserId] = (spawnedMusicBot, indexation);

            Log.Debug($"Generated Music Bot for player {target.Nickname} / Bot: {spawnedMusicBot.Nickname ?? "Null"}, Index {indexation}");
            spawnedMusicBot.IsGodModeEnabled = true;

            Round.IgnoredPlayers.Add(spawnedMusicBot.ReferenceHub);
            return spawnedMusicBot;
        }

        public static Npc GetMusicBotByUserId(string userId)
        {
            if (FoundationFortune.Singleton.MusicBots.TryGetValue(userId, out var botData))
            {
                if (botData.bot != null)
                {
                    return botData.bot;
                }
                else
                {
                    Log.Debug("Bot object in botData is null.");
                }
            }
            else
            {
                Log.Debug("BotData not found in MusicBots dictionary for user ID: " + userId);
            }
            return null;
        }


        public static Npc SpawnFix(string name, RoleTypeId role, int id = 0)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
            Npc npc = new(gameObject)
            {
                IsNPC = true
            };
            try
            {
                npc.ReferenceHub.roleManager.InitializeNewRole(RoleTypeId.None, RoleChangeReason.None, RoleSpawnFlags.None);
            }
            catch (Exception ex) { }

            if (RecyclablePlayerId.FreeIds.Contains(id)) RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
            else if (RecyclablePlayerId._autoIncrement >= id) id = ++RecyclablePlayerId._autoIncrement;

            NetworkServer.AddPlayerForConnection(new FakeConnection(id), gameObject);
            try
            {
                npc.ReferenceHub.characterClassManager.InstanceMode = ClientInstanceMode.DedicatedServer;
            }
            catch (Exception ex) { }

            npc.ReferenceHub.nicknameSync.Network_myNickSync = $"MusicBot-{name}";
            Timing.CallDelayed(0.25f, delegate
            {
                npc.Role.Set(role, SpawnReason.ForceClass);
            });
            return npc;
        }

        public static int GetNextMusicBotIndexation(string target)
        {
            if (FoundationFortune.Singleton.MusicBots.TryGetValue(target, out var botData))
            {
                int currentIndexationNumber = botData.indexation;
                int newIndexationNumber = currentIndexationNumber + 1;

                while (FoundationFortune.Singleton.MusicBots.Values.Any(data => data.indexation == newIndexationNumber)) newIndexationNumber++;
                FoundationFortune.Singleton.MusicBots[target] = (botData.bot, newIndexationNumber);
                return newIndexationNumber;
            }

            int nextAvailableIndex = 0;
            while (FoundationFortune.Singleton.MusicBots.Values.Any(data => data.indexation == nextAvailableIndex)) nextAvailableIndex++;
            return nextAvailableIndex;
        }

        public static bool RemoveMusicBot(string target)
        {
            if (FoundationFortune.Singleton.MusicBots.TryGetValue(target, out var botData))
            {
                var (bot, _) = botData;
                if (bot != null)
                {
                    Timing.CallDelayed(0.3f, () =>
                    {
                        CustomNetworkManager.TypedSingleton.OnServerDisconnect(bot.NetworkIdentity.connectionToClient);
                    });
                }
                FoundationFortune.Singleton.MusicBots.Remove(target);
                return true;
            }
            return false;
        }
    }
}
