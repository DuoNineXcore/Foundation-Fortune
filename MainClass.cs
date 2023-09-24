using PlayerRoles;
using LiteDB;
using MEC;
using System.IO;
using System;
using FoundationFortune.API.Database;
using FoundationFortune.Events;
using Exiled.API.Features;
using Exiled.API.Features.Attributes;
using Exiled.CustomItems.API.Features;
using Exiled.API.Features.Spawn;
using System.Collections.Generic;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Items;
using HarmonyLib;
using SCPSLAudioApi;
using FoundationFortune.API.NPCs;
using System.Reflection;

namespace FoundationFortune
{
	public class FoundationFortune : Plugin<Config>
	{
        public override string Author => "DuoNineXcore";
        public override string Name => "Foundation Fortune";
        public override string Prefix => "foundationfortune";

        private Harmony _harmony;

        public static FoundationFortune Singleton;
        public Dictionary<string, (Npc? bot, int indexation)> BuyingBotIndexation { get; private set; } = new Dictionary<string, (Npc? bot, int indexation)>();
        public Dictionary<string, List<PerkType>> purchasedPerks = new Dictionary<string, List<PerkType>>();
        public ServerEvents serverEvents = new();
        public LiteDatabase db;

        public override void OnEnabled()
        {
            Singleton = this;
            CreateDatabase();
            RegisterEvents();
            Startup.SetupDependencies();
            CustomItem.RegisterItems();
            if (_harmony is null)
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
        }

        private void RegisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted += serverEvents.RoundStart;
            Exiled.Events.Handlers.Player.Verified += serverEvents.RegisterInDatabase;
            Exiled.Events.Handlers.Player.Died += serverEvents.KillingReward;
            Exiled.Events.Handlers.Player.Escaping += serverEvents.EscapingReward;
            Exiled.Events.Handlers.Player.DroppingItem += serverEvents.SellingItem;
            Exiled.Events.Handlers.Scp049.ActivatingSense += serverEvents.FuckYourAbility;
            Exiled.Events.Handlers.Scp0492.TriggeringBloodlust += serverEvents.FuckYourOtherAbility;
            Exiled.Events.Handlers.Player.Spawning += serverEvents.SpawningNpc;
            //CharacterClassManager.OnInstanceModeChanged += serverEvents.InstanceModeChange;
        }

        private void UnregisterEvents()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= serverEvents.RoundStart;
            Exiled.Events.Handlers.Player.Verified -= serverEvents.RegisterInDatabase;
            Exiled.Events.Handlers.Player.Died -= serverEvents.KillingReward;
            Exiled.Events.Handlers.Player.Escaping -= serverEvents.EscapingReward;
            Exiled.Events.Handlers.Player.DroppingItem -= serverEvents.SellingItem;
            Exiled.Events.Handlers.Scp049.ActivatingSense -= serverEvents.FuckYourAbility;
            Exiled.Events.Handlers.Scp0492.TriggeringBloodlust -= serverEvents.FuckYourOtherAbility;
            Exiled.Events.Handlers.Player.Spawning -= serverEvents.SpawningNpc;
            //CharacterClassManager.OnInstanceModeChanged -= serverEvents.InstanceModeChange;
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
