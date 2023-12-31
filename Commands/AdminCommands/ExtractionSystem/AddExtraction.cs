﻿using System;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using FoundationFortune.API.Core.Database;

namespace FoundationFortune.Commands.AdminCommands.ExtractionSystem;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
internal class AddExtraction : ICommand, IUsageProvider
{
    public string Command { get; } = "ff_addextraction";
    public string Description { get; } = "Create an extraction zone event";
    public string[] Aliases { get; } = new string[] { };
    public string[] Usage { get; } = new string[] { "<RoomType> <durationInSeconds>" };

    public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
    {
        Player p = Player.Get(sender);

        if (!sender.CheckPermission("ff.extractionsystem.add") && !PlayerSettingsRepository.GetPluginAdmin(p.UserId))
        {
            response = "You do not have permission to use this command.";
            return false;
        }

        if (args.Count < 2)
        {
            response = "Usage: ff_addextraction <RoomType> <durationInSeconds>";
            return false;
        }

        string roomTypeString = args.At(0);
        string durationString = args.At(1);

        if (!Enum.TryParse(roomTypeString, true, out RoomType roomType))
        {
            response = "Invalid RoomType. Please provide a valid RoomType.";
            return false;
        }

        if (!float.TryParse(durationString, out float duration) || duration <= 0)
        {
            response = "Invalid duration. Please provide a valid positive duration in seconds.";
            return false;
        }

        API.Core.Systems.ExtractionSystem.StartExtractionEvent(roomType, duration);

        response = $"Extraction zone event created for {roomType} lasting {duration} seconds.";
        return true;
    }
}