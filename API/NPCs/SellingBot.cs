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
    public static class SellingBot
    {
        public static readonly IReadOnlyList<string> allowedSellingBotNameColors;

        static SellingBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
            allowedSellingBotNameColors = allowedColors;
        }

        public static Npc SpawnSellingBot(string target, string Badge, string Color, RoleTypeId Role, ItemType? HeldItem, Vector3 scale)
        {
            int indexation = 0;
            while (FoundationFortune.Singleton.SellingBots.Values.Any(data => data.indexation == indexation)) indexation++;

            Npc spawnedSellingBot = NPCHelperMethods.SpawnFix(target, Role, indexation);
            string botKey = $"SellingBot-{target}";
            FoundationFortune.Singleton.SellingBots[botKey] = (spawnedSellingBot, indexation);

            spawnedSellingBot.RankName = Badge;
            spawnedSellingBot.RankColor = Color;
            spawnedSellingBot.Scale = scale;

            Round.IgnoredPlayers.Add(spawnedSellingBot.ReferenceHub);
            Timing.CallDelayed(0.5f, () =>
            {
                if (!HeldItem.HasValue) return;
                spawnedSellingBot.ClearInventory();
                spawnedSellingBot.CurrentItem = Item.Create((ItemType)HeldItem, spawnedSellingBot);
            });
            return spawnedSellingBot;
        }
        
        public static bool RemoveSellingBot(string target)
        {
            string botKey = $"SellingBot-{target}";
            if (!FoundationFortune.Singleton.SellingBots.TryGetValue(botKey, out var botData)) return false;
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
            FoundationFortune.Singleton.SellingBots.Remove(botKey);
            return true;
        }
    }
}
