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
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Core.Models.Classes.NPCs;
using FoundationFortune.API.Core.Models.Enums.Perks;
using FoundationFortune.API.Features.Systems;
using FoundationFortune.Configs.EXILED;
using FoundationFortune.EventHandlers;
using MEC;

namespace FoundationFortune;
/// <summary>
/// the leg
/// </summary>
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
	public HintSystem HintSystem = new();
	private FoundationFortuneEventHandlers FoundationFortuneEventHandlers = new();
	private EXILEDEventHandlers ExiledEventHandlers = new();
	public LiteDatabase db;
	
	public Dictionary<Player, Dictionary<PerkType, int>> ConsumedPerks = new();
	public Dictionary<Player, Dictionary<PerkType, CoroutineHandle>> PerkCoroutines = new();
	
	public Dictionary<string, (Npc bot, int indexation)> BuyingBots = new();
	public Dictionary<string, (Npc bot, int indexation)> SellingBots = new();
	public List<PlayerMusicBotPair> MusicBotPairs { get; private set; } = new();

	public override void OnEnabled()
	{
		BuddyHollyLyrics.WelcomeText();
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
		ExiledEventHandlers = null;
		FoundationFortuneEventHandlers = null;
		HintSystem = null;
	}

	private static void SetupConfigs()
	{
		BuyableItemsList = DirectoryIterator.LoadAndAssignConfig<BuyableItemsList>();
		PerkSystemSettings = DirectoryIterator.LoadAndAssignConfig<PerkSystemSettings>();
		MoneyExtractionSystemSettings = DirectoryIterator.LoadAndAssignConfig<MoneyExtractionSystemSettings>();
		SellableItemsList = DirectoryIterator.LoadAndAssignConfig<SellableItemsList>();
		VoiceChatSettings = DirectoryIterator.LoadAndAssignConfig<VoiceChatSettings>();
		MoneyXpRewards = DirectoryIterator.LoadAndAssignConfig<MoneyXPRewards>();
		FoundationFortuneNpcSettings = DirectoryIterator.LoadAndAssignConfig<FoundationFortuneNPCSettings>();
	}
	
	private void RegisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified += ExiledEventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying += ExiledEventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Spawned += ExiledEventHandlers.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died += ExiledEventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping += ExiledEventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem += ExiledEventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left += ExiledEventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting += ExiledEventHandlers.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting += ExiledEventHandlers.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted += ExiledEventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound += ExiledEventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded += ExiledEventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam += ExiledEventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense += ExiledEventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += ExiledEventHandlers.FuckYourOtherAbility;
		
		FoundationFortuneNPCs.SoldItem += FoundationFortuneEventHandlers.SoldItem;
		FoundationFortuneNPCs.BoughtItem += FoundationFortuneEventHandlers.BoughtItem;
		FoundationFortuneNPCs.BoughtPerk += FoundationFortuneEventHandlers.BoughtPerk;
		FoundationFortuneNPCs.UsedFoundationFortuneNPC += FoundationFortuneEventHandlers.UsedFoundationFortuneNPC;
		FoundationFortunePerks.UsedFoundationFortunePerk += FoundationFortuneEventHandlers.UsedFoundationFortunePerk;
	}

	private void UnregisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified -= ExiledEventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying -= ExiledEventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Spawned -= ExiledEventHandlers.EtherealInterventionSpawn;
		Exiled.Events.Handlers.Player.Died -= ExiledEventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping -= ExiledEventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem -= ExiledEventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left -= ExiledEventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.Hurting -= ExiledEventHandlers.HurtingPlayer;
		Exiled.Events.Handlers.Player.Shooting -= ExiledEventHandlers.ShootingWeapon;
		Exiled.Events.Handlers.Server.RoundStarted -= ExiledEventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound -= ExiledEventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded -= ExiledEventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam -= ExiledEventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense -= ExiledEventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= ExiledEventHandlers.FuckYourOtherAbility;

		FoundationFortuneNPCs.SoldItem -= FoundationFortuneEventHandlers.SoldItem;
		FoundationFortuneNPCs.BoughtItem -= FoundationFortuneEventHandlers.BoughtItem;
		FoundationFortuneNPCs.BoughtPerk -= FoundationFortuneEventHandlers.BoughtPerk;
		FoundationFortuneNPCs.UsedFoundationFortuneNPC -= FoundationFortuneEventHandlers.UsedFoundationFortuneNPC;
		FoundationFortunePerks.UsedFoundationFortunePerk -= FoundationFortuneEventHandlers.UsedFoundationFortunePerk;
	}
	
	public static void Log(string message, LogLevel severity)
	{
		var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
		if (declaringType == null) return;
		string className = declaringType.Name;
		string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

		string logEntry = $"[Foundation Fortune 1.0 - {severity.ToString().ToUpper()}] {className}::{methodName} -> {message}";
		switch (severity)
		{
			case LogLevel.Debug: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkCyan); break;
			case LogLevel.Error: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.DarkBlue); break;
			case LogLevel.Warn: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Blue); break;
			case LogLevel.Info: Exiled.API.Features.Log.SendRaw(logEntry, ConsoleColor.Cyan); break;
			default: throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
		}
	}
	
	public static void Log(string message, ConsoleColor color)
	{
		var declaringType = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType;
		if (declaringType == null) return;
		string className = declaringType.Name;
		string methodName = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;

		string logEntry = $"[Foundation Fortune 1.0] {className}::{methodName} -> {message}";
		Exiled.API.Features.Log.SendRaw(logEntry, color);
	}
}

