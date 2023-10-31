using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using FoundationFortune.Configs;
using HarmonyLib;
using LiteDB;
using MEC;
using SCPSLAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using FoundationFortune.API.HintSystem;
using FoundationFortune.API.Models;

namespace FoundationFortune
{
    public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
	{
		public override string Author => "DuoNineXcore & Misfiy";
		public override string Name => "Foundation Fortune";
		public override string Prefix => "this plugin is a performance issue";
		public override Version Version => new(1, 0, 0);

		public static readonly string commonDirectoryPath = Path.Combine(Paths.IndividualConfigs, "this plugin is a performance issue", "Foundation Fortune Assets");
		public static readonly string audioFilesPath = Path.Combine(Path.Combine(commonDirectoryPath, "Sound Files"));

		private Harmony harmony;

		public static FoundationFortune Singleton;
		public static List<ObjectInteractions> PlayerPurchaseLimits = new();
        public ServerEvents ServerEvents = new();
		public LiteDatabase db;

		public Dictionary<Player, Dictionary<PerkType, int>> ConsumedPerks = new();
        public Dictionary<string, (Npc bot, int indexation)> BuyingBots  = new();
		public Dictionary<string, (Npc bot, int indexation)> SellingBots = new();
		public List<PlayerMusicBotPair> MusicBotPairs { get; private set; } = new();
		
		public override void OnEnabled()
		{
			Singleton = this;
			CreateDirectories();
			RegisterEvents();
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
			UnregisterEvents();
			Singleton = null;
			CustomItem.UnregisterItems();
            harmony?.UnpatchAll(harmony.Id);
            harmony = null!;
            ServerEvents = null;
        }

        private void RegisterEvents()
		{
			Exiled.Events.Handlers.Player.Verified += ServerEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying += ServerEvents.EtherealInterventionHandler;
			Exiled.Events.Handlers.Player.Spawned += ServerEvents.EtherealInterventionSpawn;
			Exiled.Events.Handlers.Player.Died += ServerEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping += ServerEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem += ServerEvents.SellingItem;
			Exiled.Events.Handlers.Player.Left += ServerEvents.DestroyMusicBots; 
			Exiled.Events.Handlers.Player.Hurting += ServerEvents.HurtingPlayer;
			Exiled.Events.Handlers.Player.Shooting += ServerEvents.ShootingWeapon;

			Exiled.Events.Handlers.Server.RoundStarted += ServerEvents.RoundStart;
			Exiled.Events.Handlers.Server.RestartingRound += ServerEvents.RoundRestart;
			Exiled.Events.Handlers.Server.RoundEnded += ServerEvents.RoundEnded;
			Exiled.Events.Handlers.Server.RespawningTeam += ServerEvents.PreventBotsFromSpawningInWaves;
            
			Exiled.Events.Handlers.Scp049.ActivatingSense += ServerEvents.FuckYourAbility;
			Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += ServerEvents.FuckYourOtherAbility;
		}

		private void UnregisterEvents()
		{
			Exiled.Events.Handlers.Player.Verified -= ServerEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying -= ServerEvents.EtherealInterventionHandler;
			Exiled.Events.Handlers.Player.Spawned -= ServerEvents.EtherealInterventionSpawn;
			Exiled.Events.Handlers.Player.Died -= ServerEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping -= ServerEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem -= ServerEvents.SellingItem;
			Exiled.Events.Handlers.Player.Left -= ServerEvents.DestroyMusicBots;
			Exiled.Events.Handlers.Player.Hurting -= ServerEvents.HurtingPlayer;
			Exiled.Events.Handlers.Player.Shooting -= ServerEvents.ShootingWeapon;

			Exiled.Events.Handlers.Server.RoundStarted -= ServerEvents.RoundStart;
			Exiled.Events.Handlers.Server.RestartingRound -= ServerEvents.RoundRestart;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerEvents.RoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam -= ServerEvents.PreventBotsFromSpawningInWaves;
            
            Exiled.Events.Handlers.Scp049.ActivatingSense -= ServerEvents.FuckYourAbility;
            Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= ServerEvents.FuckYourOtherAbility;
		}

		private void CreateDirectories()
		{
			try
			{
				string databaseFilePath = Path.Combine(commonDirectoryPath, "Foundation Fortune.db");

				if (!File.Exists(databaseFilePath))
				{
					if (!Directory.Exists(commonDirectoryPath))
						Directory.CreateDirectory(commonDirectoryPath);

					if (!Directory.Exists(audioFilesPath))
						Directory.CreateDirectory(audioFilesPath);
					
					db = new LiteDatabase(databaseFilePath);

					var collection = db.GetCollection<PlayerData>();
					collection.EnsureIndex(x => x.UserId);

					Log.Info($"Database created successfully at {databaseFilePath}");
				}
				else
				{
					db = new LiteDatabase(databaseFilePath);
					Log.Info($"Database loaded successfully at {databaseFilePath}.");
				}
			}
			catch (Exception ex)
			{
				Log.Error($"Failed to create/open database: {ex}");
				Timing.CallDelayed(1f, OnDisabled);
			}
		}
	}
}
