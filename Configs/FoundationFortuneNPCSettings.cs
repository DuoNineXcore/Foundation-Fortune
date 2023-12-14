using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Enums;
using FoundationFortune.API.Common.Interfaces.Configs;
using FoundationFortune.API.Common.Models.NPCs;
using PlayerRoles;
using UnityEngine;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs;

public class FoundationFortuneNpcSettings : IFoundationFortuneConfig
{
	[YamlIgnore] public string PropertyName { get; set; } = "Server Event Settings";
		
	[Description("Foundation Fortune NPC Settings.")]
	public bool FoundationFortuneNpCs { get; set; } = true;
	public bool BuyingBots { get; set; } = true;
	public bool SellingBots { get; set; } = true;
	public bool MusicBots { get; set; } = true;
	public float BuyingBotRadius { get; set; } = 3f;
	public bool BuyingBotFixedLocation { get; set; } = true;

	public List<BuyingBotSpawn> BuyingBotSpawnSettings { get; set; } = new List<BuyingBotSpawn>
	{
		new BuyingBotSpawn { Name = "Buying Bot 1", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.Tutorial, HeldItem = ItemType.Lantern, Scale = new Vector3(1, 1, 1), Room = RoomType.HczNuke },
		new BuyingBotSpawn { Name = "Buying Bot 2", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.Tutorial, HeldItem = ItemType.Lantern, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz079 },
		new BuyingBotSpawn { Name = "Buying Bot 3", Badge = "Foundation Fortune", BadgeColor = "pumpkin", Role = RoleTypeId.Tutorial, HeldItem = ItemType.Lantern, Scale = new Vector3(1, 1, 1), Room = RoomType.HczArmory }
	};

	public List<RoomType> BuyingBotRandomRooms { get; set; } = new List<RoomType>()
	{
		RoomType.EzCafeteria,
		RoomType.EzCollapsedTunnel,
		RoomType.HczStraight,
		RoomType.HczNuke,
		RoomType.HczTesla,
		RoomType.LczClassDSpawn,
		RoomType.EzCheckpointHallway,
		RoomType.HczServers,
	};

	public float SellingBotRadius { get; set; } = 3f;
	public bool SellingBotFixedLocation { get; set; } = true;

	public List<SellingBotSpawn> SellingBotSpawnSettings { get; set; } = new List<SellingBotSpawn>
	{
		new SellingBotSpawn { Name = "Selling Bot 1", Badge = "Foundation Fortune", BadgeColor = "yellow", Role = RoleTypeId.Tutorial, HeldItem = ItemType.Lantern, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz096 },
		new SellingBotSpawn { Name = "Selling Bot 2", Badge = "Foundation Fortune", BadgeColor = "yellow", Role = RoleTypeId.Tutorial, HeldItem = ItemType.Lantern, Scale = new Vector3(1, 1, 1), Room = RoomType.Hcz939 },
	};

	public List<RoomType> SellingBotRandomRooms { get; set; } = new List<RoomType>()
	{
		RoomType.EzCafeteria,
		RoomType.EzCollapsedTunnel,
		RoomType.HczStraight,
		RoomType.HczNuke,
		RoomType.HczTesla,
		RoomType.LczClassDSpawn,
		RoomType.EzCheckpointHallway,
		RoomType.HczServers,
	};
}