using System;
using System.Collections.Generic;
using System.Text;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Features.NPCs;

namespace FoundationFortune.Commands.AdminCommands.NPCManagement;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler((typeof(GameConsoleCommandHandler)))]
internal class ListNPCs : ICommand
{
    public string Command { get; } = "ff_listnpcs";
    public string Description { get; } = "List all NPCs in the game.";
    public string[] Aliases { get; } = new string[] { };

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        if (!sender.CheckPermission("ff.npc.list"))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (FoundationFortune.Instance == null)
        {
            response = "FoundationFortune.Instance is null.";
            return false;
        }

        StringBuilder sb = new();
        sb.AppendLine("Foundation Fortune NPCs:");
        AppendBotList(sb, "Selling Bots", NPCInitialization.SellingBots.Values);
        AppendBotList(sb, "Buying Bots", NPCInitialization.BuyingBots.Values);
        response = sb.ToString();
        return true;
    }

    private static void AppendBotList(StringBuilder sb, string title, IEnumerable<(Npc bot, int indexation)> collection)
    {
        sb.AppendLine(title + ":");
        foreach (var (bot, indexation) in collection) sb.AppendLine(bot != null ? $"Nickname: {bot.Nickname} Index: {indexation} -> Position: {bot.Position} - Room: {bot.CurrentRoom.Type}" : "Invalid bot format.");
    }
}