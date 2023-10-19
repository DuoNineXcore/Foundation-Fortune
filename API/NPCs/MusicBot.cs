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
using VoiceChat;

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

        public static bool RemoveMusicBot(string target)
        {
            string botKey = $"MusicBot-{target}";
            if (FoundationFortune.Singleton.MusicBots.TryGetValue(botKey, out var botData))
            {
                var (bot, _) = botData;
                if (bot != null)
                {
                    Timing.CallDelayed(0.3f, () =>
                    {
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
