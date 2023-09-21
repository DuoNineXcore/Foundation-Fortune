namespace FoundationFortune.API.NPCs
{
    using Exiled.API.Enums;
    using Exiled.API.Extensions;
    using Exiled.API.Features;
    using Exiled.API.Features.Components;
    using Exiled.API.Features.Items;
    using MEC;
    using Mirror;
    using PlayerRoles;
    using PlayerRoles.FirstPersonControl;
    using PlayerRoles.PlayableScps.Subroutines;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class BuyingBotComponent : MonoBehaviour
    {
        internal Npc BuyingBotNPC;
        internal Player ply;
    }

    public static class BuyingBot
    {
        static BuyingBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();

            List<string> allowedColors = new(serverRoles.NamedColors.Length);

            foreach (ServerRoles.NamedColor namedColor in serverRoles.NamedColors)
            {
                if (namedColor.Restricted)
                    continue;

                allowedColors.Add(namedColor.Name);
            }

            allowedBuyingBotNameColors = allowedColors;
        }

        public static readonly IReadOnlyList<string> allowedBuyingBotNameColors;

        public static bool RemoveBuyingBot(int indexationNumber)
        {
            foreach (var kvp in FoundationFortune.Singleton.BuyingBotIndexation)
            {
                if (kvp.Value.indexation == indexationNumber)
                {
                    Npc bot = kvp.Value.bot;
                    if (bot != null)
                    {
                        bot.ClearInventory();
                        Timing.CallDelayed(0.3f, () =>
                        {
                            FoundationFortune.Singleton.serverEvents.buyingBotPositions.Remove(bot);
                            NetworkServer.Destroy(bot.GameObject);
                            bot.Vaporize(bot);
                            CustomNetworkManager.TypedSingleton.OnServerDisconnect(bot.NetworkIdentity.connectionToClient);
                        });
                    }
                    FoundationFortune.Singleton.BuyingBotIndexation.Remove(kvp.Key);
                    return true;
                }
            }
            return false;
        }

        public static int GetNextIndexationNumber(string target)
        {
            if (FoundationFortune.Singleton.BuyingBotIndexation.TryGetValue(target, out var botData))
            {
                int currentIndexationNumber = botData.indexation;
                int newIndexationNumber = currentIndexationNumber + 1;

                while (FoundationFortune.Singleton.BuyingBotIndexation.Values.Any(data => data.indexation == newIndexationNumber))
                {
                    newIndexationNumber++;
                }

                FoundationFortune.Singleton.BuyingBotIndexation[target] = (botData.bot, newIndexationNumber);
                return newIndexationNumber;
            }

            int nextAvailableIndex = 0;
            while (FoundationFortune.Singleton.BuyingBotIndexation.Values.Any(data => data.indexation == nextAvailableIndex))
            {
                nextAvailableIndex++;
            }

            return nextAvailableIndex;
        }

        public static bool IsSellingBot(ReferenceHub refHub)
        {
            return FoundationFortune.Singleton.BuyingBotIndexation.Values.Any(botAndIndexation => botAndIndexation.bot == Player.Get(refHub));
        }

        public static bool IsSellingBot(ScpSubroutineBase targetTrack)
        {
            return targetTrack.Role.TryGetOwner(out ReferenceHub refHub)
                    && FoundationFortune.Singleton.BuyingBotIndexation.Values.Any(botAndIndexation => botAndIndexation.bot == Player.Get(refHub));
        }

        public static Npc SpawnBuyingBot(string target, string Badge, string Color, RoleTypeId Role, ItemType? HeldItem, Vector3 scale)
        {
            int indexationNumber = GetNextIndexationNumber(target);

            Npc SpawnedBuyingBot = SpawnFix($"{target}", Role, indexationNumber);
            string botKey = $"BuyingBot-{target}-{indexationNumber}";
            FoundationFortune.Singleton.BuyingBotIndexation.Add(botKey, (SpawnedBuyingBot, indexationNumber));

            SpawnedBuyingBot.Scale = scale;
            SpawnedBuyingBot.IsGodModeEnabled = true;
            SpawnedBuyingBot.MaxHealth = 9999;
            SpawnedBuyingBot.Health = 9999;

            SpawnedBuyingBot.RankName = Badge;
            SpawnedBuyingBot.RankColor = Color;

            Round.IgnoredPlayers.Add(SpawnedBuyingBot.ReferenceHub);

            BuyingBotComponent BuyingBotComponent = SpawnedBuyingBot.GameObject.AddComponent<BuyingBotComponent>();
            BuyingBotComponent.BuyingBotNPC = SpawnedBuyingBot;

            if (!(HeldItem == null || HeldItem == ItemType.None))
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    SpawnedBuyingBot.ClearInventory();
                    SpawnedBuyingBot.CurrentItem = Item.Create((ItemType)HeldItem, SpawnedBuyingBot);
                });
            }

            return SpawnedBuyingBot;
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
            catch (Exception arg)
            {
            }

            if (RecyclablePlayerId.FreeIds.Contains(id))
            {
                RecyclablePlayerId.FreeIds.RemoveFromQueue(id);
            }
            else if (RecyclablePlayerId._autoIncrement >= id)
            {
                id = ++RecyclablePlayerId._autoIncrement;
            }

            NetworkServer.AddPlayerForConnection(new FakeConnection(id), gameObject);
            try
            {
                npc.ReferenceHub.characterClassManager.SyncedUserId = "FoundationFortune";
                npc.ReferenceHub.characterClassManager.InstanceMode = ClientInstanceMode.Unverified;
            }
            catch (Exception e)
            {
                Log.Debug($"2 Ignore: {e}");
            }

            npc.ReferenceHub.nicknameSync.Network_myNickSync = name;
            Player.Dictionary.Add(gameObject, npc);
            Timing.CallDelayed(0.3f, delegate
            {
                npc.Role.Set(role, SpawnReason.ForceClass, position.HasValue ? RoleSpawnFlags.AssignInventory : RoleSpawnFlags.All);
                npc.ClearInventory();
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

        // source for this method: o5zereth on discord
        public static (ushort horizontal, ushort vertical) ToClientUShorts(Quaternion rotation)
        {
            if (rotation.eulerAngles.z != 0f)
            {
                rotation = Quaternion.LookRotation(rotation * Vector3.forward, Vector3.up);
            }

            float outfHorizontal = rotation.eulerAngles.y;
            float outfVertical = -rotation.eulerAngles.x;

            if (outfVertical < -90f)
            {
                outfVertical += 360f;
            }
            else if (outfVertical > 270f)
            {
                outfVertical -= 360f;
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
