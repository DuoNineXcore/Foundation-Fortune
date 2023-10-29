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

namespace FoundationFortune.API.NPCs
{
    public static class BuyingBot
    {
        public static readonly IReadOnlyList<string> allowedBuyingBotNameColors;

        static BuyingBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
            allowedBuyingBotNameColors = allowedColors;
        }

        public static int GetNextBuyingBotIndexation(string target)
        {
            if (FoundationFortune.Singleton.BuyingBots.TryGetValue(target, out var botData))
            {
                int currentIndexationNumber = botData.indexation;
                int newIndexationNumber = currentIndexationNumber + 1;

                while (FoundationFortune.Singleton.BuyingBots.Values.Any(data => data.indexation == newIndexationNumber)) newIndexationNumber++;
                FoundationFortune.Singleton.BuyingBots[target] = (botData.bot, newIndexationNumber);
                return newIndexationNumber;
            }

            int nextAvailableIndex = 0;
            while (FoundationFortune.Singleton.BuyingBots.Values.Any(data => data.indexation == nextAvailableIndex)) nextAvailableIndex++;
            FoundationFortune.Singleton.BuyingBots[target] = (null, nextAvailableIndex);
            return nextAvailableIndex;
        }

        public static Npc SpawnBuyingBot(string target, string Badge, string Color, RoleTypeId Role, ItemType? HeldItem, Vector3 scale)
        {
            string botKey = $"BuyingBot-{target}";
            int indexation = GetNextBuyingBotIndexation(FoundationFortune.Singleton.BuyingBots.Keys.ToString());
            Npc spawnedBuyingBot = NPCHelperMethods.SpawnFix(target, Role, indexation);

            FoundationFortune.Singleton.BuyingBots[botKey] = (spawnedBuyingBot, indexation);

            spawnedBuyingBot.RankName = Badge;
            spawnedBuyingBot.RankColor = Color;
            spawnedBuyingBot.Scale = scale;

            Round.IgnoredPlayers.Add(spawnedBuyingBot.ReferenceHub);

            Timing.CallDelayed(0.5f, () =>
            {
                if (!HeldItem.HasValue) return;
                spawnedBuyingBot.ClearInventory();
                spawnedBuyingBot.CurrentItem = Item.Create((ItemType)HeldItem, spawnedBuyingBot);
            });

            return spawnedBuyingBot;
        }

        public static bool RemoveBuyingBot(string target)
        {
            string botKey = $"BuyingBot-{target}";
            if (!FoundationFortune.Singleton.BuyingBots.TryGetValue(botKey, out var botData)) return false;
            var (bot, _) = botData;
            if (bot != null)
            {
                bot.ClearInventory();
                Timing.CallDelayed(0.3f, () =>
                {
                    bot.Vaporize(bot);
                    CustomNetworkManager.TypedSingleton.OnServerDisconnect(bot.NetworkIdentity.connectionToClient);
                });
            }
            FoundationFortune.Singleton.BuyingBots.Remove(botKey);
            return true;
        }
    }
}
