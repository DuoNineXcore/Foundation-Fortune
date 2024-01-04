using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Core.Common.Enums;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Common.Interfaces.Configs;
using FoundationFortune.API.Core.Common.Models.Items;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs.FoundationFortune;

public class BuyableItemsList : IFoundationFortuneConfig
{
	[YamlIgnore] public string PropertyName { get; set; } = "Buyable Items Settings";
	
	[Description("List of items that can be bought.")]
	public List<BuyableItem> BuyableItems { get; set; } = new()
	{
		new () { Limit = 1, Alias = "Janitor", ItemType = ItemType.KeycardJanitor, Price = 100, DisplayName = "Janitor Keycard" },
		new () { Limit = 1, Alias = "Scientist", ItemType = ItemType.KeycardScientist, Price = 200, DisplayName = "Scientist Keycard" },
		new () { Limit = 1, Alias = "ResearchCoordinator", ItemType = ItemType.KeycardResearchCoordinator, Price = 250, DisplayName = "Research Coordinator Keycard" },
		new () { Limit = 1, Alias = "ZoneManager", ItemType = ItemType.KeycardZoneManager, Price = 300, DisplayName = "Zone Manager Keycard" },
		new () { Limit = 1, Alias = "Guard", ItemType = ItemType.KeycardGuard, Price = 400, DisplayName = "Guard Keycard" },
		new () { Limit = 1, Alias = "MTFPrivate", ItemType = ItemType.KeycardMTFPrivate, Price = 500, DisplayName = "MTF Private Keycard" },
		new () { Limit = 1, Alias = "ContainmentEngineer", ItemType = ItemType.KeycardContainmentEngineer, Price = 600, DisplayName = "Containment Engineer Keycard" },
		new () { Limit = 1, Alias = "MTFOperative", ItemType = ItemType.KeycardMTFOperative, Price = 700, DisplayName = "MTF Operative Keycard" },
		new () { Limit = 1, Alias = "MTFCaptain", ItemType = ItemType.KeycardMTFCaptain, Price = 800, DisplayName = "MTF Captain Keycard" },
		new () { Limit = 1, Alias = "FacilityManager", ItemType = ItemType.KeycardFacilityManager, Price = 900, DisplayName = "Facility Manager Keycard" },
		new () { Limit = 1, Alias = "ChaosInsurgency", ItemType = ItemType.KeycardChaosInsurgency, Price = 1000, DisplayName = "Chaos Insurgency Keycard" },
		new () { Limit = 1, Alias = "O5", ItemType = ItemType.KeycardO5, Price = 1200, DisplayName = "O5 Keycard" },
		new () { Limit = 1, Alias = "Radio", ItemType = ItemType.Radio, Price = 300, DisplayName = "Radio" },
		new () { Limit = 1, Alias = "GunCOM15", ItemType = ItemType.GunCOM15, Price = 800, DisplayName = "COM-15 Pistol" },
		new () { Limit = 1, Alias = "Medkit", ItemType = ItemType.Medkit, Price = 600, DisplayName = "Medkit" },
		new () { Limit = 1, Alias = "Flashlight", ItemType = ItemType.Flashlight, Price = 100, DisplayName = "Flashlight" },
		new () { Limit = 1, Alias = "MicroHID", ItemType = ItemType.MicroHID, Price = 1500, DisplayName = "Micro HID" },
		new () { Limit = 1, Alias = "SCP500", ItemType = ItemType.SCP500, Price = 2000, DisplayName = "SCP-500" },
		new () { Limit = 1, Alias = "SCP207", ItemType = ItemType.SCP207, Price = 800, DisplayName = "SCP-207" },
		new () { Limit = 1, Alias = "Ammo12gauge", ItemType = ItemType.Ammo12gauge, Price = 50, DisplayName = "12 Gauge Ammo" },
		new () { Limit = 1, Alias = "GunE11SR", ItemType = ItemType.GunE11SR, Price = 1200, DisplayName = "E-11 SR" },
		new () { Limit = 1, Alias = "GunCrossvec", ItemType = ItemType.GunCrossvec, Price = 1400, DisplayName = "Crossvec" },
		new () { Limit = 1, Alias = "Ammo556x45", ItemType = ItemType.Ammo556x45, Price = 100, DisplayName = "5.56x45mm Ammo" },
		new () { Limit = 1, Alias = "GunFSP9", ItemType = ItemType.GunFSP9, Price = 1100, DisplayName = "FSP-9" },
		new () { Limit = 1, Alias = "GunLogicer", ItemType = ItemType.GunLogicer, Price = 1800, DisplayName = "Logicer" },
		new () { Limit = 1, Alias = "GrenadeHE", ItemType = ItemType.GrenadeHE, Price = 400, DisplayName = "HE Grenade" },
		new () { Limit = 1, Alias = "GrenadeFlash", ItemType = ItemType.GrenadeFlash, Price = 300, DisplayName = "Flash Grenade" },
		new () { Limit = 1, Alias = "Ammo44cal", ItemType = ItemType.Ammo44cal, Price = 60, DisplayName = ".44 Cal Ammo" },
		new () { Limit = 1, Alias = "Ammo762x39", ItemType = ItemType.Ammo762x39, Price = 80, DisplayName = "7.62x39mm Ammo" },
		new () { Limit = 1, Alias = "Ammo9x19", ItemType = ItemType.Ammo9x19, Price = 40, DisplayName = "9x19mm Ammo" },
		new () { Limit = 1, Alias = "GunCOM18", ItemType = ItemType.GunCOM18, Price = 900, DisplayName = "COM-18 Pistol" },
		new () { Limit = 1, Alias = "SCP018", ItemType = ItemType.SCP018, Price = 600, DisplayName = "SCP-018" },
		new () { Limit = 1, Alias = "SCP268", ItemType = ItemType.SCP268, Price = 1000, DisplayName = "SCP-268" },
		new () { Limit = 1, Alias = "Adrenaline", ItemType = ItemType.Adrenaline, Price = 150, DisplayName = "Adrenaline" },
		new () { Limit = 1, Alias = "Painkillers", ItemType = ItemType.Painkillers, Price = 120, DisplayName = "Painkillers" },
		new () { Limit = 1, Alias = "Coin", ItemType = ItemType.Coin, Price = 5, DisplayName = "Coin" },
		new () { Limit = 1, Alias = "ArmorLight", ItemType = ItemType.ArmorLight, Price = 300, DisplayName = "Light Armor" },
		new () { Limit = 1, Alias = "ArmorCombat", ItemType = ItemType.ArmorCombat, Price = 500, DisplayName = "Combat Armor" },
		new () { Limit = 1, Alias = "ArmorHeavy", ItemType = ItemType.ArmorHeavy, Price = 700, DisplayName = "Heavy Armor" },
		new () { Limit = 1, Alias = "GunRevolver", ItemType = ItemType.GunRevolver, Price = 1000, DisplayName = "Revolver" },
		new () { Limit = 1, Alias = "GunAK", ItemType = ItemType.GunAK, Price = 1400, DisplayName = "AK" },
		new () { Limit = 1, Alias = "GunShotgun", ItemType = ItemType.GunShotgun, Price = 1600, DisplayName = "Shotgun" },
		new () { Limit = 1, Alias = "SCP330", ItemType = ItemType.SCP330, Price = 700, DisplayName = "SCP-330" },
		new () { Limit = 1, Alias = "SCP2176", ItemType = ItemType.SCP2176, Price = 900, DisplayName = "SCP-2176" },
		new () { Limit = 1, Alias = "SCP244a", ItemType = ItemType.SCP244a, Price = 1100, DisplayName = "SCP-244a" },
		new () { Limit = 1, Alias = "SCP244b", ItemType = ItemType.SCP244b, Price = 1100, DisplayName = "SCP-244b" },
		new () { Limit = 1, Alias = "SCP1853", ItemType = ItemType.SCP1853, Price = 800, DisplayName = "SCP-1853" },
		new () { Limit = 1, Alias = "ParticleDisruptor", ItemType = ItemType.ParticleDisruptor, Price = 1600, DisplayName = "Particle Disruptor" },
		new () { Limit = 1, Alias = "GunCom45", ItemType = ItemType.GunCom45, Price = 1000, DisplayName = "COM-45 Pistol" },
		new () { Limit = 1, Alias = "SCP1576", ItemType = ItemType.SCP1576, Price = 1300, DisplayName = "SCP-1576" },
		new () { Limit = 1, Alias = "Jailbird", ItemType = ItemType.Jailbird, Price = 500, DisplayName = "Jailbird" },
		new () { Limit = 1, Alias = "AntiSCP207", ItemType = ItemType.AntiSCP207, Price = 300, DisplayName = "Anti-SCP-207" },
		new () { Limit = 1, Alias = "GunFRMG0", ItemType = ItemType.GunFRMG0, Price = 1800, DisplayName = "FRMG-0" },
		new () { Limit = 1, Alias = "GunA7", ItemType = ItemType.GunA7, Price = 1400, DisplayName = "A7" },
		new () { Limit = 1, Alias = "Lantern", ItemType = ItemType.Lantern, Price = 200, DisplayName = "Lantern" }, 
	};

