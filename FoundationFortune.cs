using System;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Core.Systems;
using FoundationFortune.Configs.EXILED;
using FoundationFortune.Configs.FoundationFortune;
using FoundationFortune.EventHandlers;
using HarmonyLib;
using LiteDB;
using SCPSLAudioApi;

namespace FoundationFortune;

/// <summary>
/// five pebbles
/// </summary>
public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
{
	public override string Author => "DuoNineXcore";
	public override string Name => "Foundation Fortune";
	public override string Prefix => "this plugin is a performance issue";
	public override Version Version => new(1, 0, 0);

	public static BuyableItemsList BuyableItemsList;
	public static PerkSystemSettings PerkSystemSettings;
	public static MoneyExtractionSystemSettings MoneyExtractionSystemSettings;
	public static SellableItemsList SellableItemsList;
	public static VoiceChatSettings VoiceChatSettings;
	public static MoneyXPRewards MoneyXPRewards;
	public static FoundationFortuneNpcSettings FoundationFortuneNpcSettings; 
		
	public static FoundationFortune Instance;

	public LiteDatabase PlayerSettingsDatabase;
	public LiteDatabase PlayerStatsDatabase;
	public LiteDatabase QuestRotationDatabase;

	public FoundationFortuneEventHandlers FoundationFortuneEventHandlers = new();
	public ExiledEventHandlers ExiledEventHandlers = new();
	public HintSystem HintSystem = new();

	private Harmony _harmony;

	public override void OnEnabled()
	{
		Instance = this;
		_harmony = new("Foundation Fortune");
		_harmony.PatchAll();
		DirectoryIterator.InitializeLogFile();
		DirectoryIterator.InitializeDatabases();
		DirectoryIterator.SetupDirectories();
		RegisterEvents();
		SetupConfigs();
		WelcomeText.PrintMessage();
		Startup.SetupDependencies();
		CustomItem.RegisterItems();
	}

	public override void OnDisabled()
	{
		PlayerSettingsDatabase?.Checkpoint();
		PlayerSettingsDatabase?.Dispose();
		PlayerStatsDatabase?.Checkpoint();
		PlayerStatsDatabase?.Dispose();
		QuestRotationDatabase?.Checkpoint();
		QuestRotationDatabase?.Dispose();
		_harmony = null!;
		ExiledEventHandlers = null;
		FoundationFortuneEventHandlers = null;
		Instance = null;
		PlayerSettingsDatabase = null;
		PlayerStatsDatabase = null;
		QuestRotationDatabase = null;
		_harmony?.UnpatchAll(_harmony.Id);
		UnregisterEvents();
		IndexationMethods.ClearIndexations();
		CoroutineManager.StopAllCoroutines();
		CustomItem.UnregisterItems();
	}

	private static void SetupConfigs()
	{
		BuyableItemsList = DirectoryIterator.LoadAndAssignConfig<BuyableItemsList>();
		PerkSystemSettings = DirectoryIterator.LoadAndAssignConfig<PerkSystemSettings>();
		MoneyExtractionSystemSettings = DirectoryIterator.LoadAndAssignConfig<MoneyExtractionSystemSettings>();
		SellableItemsList = DirectoryIterator.LoadAndAssignConfig<SellableItemsList>();
		VoiceChatSettings = DirectoryIterator.LoadAndAssignConfig<VoiceChatSettings>();
		MoneyXPRewards = DirectoryIterator.LoadAndAssignConfig<MoneyXPRewards>();
		FoundationFortuneNpcSettings = DirectoryIterator.LoadAndAssignConfig<FoundationFortuneNpcSettings>();
	}
	
	private void RegisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified += ExiledEventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying += ExiledEventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Died += ExiledEventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping += ExiledEventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem += ExiledEventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left += ExiledEventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.ThrownProjectile += ExiledEventHandlers.ThrownGhostlight;
		Exiled.Events.Handlers.Player.UnlockingGenerator += ExiledEventHandlers.UnlockingGenerator;
		Exiled.Events.Handlers.Player.TogglingNoClip += ExiledEventHandlers.ActivatingPerk;
		Exiled.Events.Handlers.Server.RoundStarted += ExiledEventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound += ExiledEventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded += ExiledEventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam += ExiledEventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp173.BlinkingRequest += ExiledEventHandlers.BlinkingRequestEvent;
		Exiled.Events.Handlers.Scp096.AddingTarget += ExiledEventHandlers.AddingTargetEvent;
		Exiled.Events.Handlers.Scp049.ActivatingSense += ExiledEventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += ExiledEventHandlers.FuckYourOtherAbility;

		FoundationFortuneItemEvents.SoldItem += FoundationFortuneEventHandlers.SoldItem;
		FoundationFortuneItemEvents.BoughtItem += FoundationFortuneEventHandlers.BoughtItem;
		FoundationFortuneItemEvents.BoughtPerk += FoundationFortuneEventHandlers.BoughtPerk;
		FoundationFortuneNPCEvents.UsedFoundationFortuneNpc += FoundationFortuneEventHandlers.UsedFoundationFortuneNpc;
	}

	private void UnregisterEvents()
	{
		Exiled.Events.Handlers.Player.Verified -= ExiledEventHandlers.RegisterInDatabase;
		Exiled.Events.Handlers.Player.Dying -= ExiledEventHandlers.DyingEvent;
		Exiled.Events.Handlers.Player.Died -= ExiledEventHandlers.KillingReward;
		Exiled.Events.Handlers.Player.Escaping -= ExiledEventHandlers.EscapingReward;
		Exiled.Events.Handlers.Player.DroppingItem -= ExiledEventHandlers.SellingItem;
		Exiled.Events.Handlers.Player.Left -= ExiledEventHandlers.DestroyMusicBots;
		Exiled.Events.Handlers.Player.ThrownProjectile -= ExiledEventHandlers.ThrownGhostlight;
		Exiled.Events.Handlers.Player.UnlockingGenerator -= ExiledEventHandlers.UnlockingGenerator;
		Exiled.Events.Handlers.Player.TogglingNoClip -= ExiledEventHandlers.ActivatingPerk;
		Exiled.Events.Handlers.Server.RoundStarted -= ExiledEventHandlers.RoundStart;
		Exiled.Events.Handlers.Server.RestartingRound -= ExiledEventHandlers.RoundRestart;
		Exiled.Events.Handlers.Server.RoundEnded -= ExiledEventHandlers.RoundEnded;
		Exiled.Events.Handlers.Server.RespawningTeam -= ExiledEventHandlers.PreventBotsFromSpawningInWaves;
		Exiled.Events.Handlers.Scp049.ActivatingSense -= ExiledEventHandlers.FuckYourAbility;
		Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= ExiledEventHandlers.FuckYourOtherAbility;

		FoundationFortuneItemEvents.SoldItem -= FoundationFortuneEventHandlers.SoldItem;
		FoundationFortuneItemEvents.BoughtItem -= FoundationFortuneEventHandlers.BoughtItem;
		FoundationFortuneItemEvents.BoughtPerk -= FoundationFortuneEventHandlers.BoughtPerk;
		FoundationFortuneNPCEvents.UsedFoundationFortuneNpc -= FoundationFortuneEventHandlers.UsedFoundationFortuneNpc;
	}
}