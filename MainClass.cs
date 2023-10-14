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

namespace FoundationFortune
{
    public class FoundationFortune : Plugin<PluginConfigs, PluginTranslations>
	{
		public override string Author => "DuoNineXcore & Misfiy";
		public override string Name => "Foundation Fortune";
		public override string Prefix => "this plugin is a performance issue";
		public override Version Version => new(1, 0, 0);

		private Harmony _harmony;

		public static FoundationFortune Singleton;
		#nullable enable
		public Dictionary<string, (Npc? bot, int indexation)> BuyingBotIndexation { get; private set; } = new Dictionary<string, (Npc? bot, int indexation)>();
		#nullable disable
		public static List<ObjectInteractions> PlayerLimits = new();
        public ServerEvents serverEvents = new();
		public LiteDatabase db;

		public override void OnEnabled()
		{
			Singleton = this;
			CreateDatabase();
			RegisterEvents();
			Startup.SetupDependencies();
			CustomItem.RegisterItems();
			if (_harmony == null)
			{
				_harmony = new("FoundationFortune");
				_harmony.PatchAll();
			}
		}

		public override void OnDisabled()
		{
			db?.Checkpoint();
			db?.Dispose();
			db = null;
			UnregisterEvents();
			Singleton = null;
			CustomItem.UnregisterItems();
            _harmony?.UnpatchAll(_harmony.Id);
        }

		private void RegisterEvents()
		{
			Exiled.Events.Handlers.Server.RoundStarted += serverEvents.RoundStart;
			Exiled.Events.Handlers.Player.Verified += serverEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying += serverEvents.EthernalInterventionHandler;
			Exiled.Events.Handlers.Player.Died += serverEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping += serverEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem += serverEvents.SellingItem;
			Exiled.Events.Handlers.Scp049.ActivatingSense += serverEvents.FuckYourAbility;
			Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += serverEvents.FuckYourOtherAbility;
			Exiled.Events.Handlers.Player.Spawning += serverEvents.SpawningNpc;
			Exiled.Events.Handlers.Server.EndingRound += serverEvents.RoundEnding;
			Exiled.Events.Handlers.Server.RestartingRound += serverEvents.RoundRestart;
		}

		private void UnregisterEvents()
		{
			Exiled.Events.Handlers.Server.RoundStarted -= serverEvents.RoundStart;
			Exiled.Events.Handlers.Player.Verified -= serverEvents.RegisterInDatabase;
			Exiled.Events.Handlers.Player.Dying -= serverEvents.EthernalInterventionHandler;
			Exiled.Events.Handlers.Player.Died -= serverEvents.KillingReward;
			Exiled.Events.Handlers.Player.Escaping -= serverEvents.EscapingReward;
			Exiled.Events.Handlers.Player.DroppingItem -= serverEvents.SellingItem;
			Exiled.Events.Handlers.Scp049.ActivatingSense -= serverEvents.FuckYourAbility;
			Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= serverEvents.FuckYourOtherAbility;
			Exiled.Events.Handlers.Player.Spawning -= serverEvents.SpawningNpc;
			Exiled.Events.Handlers.Server.EndingRound -= serverEvents.RoundEnding;
			Exiled.Events.Handlers.Server.RestartingRound -= serverEvents.RoundRestart;

			serverEvents = null;
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