	public List<BuyableCustomItem> BuyableCustomItems { get; set; } = new()
	{
		new() { Limit = 1, Alias = "beacon", Price = 5000, DisplayName = "Resurgence Beacon", CustomItemType = CustomItemType.ResurgenceBeacon, ItemType = ItemType.Coin, Description = "revive someone with it lmao" }
	};

	public List<BuyablePerk> PerkItems { get; set; } = new()
	{
		new() { Limit = 1, Alias = "speed", PerkType = PerkType.HyperactiveBehavior, Price = 2500, DisplayName = "Hyperactivity", Description = "" },
		new() { Limit = 1, Alias = "regen", PerkType = PerkType.EthericVitality, Price = 2800, DisplayName = "Etheric Vitality", Description = "" },
		new() { Limit = 1, Alias = "rage", PerkType = PerkType.ViolentImpulses, Price = 3000, DisplayName = "Violent Impulses", Description = "" },
		new() { Limit = 1, Alias = "bliss", PerkType = PerkType.BlissfulAgony, Price = 3400, DisplayName = "Blissful Agony", Description = "" },
		new() { Limit = 1, Alias = "sacrifice", PerkType = PerkType.SacrificialSurge, Price = 3500, DisplayName = "Sacrificial Surge", Description = "" },
		new() { Limit = 1, Alias = "damage", PerkType = PerkType.VitalitySacrifice, Price = 3200, DisplayName = "Vitality Sacrifice", Description = "" },
		new() { Limit = 1, Alias = "resistance", PerkType = PerkType.ExplosiveResilience, Price = 3800, DisplayName = "Explosive Resilience", Description = "" },
		new() { Limit = 1, Alias = "selfres", PerkType = PerkType.GracefulSaint, Price = 4200, DisplayName = "Graceful Saint", Description = "" },
		new() { Limit = 1, Alias = "midas", PerkType = PerkType.TouchOfMidas, Price = 4500, DisplayName = "Touch of Midas", Description = "" },
	};
}