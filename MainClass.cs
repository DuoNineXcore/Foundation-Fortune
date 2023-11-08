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
using FoundationFortune.API.Events.EventArgs;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.NPCs;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.NPCs;
using FoundationFortune.Configs.EXILED;
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

	public static readonly string commonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets");
	public static readonly string databaseDirectoryPath = Path.Combine(commonDirectoryPath, "Database");
	public static readonly string audioFilesPath = Path.Combine(commonDirectoryPath, "Sound Files");
	public static readonly string playerAudioFilesPath = Path.Combine(audioFilesPath, "PlayerVoiceChatUsageType");
	public static readonly string npcAudioFilesPath = Path.Combine(audioFilesPath, "NPCVoiceChatUsageType");

	public static BuyableItemsList BuyableItemsList;
	public static PerkSystemSettings PerkSystemSettings;
	public static MoneyExtractionSystemSettings MoneyExtractionSystemSettings;
	public static SellableItemsList SellableItemsList;
	public static VoiceChatSettings VoiceChatSettings;
	public static ServerEventSettings ServerEventSettings;
	public static FoundationFortuneNPCSettings FoundationFortuneNpcSettings; 
	
	private Harmony harmony;

	public static FoundationFortune Singleton;
	public static List<ObjectInteractions> PlayerPurchaseLimits = new();
	public FoundationFortuneAPI FoundationFortuneAPI = new();
	public LiteDatabase db;
	
	public Dictionary<Player, Dictionary<PerkType, int>> ConsumedPerks = new();
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
	}

	private static void SetupConfigs()
	{
		DirectoryIterator.LoadConfig<PerkSystemSettings>();
		DirectoryIterator.LoadConfig<MoneyExtractionSystemSettings>();
		DirectoryIterator.LoadConfig<BuyableItemsList>();
		DirectoryIterator.LoadConfig<SellableItemsList>();
		DirectoryIterator.LoadConfig<VoiceChatSettings>();
		DirectoryIterator.LoadConfig<ServerEventSettings>();
		DirectoryIterator.LoadConfig<FoundationFortuneNPCSettings>();
		BuyableItemsList = DirectoryIterator.GetConfig<BuyableItemsList>();
		PerkSystemSettings = DirectoryIterator.GetConfig<PerkSystemSettings>();
		MoneyExtractionSystemSettings = DirectoryIterator.GetConfig<MoneyExtractionSystemSettings>();
		SellableItemsList = DirectoryIterator.GetConfig<SellableItemsList>();
		VoiceChatSettings = DirectoryIterator.GetConfig<VoiceChatSettings>();
		ServerEventSettings = DirectoryIterator.GetConfig<ServerEventSettings>();
		FoundationFortuneNpcSettings = DirectoryIterator.GetConfig<FoundationFortuneNPCSettings>();
	}

	private void RegisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified += FoundationFortuneAPI.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying += FoundationFortuneAPI.EtherealInterventionHandler;
		Exiled.Events.Handlers.Player.Spawned += FoundationFortuneAPI.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died += FoundationFortuneAPI.KillingReward;
		Exiled.Events.Handlers.Player.Escaping += FoundationFortuneAPI.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem += FoundationFortuneAPI.SellingItem;
		Exiled.Events.Handlers.Player.Left += FoundationFortuneAPI.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting += FoundationFortuneAPI.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting += FoundationFortuneAPI.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted += FoundationFortuneAPI.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound += FoundationFortuneAPI.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded += FoundationFortuneAPI.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam += FoundationFortuneAPI.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense += FoundationFortuneAPI.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += FoundationFortuneAPI.FuckYourOtherAbility;
		API.Events.Handlers.FoundationFortuneNPC.UsedFoundationFortuneNPC += FoundationFortuneAPI.UsedFoundationFortuneNPC;
	}

	private void UnregisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified -= FoundationFortuneAPI.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying -= FoundationFortuneAPI.EtherealInterventionHandler;
		Exiled.Events.Handlers.Player.Spawned -= FoundationFortuneAPI.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died -= FoundationFortuneAPI.KillingReward;
		Exiled.Events.Handlers.Player.Escaping -= FoundationFortuneAPI.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem -= FoundationFortuneAPI.SellingItem;
		Exiled.Events.Handlers.Player.Left -= FoundationFortuneAPI.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting -= FoundationFortuneAPI.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting -= FoundationFortuneAPI.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted -= FoundationFortuneAPI.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound -= FoundationFortuneAPI.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded -= FoundationFortuneAPI.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam -= FoundationFortuneAPI.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense -= FoundationFortuneAPI.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= FoundationFortuneAPI.FuckYourOtherAbility;
		API.Events.Handlers.FoundationFortuneNPC.UsedFoundationFortuneNPC -= FoundationFortuneAPI.UsedFoundationFortuneNPC;
	}
	
	public static void Log(string message, LogLevel severity)
	{
		var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
		if (declaringType == null) return;
		string className = declaringType.Name;
		string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
        
		string logEntry = $"[{severity.ToString().ToUpper()}] [{className} - {methodName}] {message}";
		switch (severity)
		{
			case LogLevel.Debug: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Cyan); break;
			case LogLevel.Error: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkBlue); break;
			case LogLevel.Warn: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Blue); break;
			case LogLevel.Info: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkCyan); break;
			default: throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
		}
	}
}

