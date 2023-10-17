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
            FoundationFortune.Singleton.MusicBots[target] = (null, nextAvailableIndex);
            return nextAvailableIndex;
        }

        public static Npc SpawnMusicBot(string target, string Badge, string Color, RoleTypeId Role, ItemType? HeldItem, Vector3 scale)
        {
            string botKey = $"MusicBot-{target}";
            int indexation = GetNextMusicBotIndexation(FoundationFortune.Singleton.MusicBots.Keys.ToString());
            Npc spawnedMusicBot = NPCHelperMethods.SpawnFix(target, Role, indexation);

            FoundationFortune.Singleton.MusicBots[botKey] = (spawnedMusicBot, indexation);

            spawnedMusicBot.RankName = Badge;
            spawnedMusicBot.RankColor = Color;
            spawnedMusicBot.Scale = scale;
            spawnedMusicBot.IsGodModeEnabled = true;
            spawnedMusicBot.MaxHealth = 9999;
            spawnedMusicBot.Health = 9999;

            Round.IgnoredPlayers.Add(spawnedMusicBot.ReferenceHub);

            Timing.CallDelayed(0.5f, () =>
            {
                spawnedMusicBot.ClearInventory();
                if (HeldItem.HasValue)
                {
                    spawnedMusicBot.ClearInventory();
                    spawnedMusicBot.CurrentItem = Item.Create((ItemType)HeldItem, spawnedMusicBot);
                }
            });

            return spawnedMusicBot;
        }

        public static bool RemoveMusicBot(string target)
        {
            string botKey = $"MusicBot-{target}";
            if (FoundationFortune.Singleton.MusicBots.TryGetValue(botKey, out var botData))
            {
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
                FoundationFortune.Singleton.MusicBots.Remove(botKey);
                return true;
            }
            return false;
        }
    }
}
