﻿using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Components;
using Exiled.API.Features.Items;
using MEC;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Subroutines;
using SCPSLAudioApi.AudioCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CentralAuth;
using UnityEngine;
using VoiceChat;
using FoundationFortune.API.Models.Classes;

namespace FoundationFortune.API.NPCs
{
    public static class NPCHelperMethods
    {
        public static bool IsFoundationFortuneNPC(ReferenceHub refHub)
        {
            Player targetPlayer = Player.Get(refHub);
            return FoundationFortune.Singleton.BuyingBots.Values.Any(botAndIndexation => botAndIndexation.bot == targetPlayer)
                   || FoundationFortune.Singleton.SellingBots.Values.Any(botAndIndexation => botAndIndexation.bot == targetPlayer)
                   || FoundationFortune.Singleton.MusicBotPairs.Any(pair => pair.Player == targetPlayer);
        }

        public static bool IsFoundationFortuneNPC(ScpSubroutineBase targetTrack)
        {
            if (targetTrack.Role.TryGetOwner(out ReferenceHub refHub)) return IsFoundationFortuneNPC(refHub);
            return false;
        }

        public static Npc SpawnFix(string name, RoleTypeId role, int id = 0, Vector3? position = null)
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

            npc.ReferenceHub.nicknameSync.Network_myNickSync = name;
            Timing.CallDelayed(0.25f, delegate
            {
                npc.Role.Set(role, SpawnReason.ForceClass, position.HasValue ? RoleSpawnFlags.AssignInventory : RoleSpawnFlags.All);
            });
            if (position.HasValue)
            {
                Timing.CallDelayed(0.5f, delegate
                {
                    npc.Position = position.Value;
                });
            }
            return npc;
        }

        public static (ushort horizontal, ushort vertical) ToClientUShorts(Quaternion rotation)
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
                const float ToHorizontal = 65535f / 360f;
                horizontal = Mathf.Clamp(horizontal, 0f, 360f);
                return (ushort)Mathf.RoundToInt(horizontal * ToHorizontal);
            }

            static ushort ToVertical(float vertical)
            {
                const float ToVertical = 65535f / 176f;
                vertical = Mathf.Clamp(vertical, -88f, 88f) + 88f;
                return (ushort)Mathf.RoundToInt(vertical * ToVertical);
            }
        }

        public static void LookAt(Npc npc, Vector3 position)
        {
            Vector3 direction = position - npc.Position;
            Quaternion quat = Quaternion.LookRotation(direction, Vector3.up);
            var mouseLook = ((IFpcRole)npc.ReferenceHub.roleManager.CurrentRole).FpcModule.MouseLook;
            (ushort horizontal, ushort vertical) = ToClientUShorts(quat);
            mouseLook.ApplySyncValues(horizontal, vertical);
        }
    }
}