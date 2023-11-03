using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using FoundationFortune.Configs;
using HarmonyLib;
using LiteDB;
using SCPSLAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using FoundationFortune.API;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Classes.NPCs;
using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune
{
	// ReSharper disable once ClassNeverInstantiated.Global
	// STFU!!!!!!!!!!!!!
	
	/// <summary>
	/// ur here for the singleton arent you
	/// </summary>
	public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
	{
		public override string Author => "DuoNineXcore & Misfiy";
		public override string Name => "Foundation Fortune";
		public override string Prefix => "this plugin is a performance issue";
		public override Version Version => new(1, 0, 0);

		public static readonly string commonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets");
		public static readonly string audioFilesPath = Path.Combine(commonDirectoryPath, "Sound Files");
		public static readonly string playerAudioFilesPath = Path.Combine(audioFilesPath, "PlayerVoiceChatUsageType");
		public static readonly string npcAudioFilesPath = Path.Combine(audioFilesPath, "NPCVoiceChatUsageType");

		private Harmony harmony;

		public static FoundationFortune Singleton;
		public static List<ObjectInteractions> PlayerPurchaseLimits = new();
        public FoundationFortuneAPI FoundationFortuneAPI = new();
		public LiteDatabase db;

		public Dictionary<Player, Dictionary<PerkType, int>> ConsumedPerks = new();
        public Dictionary<string, (Npc bot, int indexation)> BuyingBots  = new();
		public Dictionary<string, (Npc bot, int indexation)> SellingBots = new();
		public List<PlayerMusicBotPair> MusicBotPairs { get; private set; } = new();
		
		public override void OnEnabled()
		{
			Singleton = this;
			RegisterEvents();
			DirectoryIterator.SetupDirectories();
			Startup.SetupDependencies();
			CustomItem.RegisterItems();
			harmony = new("FoundationFortune");
			harmony.PatchAll();
		}

		public override void OnDisabled()
		{
			CoroutineManager.KillCoroutines();
			db?.Checkpoint();
			db?.Dispose();
			db = null;
			UnregisterEvents();
			Singleton = null;
			CustomItem.UnregisterItems();
            harmony?.UnpatchAll(harmony.Id);
            harmony = null!;
            FoundationFortuneAPI = null;
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
	}
}
