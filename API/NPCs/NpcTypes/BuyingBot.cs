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

        /// <summary>
        /// Spawns a buying bot with the specified parameters.
        /// </summary>
        /// <param name="target">The target user ID or name for the buying bot.</param>
        /// <param name="Badge">The rank name for the buying bot.</param>
        /// <param name="Color">The rank color for the buying bot.</param>
        /// <param name="Role">The role type ID for the buying bot.</param>
        /// <param name="HeldItem">The held item type for the buying bot (null for no item).</param>
        /// <param name="scale">The scale vector for the buying bot.</param>
        /// <returns>The spawned buying bot.</returns>
        public static Npc SpawnBuyingBot(string target, string Badge, string Color, RoleTypeId Role, ItemType? HeldItem, Vector3 scale)
        {
            int indexation = 0;
            while (FoundationFortune.Singleton.BuyingBots.Values.Any(data => data.indexation == indexation)) indexation++;
    
            Npc spawnedBuyingBot = NPCHelperMethods.SpawnFix(target, Role, indexation);
            string botKey = $"BuyingBot-{target}";
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
