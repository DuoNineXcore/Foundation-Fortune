using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using FoundationFortune.Configs;
using HarmonyLib;
using LiteDB;
using SCPSLAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using FoundationFortune.API;
using FoundationFortune.API.Events;
using FoundationFortune.API.Events.EventArgs;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.NPCs;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.NPCs;
using FoundationFortune.Configs.EXILED;
using MEC;
using PluginAPI.Enums;
using YamlDotNet.Serialization;

namespace FoundationFortune;

// ReSharper disable once ClassNeverInstantiated.Global
// IM NOT MAKING THIS SHIT ABSTRACT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
{
	public override string Author => "DuoNineXcore & Misfiy";
	public override string Name => "Foundation Fortune";
	public override string Prefix => "this plugin is a performance issue";
	public override Version Version => new(1, 0, 0);

	public static readonly string CommonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets"); //WHY CAN I NOT USE PREFIX RAGJSAGHKJFGHASIDKJGBNFDKSGNSKGFNSKJGN
	public static readonly string DatabaseDirectoryPath = Path.Combine(CommonDirectoryPath, "Database");
	public static readonly string AudioFilesPath = Path.Combine(CommonDirectoryPath, "Sound Files");
	public static readonly string PlayerAudioFilesPath = Path.Combine(AudioFilesPath, "PlayerVoiceChatUsageType");
	public static readonly string NpcAudioFilesPath = Path.Combine(AudioFilesPath, "NPCVoiceChatUsageType");

	public static BuyableItemsList BuyableItemsList;
	public static PerkSystemSettings PerkSystemSettings;
	public static MoneyExtractionSystemSettings MoneyExtractionSystemSettings;
	public static SellableItemsList SellableItemsList;
	public static VoiceChatSettings VoiceChatSettings;
	public static MoneyXPRewards MoneyXpRewards;
	public static FoundationFortuneNPCSettings FoundationFortuneNpcSettings; 
	
	private Harmony harmony;

	public static FoundationFortune Singleton;
	public static List<ObjectInteractions> PlayerPurchaseLimits = new();
	public FoundationFortuneAPI FoundationFortuneAPI = new();
	public EventHandlers EventHandlers = new();
	public LiteDatabase db;
	
	public Dictionary<Player, Dictionary<PerkType, int>> ConsumedPerks = new();
	public Dictionary<Player, Dictionary<PerkType, CoroutineHandle>> PerkCoroutines = new();
	
	public Dictionary<string, (Npc bot, int indexation)> BuyingBots = new();
	public Dictionary<string, (Npc bot, int indexation)> SellingBots = new();
	public List<PlayerMusicBotPair> MusicBotPairs { get; private set; } = new();

	public override void OnEnabled()
	{
		Singleton = this;
		RegisterEvents();
		DirectoryIterator.SetupDirectories();
		SetupConfigs();
		Startup.SetupDependencies();
		CustomItem.RegisterItems();
		harmony = new("FoundationFortune");
		harmony.PatchAll();
	}

	public override void OnDisabled()
	{
		db?.Checkpoint();
		db?.Dispose();
		db = null;
		Singleton = null;
		UnregisterEvents();
		CoroutineManager.StopAllCoroutines();
		CustomItem.UnregisterItems();
		harmony?.UnpatchAll(harmony.Id);
		harmony = null!;
		FoundationFortuneAPI = null;
		EventHandlers = null;
	}

	private static void SetupConfigs()
	{
		DirectoryIterator.LoadConfig<PerkSystemSettings>();
		DirectoryIterator.LoadConfig<MoneyExtractionSystemSettings>();
		DirectoryIterator.LoadConfig<BuyableItemsList>();
		DirectoryIterator.LoadConfig<SellableItemsList>();
		DirectoryIterator.LoadConfig<VoiceChatSettings>();
		DirectoryIterator.LoadConfig<MoneyXPRewards>();
		DirectoryIterator.LoadConfig<FoundationFortuneNPCSettings>();
		BuyableItemsList = DirectoryIterator.GetConfig<BuyableItemsList>();
		PerkSystemSettings = DirectoryIterator.GetConfig<PerkSystemSettings>();
		MoneyExtractionSystemSettings = DirectoryIterator.GetConfig<MoneyExtractionSystemSettings>();
		SellableItemsList = DirectoryIterator.GetConfig<SellableItemsList>();
		VoiceChatSettings = DirectoryIterator.GetConfig<VoiceChatSettings>();
		MoneyXpRewards = DirectoryIterator.GetConfig<MoneyXPRewards>();
		FoundationFortuneNpcSettings = DirectoryIterator.GetConfig<FoundationFortuneNPCSettings>();
	}

	private void RegisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified += EventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying += EventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Spawned += EventHandlers.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died += EventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping += EventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem += EventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left += EventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting += EventHandlers.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting += EventHandlers.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted += EventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound += EventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded += EventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam += EventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense += EventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += EventHandlers.FuckYourOtherAbility;
		
		API.Events.Handlers.FoundationFortuneNPCs.SoldItem += EventHandlers.SoldItem;
		API.Events.Handlers.FoundationFortuneNPCs.BoughtItem += EventHandlers.BoughtItem;
		API.Events.Handlers.FoundationFortuneNPCs.BoughtPerk += EventHandlers.BoughtPerk;
		API.Events.Handlers.FoundationFortuneNPCs.UsedFoundationFortuneNPC += EventHandlers.UsedFoundationFortuneNPC;
		API.Events.Handlers.FoundationFortunePerks.UsedFoundationFortunePerk += EventHandlers.UsedFoundationFortunePerk;
	}

	private void UnregisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified -= EventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying -= EventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Spawned -= EventHandlers.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died -= EventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping -= EventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem -= EventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left -= EventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting -= EventHandlers.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting -= EventHandlers.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted -= EventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound -= EventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded -= EventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam -= EventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense -= EventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= EventHandlers.FuckYourOtherAbility;

		API.Events.Handlers.FoundationFortuneNPCs.SoldItem -= EventHandlers.SoldItem;
		API.Events.Handlers.FoundationFortuneNPCs.BoughtItem -= EventHandlers.BoughtItem;
		API.Events.Handlers.FoundationFortuneNPCs.BoughtPerk -= EventHandlers.BoughtPerk;
		API.Events.Handlers.FoundationFortuneNPCs.UsedFoundationFortuneNPC -= EventHandlers.UsedFoundationFortuneNPC;
		API.Events.Handlers.FoundationFortunePerks.UsedFoundationFortunePerk -= EventHandlers.UsedFoundationFortunePerk;
	}
	
	public static void Log(string message, LogLevel severity)
	{
		var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
		if (declaringType == null) return;
		string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
        
		string logEntry = $"[Foundation Fortune 1.0 - {severity.ToString().ToUpper()}] [{methodName}] {message}";
		switch (severity)
		{
			case LogLevel.Debug: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkCyan); break;
			case LogLevel.Error: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkBlue); break;
			case LogLevel.Warn: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Blue); break;
			case LogLevel.Info: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Cyan); break;
			default: throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
		}
	}
}

