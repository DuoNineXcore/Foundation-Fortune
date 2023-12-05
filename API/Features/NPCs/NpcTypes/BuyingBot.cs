using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Features.NPCs.NpcTypes
{
    public static class BuyingBot
    {
        public static readonly IReadOnlyList<string> AllowedBuyingBotNameColors;

        static BuyingBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
            AllowedBuyingBotNameColors = allowedColors;
        }

        /// <summary>
        /// Spawns a buying bot with the specified parameters.
        /// </summary>
        /// <param name="target">The target user ID or name for the buying bot.</param>
        /// <param name="badge">The rank name for the buying bot.</param>
        /// <param name="color">The rank color for the buying bot.</param>
        /// <param name="role">The role type ID for the buying bot.</param>
        /// <param name="heldItem">The held item type for the buying bot (null for no item).</param>
        /// <param name="scale">The scale vector for the buying bot.</param>
        /// <returns>The spawned buying bot.</returns>
        public static Npc SpawnBuyingBot(string target, string badge, string color, RoleTypeId role, ItemType? heldItem, Vector3 scale)
        {
            int indexation = 0;
            while (FoundationFortune.Instance.BuyingBots.Values.Any(data => data.indexation == indexation)) indexation++;
    
            Npc spawnedBuyingBot = NpcHelperMethods.SpawnFix(target, role, indexation);
            string botKey = $"BuyingBot-{target}";
            FoundationFortune.Instance.BuyingBots[botKey] = (spawnedBuyingBot, indexation);

            spawnedBuyingBot.RankName = badge;
            spawnedBuyingBot.RankColor = color;
            spawnedBuyingBot.Scale = scale;

            Round.IgnoredPlayers.Add(spawnedBuyingBot.ReferenceHub);
            Timing.CallDelayed(0.5f, () =>
            {
                if (!heldItem.HasValue) return;
                spawnedBuyingBot.ClearInventory();
                spawnedBuyingBot.CurrentItem = Item.Create((ItemType)heldItem, spawnedBuyingBot);
            });
            return spawnedBuyingBot;
        }

        public static bool RemoveBuyingBot(string target)
        {
            string botKey = $"BuyingBot-{target}";
            if (!FoundationFortune.Instance.BuyingBots.TryGetValue(botKey, out var botData)) return false;
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
            FoundationFortune.Instance.BuyingBots.Remove(botKey);
            return true;
        }
    }
}
