using System;
using CommandSystem;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Common.Enums.NPCs;
using FoundationFortune.API.Features.NPCs.NpcTypes;

namespace FoundationFortune.Commands.AdminCommands.NPCManagement;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class RemoveNPC : ICommand, IUsageProvider
{
    public string Command { get; } = "ff_removenpc";
    public string Description { get; } = "Remove an NPC from the game.";
    public string[] Aliases { get; } = new string[] { };
    public string[] Usage { get; } = new string[] { "<NpcType> <IndexationNumber>" };

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ff.npc.remove"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (args.Count < 2)
        {
            response = "Usage: ff_removenpc <NpcType> <IndexationNumber>";
            return false;
        }

        string npcTypeString = args.At(0);
        if (!Enum.TryParse(npcTypeString, true, out NpcType npcType))
        {
            response = "Invalid NpcType specified.";
            return false;
        }

        if (!int.TryParse(args.At(1), out int indexationNumber))
        {
            response = "Invalid Indexation Number. Please provide a valid number.";
            return false;
        }

        string indexationString = indexationNumber.ToString();

        switch (npcType)
        {
            case NpcType.Buying:
                if (BuyingBot.RemoveBuyingBot(indexationString))
                {
                    response = $"Removed BuyingBot with Indexation Number {indexationNumber}.";
                    return true;
                }

                response = $"No BuyingBot found with Indexation Number {indexationNumber}.";
                return false;
            case NpcType.Selling:
                if (SellingBot.RemoveSellingBot(indexationString))
                {
                    response = $"Removed SellingBot with Indexation Number {indexationNumber}.";
                    return true;
                }

                response = $"No SellingBot found with Indexation Number {indexationNumber}.";
                return false;
            case NpcType.Music:
                if (MusicBot.RemoveMusicBot(indexationString))
                {
                    response = $"Removed MusicBot with Indexation Number {indexationNumber}.";
                    return true;
                }
                    
                response = $"No MusicBot found with Indexation Number {indexationNumber}.";
                return false;
            default:
                response = "Invalid NpcType specified.";
                return false;
        }
    }
}