using System;
using System.Collections.Generic;
using System.Linq;
using CentralAuth;
using Discord;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using FoundationFortune.API.Common.Models.NPCs;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;
using NetworkManager = Mirror.NetworkManager;

namespace FoundationFortune.API.Features.NPCs.NpcTypes;

public static class MusicBot
{
    public static readonly IReadOnlyList<string> AllowedMusicBotNameColors;

    static MusicBot()
    {
        ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
        List<string> allowedColors = new(serverRoles.NamedColors.Length);
        allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
        AllowedMusicBotNameColors = allowedColors;
    }

    /// <summary>
    /// Spawns a music bot associated with the specified player.
    /// </summary>
    /// <param name="target">The player for whom the music bot is spawned.</param>
    public static void SpawnMusicBot(Player target)
    {
        Npc spawnedMusicBot = SpawnFix(target.Nickname, RoleTypeId.Spectator);
    
        PlayerMusicBotPair pair = new PlayerMusicBotPair(target, spawnedMusicBot, false);
        NPCHelperMethods.MusicBotPairs.Add(pair);
        DirectoryIterator.Log($"Generated Music Bot for player {target.Nickname} / Bot: {spawnedMusicBot.Nickname ?? "Null"}", LogLevel.Debug);

        Round.IgnoredPlayers.Add(spawnedMusicBot.ReferenceHub);
    }

    public static Npc GetMusicBotByPlayer(Player player)
    {
        var pair = NPCHelperMethods.MusicBotPairs.Find(p => p.Player == player);
        return pair?.MusicBot;
    }

    public static bool RemoveMusicBot(string target)
    {
        var pairToRemove = NPCHelperMethods.MusicBotPairs.Find(pair => pair.Player.Nickname == target);
        if (pairToRemove == null) return false;
        Timing.CallDelayed(0.3f, () =>
        {
            CustomNetworkManager.TypedSingleton.OnServerDisconnect(pairToRemove.MusicBot.NetworkIdentity.connectionToClient);
        });
        NPCHelperMethods.MusicBotPairs.Remove(pairToRemove);
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

        npc.ReferenceHub.nicknameSync.Network_myNickSync = $"MusicBot-[{name}]";
        Timing.CallDelayed(0.5f, delegate
        {
            npc.Role.Set(role);
        });
        return npc;
    }
}