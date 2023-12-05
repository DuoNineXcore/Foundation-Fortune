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
    public static class SellingBot
    {
        public static readonly IReadOnlyList<string> AllowedSellingBotNameColors;

        static SellingBot()
        {
            ServerRoles serverRoles = NetworkManager.singleton.playerPrefab.GetComponent<ServerRoles>();
            List<string> allowedColors = new(serverRoles.NamedColors.Length);
            allowedColors.AddRange(from namedColor in serverRoles.NamedColors where !namedColor.Restricted select namedColor.Name);
            AllowedSellingBotNameColors = allowedColors;
        }

        /// <summary>
        /// Spawns a selling bot with the specified parameters.
        /// </summary>
        /// <param name="target">The target user ID or name for the selling bot.</param>
        /// <param name="badge">The rank name for the selling bot.</param>
        /// <param name="color">The rank color for the selling bot.</param>
        /// <param name="role">The role type ID for the selling bot.</param>
        /// <param name="heldItem">The held item type for the selling bot (null for no item).</param>
        /// <param name="scale">The scale vector for the selling bot.</param>
        /// <returns>The spawned selling bot.</returns>
        public static Npc SpawnSellingBot(string target, string badge, string color, RoleTypeId role, ItemType? heldItem, Vector3 scale)
        {
            int indexation = 0;
            while (FoundationFortune.Instance.SellingBots.Values.Any(data => data.indexation == indexation)) indexation++;

            Npc spawnedSellingBot = NpcHelperMethods.SpawnFix(target, role, indexation);
            string botKey = $"SellingBot-{target}";
            FoundationFortune.Instance.SellingBots[botKey] = (spawnedSellingBot, indexation);

            spawnedSellingBot.RankName = badge;
            spawnedSellingBot.RankColor = color;
            spawnedSellingBot.Scale = scale;

            Round.IgnoredPlayers.Add(spawnedSellingBot.ReferenceHub);
            Timing.CallDelayed(0.5f, () =>
            {
                if (!heldItem.HasValue) return;
                spawnedSellingBot.ClearInventory();
                spawnedSellingBot.CurrentItem = Item.Create((ItemType)heldItem, spawnedSellingBot);
            });
            return spawnedSellingBot;
        }

        public static bool RemoveSellingBot(string target)
        {
            string botKey = $"SellingBot-{target}";
            if (!FoundationFortune.Instance.SellingBots.TryGetValue(botKey, out var botData)) return false;
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
            FoundationFortune.Instance.SellingBots.Remove(botKey);
            return true;
        }
    }
}
