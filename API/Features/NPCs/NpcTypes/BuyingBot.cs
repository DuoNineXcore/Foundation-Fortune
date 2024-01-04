using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Core.Common.Components.NPCs;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Features.NPCs.NpcTypes;

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
        while (NPCInitialization.BuyingBots.Values.Any(data => data.indexation == indexation)) indexation++;
    
        Npc spawnedBuyingBot = NPCHelperMethods.SpawnFix(target, role, indexation);
        string botKey = $"BuyingBot-{target}";
        NPCInitialization.BuyingBots[botKey] = (spawnedBuyingBot, indexation);
            
        //i did not steal this from you totally
        NPCIndicatorComponent npcIndicatorComp = spawnedBuyingBot.GameObject.AddComponent<NPCIndicatorComponent>();

        npcIndicatorComp.glowLight = Exiled.API.Features.Toys.Light.Create(spawnedBuyingBot.Position);
        npcIndicatorComp.glowLight.Color = Color.blue;
        npcIndicatorComp.glowLight.ShadowEmission = false;
        npcIndicatorComp.glowLight.Range = 2f;
        npcIndicatorComp.glowLight.Intensity = 3f;
            
        npcIndicatorComp.circ = Exiled.API.Features.Toys.Primitive.Create(spawnedBuyingBot.Position);
        npcIndicatorComp.circ.Type = PrimitiveType.Sphere;
        npcIndicatorComp.circ.Collidable = false;
        npcIndicatorComp.circ.Scale = new(0.05f, 0.05f, 0.05f);
            
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
        if (!NPCInitialization.BuyingBots.TryGetValue(botKey, out var botData)) return false;
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
        NPCInitialization.BuyingBots.Remove(botKey);
        return true;
    }
}