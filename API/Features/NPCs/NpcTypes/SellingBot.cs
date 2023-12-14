using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Common.Components.NPCs;
using MEC;
using Mirror;
using PlayerRoles;
using UnityEngine;

namespace FoundationFortune.API.Features.NPCs.NpcTypes;

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
        while (NPCInitialization.SellingBots.Values.Any(data => data.indexation == indexation)) indexation++;

        Npc spawnedSellingBot = NPCHelperMethods.SpawnFix(target, role, indexation);
        string botKey = $"SellingBot-{target}";
        NPCInitialization.SellingBots[botKey] = (spawnedSellingBot, indexation);
            
        NPCIndicatorComponent npcIndicatorComp = spawnedSellingBot.GameObject.AddComponent<NPCIndicatorComponent>();

        npcIndicatorComp.glowLight = Exiled.API.Features.Toys.Light.Create(spawnedSellingBot.Position);
        npcIndicatorComp.glowLight.Color = Color.magenta;
        npcIndicatorComp.glowLight.ShadowEmission = false;
        npcIndicatorComp.glowLight.Range = 2f;
        npcIndicatorComp.glowLight.Intensity = 3f;
            
        npcIndicatorComp.circ = Exiled.API.Features.Toys.Primitive.Create(spawnedSellingBot.Position);
        npcIndicatorComp.circ.Type = PrimitiveType.Sphere;
        npcIndicatorComp.circ.Collidable = false;
        npcIndicatorComp.circ.Scale = new Vector3(0.05f, 0.05f, 0.05f);

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
        if (!NPCInitialization.SellingBots.TryGetValue(botKey, out var botData)) return false;
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
        NPCInitialization.SellingBots.Remove(botKey);
        return true;
    }
}