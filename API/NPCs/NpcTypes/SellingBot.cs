using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.NPCs.NpcTypes
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

        /// <summary>
        /// Spawns a selling bot with the specified parameters.
        /// </summary>
        /// <param name="target">The target user ID or name for the selling bot.</param>
        /// <param name="Badge">The rank name for the selling bot.</param>
        /// <param name="Color">The rank color for the selling bot.</param>
        /// <param name="Role">The role type ID for the selling bot.</param>
        /// <param name="HeldItem">The held item type for the selling bot (null for no item).</param>
        /// <param name="scale">The scale vector for the selling bot.</param>
        /// <returns>The spawned selling bot.</returns>
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
