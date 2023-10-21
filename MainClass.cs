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
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.HintSystem;
using FoundationFortune.API.Models.Enums;

namespace FoundationFortune
{
    public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
	{
		public override string Author => "DuoNineXcore & Misfiy";
		public override string Name => "Foundation Fortune";
		public override string Prefix => "this plugin is a performance issue";
		public override Version Version => new(1, 0, 0);

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
			CreateDatabase();
			RegisterEvents();
			Startup.SetupDependencies();
			CustomItem.RegisterItems();
			if (harmony != null) return;
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
            ServerEvents = null;
        }

        private void RegisterEvents()
		{
			Exiled.Events.Handlers.Server.RoundStarted += ServerEvents.RoundStart;
			Exiled.Events.Handlers.Player.Verified += ServerEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying += ServerEvents.EtherealInterventionHandler;
			Exiled.Events.Handlers.Player.Spawned += ServerEvents.EtherealInterventionSpawn;
			Exiled.Events.Handlers.Player.Died += ServerEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping += ServerEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem += ServerEvents.SellingItem;
			Exiled.Events.Handlers.Scp049.ActivatingSense += ServerEvents.FuckYourAbility;
			Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += ServerEvents.FuckYourOtherAbility;
			Exiled.Events.Handlers.Player.Spawning += ServerEvents.SpawningNpc;
			Exiled.Events.Handlers.Server.EndingRound += ServerEvents.RoundEnding;
			Exiled.Events.Handlers.Server.RestartingRound += ServerEvents.RoundRestart;
			Exiled.Events.Handlers.Server.RoundEnded += ServerEvents.RoundEnded;
			Exiled.Events.Handlers.Server.RespawningTeam += ServerEvents.PreventBotsFromSpawning;
			Exiled.Events.Handlers.Player.Left += ServerEvents.DestroyMusicBots;
		}

		private void UnregisterEvents()
		{
			Exiled.Events.Handlers.Server.RoundStarted -= ServerEvents.RoundStart;
			Exiled.Events.Handlers.Player.Verified -= ServerEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying -= ServerEvents.EtherealInterventionHandler;
            Exiled.Events.Handlers.Player.Spawned -= ServerEvents.EtherealInterventionSpawn;
            Exiled.Events.Handlers.Player.Died -= ServerEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping -= ServerEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem -= ServerEvents.SellingItem;
			Exiled.Events.Handlers.Scp049.ActivatingSense -= ServerEvents.FuckYourAbility;
			Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= ServerEvents.FuckYourOtherAbility;
			Exiled.Events.Handlers.Player.Spawning -= ServerEvents.SpawningNpc;
			Exiled.Events.Handlers.Server.EndingRound -= ServerEvents.RoundEnding;
			Exiled.Events.Handlers.Server.RestartingRound -= ServerEvents.RoundRestart;
            Exiled.Events.Handlers.Server.RoundEnded -= ServerEvents.RoundEnded;
            Exiled.Events.Handlers.Server.RespawningTeam -= ServerEvents.PreventBotsFromSpawning;
            Exiled.Events.Handlers.Player.Left -= ServerEvents.DestroyMusicBots;
		}

		private void CreateDatabase()
		{
			try
			{
				string databaseDirectoryPath = Path.Combine(Paths.Configs, "Foundation Fortune");
				string databaseFilePath = Path.Combine(databaseDirectoryPath, "Foundation Fortune.db");

				if (!File.Exists(databaseFilePath))
				{
					if (!Directory.Exists(databaseDirectoryPath))
						Directory.CreateDirectory(databaseDirectoryPath);

					db = new LiteDatabase(databaseFilePath);

					var collection = db.GetCollection<PlayerData>();
					collection.EnsureIndex(x => x.UserId, false);

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
