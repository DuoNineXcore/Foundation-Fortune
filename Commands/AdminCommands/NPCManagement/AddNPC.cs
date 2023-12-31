﻿using System;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Features.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;
using MEC;
using PlayerRoles;
using RemoteAdmin;
using UnityEngine;

namespace FoundationFortune.Commands.AdminCommands.NPCManagement;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class AddNPC : ICommand, IUsageProvider
{
    public string Command { get; } = "ff_addnpc";
    public string Description { get; } = "Add an NPC to the game.";
    public string[] Aliases { get; } = new string[] {};
    public string[] Usage { get; } = new string[] { "<NpcType> <Name> <Badge> <Color> <RoleTypeId> <HeldItemId> <ScaleX> <ScaleY> <ScaleZ>" };

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ff.npc.add"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (args.Count < 10)
        {
            response = "Usage: <NpcType> <Name> <Badge> <Color> <Role> <HeldItem> <ScaleX> <ScaleY> <ScaleZ>";
            return false;
        }

        string npcTypeString = args.At(0);
        if (!Enum.TryParse(npcTypeString, true, out NpcType npcType))
        {
            response = "Invalid NpcType specified.";
            return false;
        }

        string name = args.At(1);
        string badge = args.At(2);
        string color = args.At(3);
        string roleString = args.At(4);
        string heldItemString = args.At(5);

        if (!Enum.TryParse(roleString, out RoleTypeId role) || !Enum.TryParse(heldItemString, out ItemType heldItem))
        {
            response = "Invalid Role or HeldItem specified.";
            return false;
        }

        if (!float.TryParse(args.At(6), out float scaleX) || !float.TryParse(args.At(7), out float scaleY) || !float.TryParse(args.At(8), out float scaleZ))
        {
            response = "Invalid Scale specified. Please provide three float values for X, Y, and Z components.";
            return false;
        }

        Vector3 scale = new(scaleX, scaleY, scaleZ);
        Npc bot;
        
        switch (npcType)
        {
            case NpcType.Buying: bot = BuyingBot.SpawnBuyingBot(name, badge, color, role, heldItem, scale); break;
            case NpcType.Selling: bot = SellingBot.SpawnSellingBot(name, badge, color, role, heldItem, scale); break;
            case NpcType.Music: response = "You cannot create Music Bots"; return false;
            default: response = "Invalid NpcType specified."; return false;
        }

        if (bot != null)
        {
            if (sender is not PlayerCommandSender self)
            {
                response = "Only players can use this command.";
                return false;
            }

            Timing.CallDelayed(1f, delegate
            {
                bot.Teleport(self.ReferenceHub.gameObject.transform.position);
                NPCInitialization.BuyingBotPositions.Add(bot, bot.Position);
            });

            response = $"BuyingBot '{name}' added successfully and teleported to your position.";
            return true;
        }

        response = "Failed to add the BuyingBot.";
        return false;
    }
}