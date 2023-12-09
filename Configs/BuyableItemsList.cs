using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces;
using FoundationFortune.API.Core.Models.Interfaces.Configs;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs
{
	public class BuyableItemsList : IFoundationFortuneConfig
	{
		[YamlIgnore] public string PropertyName { get; set; } = "Buyable Items Settings";
	
		[Description("List of items that can be bought.")]
		public List<BuyableItem> BuyableItems { get; set; } = new List<BuyableItem>
		{
			new BuyableItem { Limit = 1, Alias = "Janitor", ItemType = ItemType.KeycardJanitor, Price = 100, DisplayName = "Janitor Keycard" },
			new BuyableItem { Limit = 1, Alias = "Scientist", ItemType = ItemType.KeycardScientist, Price = 200, DisplayName = "Scientist Keycard" },
			new BuyableItem { Limit = 1, Alias = "ResearchCoordinator", ItemType = ItemType.KeycardResearchCoordinator, Price = 250, DisplayName = "Research Coordinator Keycard" },
			new BuyableItem { Limit = 1, Alias = "ZoneManager", ItemType = ItemType.KeycardZoneManager, Price = 300, DisplayName = "Zone Manager Keycard" },
			new BuyableItem { Limit = 1, Alias = "Guard", ItemType = ItemType.KeycardGuard, Price = 400, DisplayName = "Guard Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFPrivate", ItemType = ItemType.KeycardMTFPrivate, Price = 500, DisplayName = "MTF Private Keycard" },
			new BuyableItem { Limit = 1, Alias = "ContainmentEngineer", ItemType = ItemType.KeycardContainmentEngineer, Price = 600, DisplayName = "Containment Engineer Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFOperative", ItemType = ItemType.KeycardMTFOperative, Price = 700, DisplayName = "MTF Operative Keycard" },
			new BuyableItem { Limit = 1, Alias = "MTFCaptain", ItemType = ItemType.KeycardMTFCaptain, Price = 800, DisplayName = "MTF Captain Keycard" },
			new BuyableItem { Limit = 1, Alias = "FacilityManager", ItemType = ItemType.KeycardFacilityManager, Price = 900, DisplayName = "Facility Manager Keycard" },
			new BuyableItem { Limit = 1, Alias = "ChaosInsurgency", ItemType = ItemType.KeycardChaosInsurgency, Price = 1000, DisplayName = "Chaos Insurgency Keycard" },
			new BuyableItem { Limit = 1, Alias = "O5", ItemType = ItemType.KeycardO5, Price = 1200, DisplayName = "O5 Keycard" },
			new BuyableItem { Limit = 1, Alias = "Radio", ItemType = ItemType.Radio, Price = 300, DisplayName = "Radio" },
			new BuyableItem { Limit = 1, Alias = "GunCOM15", ItemType = ItemType.GunCOM15, Price = 800, DisplayName = "COM-15 Pistol" },
			new BuyableItem { Limit = 1, Alias = "Medkit", ItemType = ItemType.Medkit, Price = 600, DisplayName = "Medkit" },
			new BuyableItem { Limit = 1, Alias = "Flashlight", ItemType = ItemType.Flashlight, Price = 100, DisplayName = "Flashlight" },
			new BuyableItem { Limit = 1, Alias = "MicroHID", ItemType = ItemType.MicroHID, Price = 1500, DisplayName = "Micro HID" },
			new BuyableItem { Limit = 1, Alias = "SCP500", ItemType = ItemType.SCP500, Price = 2000, DisplayName = "SCP-500" },
			new BuyableItem { Limit = 1, Alias = "SCP207", ItemType = ItemType.SCP207, Price = 800, DisplayName = "SCP-207" },
			new BuyableItem { Limit = 1, Alias = "Ammo12gauge", ItemType = ItemType.Ammo12gauge, Price = 50, DisplayName = "12 Gauge Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunE11SR", ItemType = ItemType.GunE11SR, Price = 1200, DisplayName = "E-11 SR" },
			new BuyableItem { Limit = 1, Alias = "GunCrossvec", ItemType = ItemType.GunCrossvec, Price = 1400, DisplayName = "Crossvec" },
			new BuyableItem { Limit = 1, Alias = "Ammo556x45", ItemType = ItemType.Ammo556x45, Price = 100, DisplayName = "5.56x45mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunFSP9", ItemType = ItemType.GunFSP9, Price = 1100, DisplayName = "FSP-9" },
			new BuyableItem { Limit = 1, Alias = "GunLogicer", ItemType = ItemType.GunLogicer, Price = 1800, DisplayName = "Logicer" },
			new BuyableItem { Limit = 1, Alias = "GrenadeHE", ItemType = ItemType.GrenadeHE, Price = 400, DisplayName = "HE Grenade" },
			new BuyableItem { Limit = 1, Alias = "GrenadeFlash", ItemType = ItemType.GrenadeFlash, Price = 300, DisplayName = "Flash Grenade" },
			new BuyableItem { Limit = 1, Alias = "Ammo44cal", ItemType = ItemType.Ammo44cal, Price = 60, DisplayName = ".44 Cal Ammo" },
			new BuyableItem { Limit = 1, Alias = "Ammo762x39", ItemType = ItemType.Ammo762x39, Price = 80, DisplayName = "7.62x39mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "Ammo9x19", ItemType = ItemType.Ammo9x19, Price = 40, DisplayName = "9x19mm Ammo" },
			new BuyableItem { Limit = 1, Alias = "GunCOM18", ItemType = ItemType.GunCOM18, Price = 900, DisplayName = "COM-18 Pistol" },
			new BuyableItem { Limit = 1, Alias = "SCP018", ItemType = ItemType.SCP018, Price = 600, DisplayName = "SCP-018" },
			new BuyableItem { Limit = 1, Alias = "SCP268", ItemType = ItemType.SCP268, Price = 1000, DisplayName = "SCP-268" },
			new BuyableItem { Limit = 1, Alias = "Adrenaline", ItemType = ItemType.Adrenaline, Price = 150, DisplayName = "Adrenaline" },
			new BuyableItem { Limit = 1, Alias = "Painkillers", ItemType = ItemType.Painkillers, Price = 120, DisplayName = "Painkillers" },
			new BuyableItem { Limit = 1, Alias = "Coin", ItemType = ItemType.Coin, Price = 5, DisplayName = "Coin" },
			new BuyableItem { Limit = 1, Alias = "ArmorLight", ItemType = ItemType.ArmorLight, Price = 300, DisplayName = "Light Armor" },
			new BuyableItem { Limit = 1, Alias = "ArmorCombat", ItemType = ItemType.ArmorCombat, Price = 500, DisplayName = "Combat Armor" },
			new BuyableItem { Limit = 1, Alias = "ArmorHeavy", ItemType = ItemType.ArmorHeavy, Price = 700, DisplayName = "Heavy Armor" },
			new BuyableItem { Limit = 1, Alias = "GunRevolver", ItemType = ItemType.GunRevolver, Price = 1000, DisplayName = "Revolver" },
			new BuyableItem { Limit = 1, Alias = "GunAK", ItemType = ItemType.GunAK, Price = 1400, DisplayName = "AK" },
			new BuyableItem { Limit = 1, Alias = "GunShotgun", ItemType = ItemType.GunShotgun, Price = 1600, DisplayName = "Shotgun" },
			new BuyableItem { Limit = 1, Alias = "SCP330", ItemType = ItemType.SCP330, Price = 700, DisplayName = "SCP-330" },
			new BuyableItem { Limit = 1, Alias = "SCP2176", ItemType = ItemType.SCP2176, Price = 900, DisplayName = "SCP-2176" },
			new BuyableItem { Limit = 1, Alias = "SCP244a", ItemType = ItemType.SCP244a, Price = 1100, DisplayName = "SCP-244a" },
			new BuyableItem { Limit = 1, Alias = "SCP244b", ItemType = ItemType.SCP244b, Price = 1100, DisplayName = "SCP-244b" },
			new BuyableItem { Limit = 1, Alias = "SCP1853", ItemType = ItemType.SCP1853, Price = 800, DisplayName = "SCP-1853" },
			new BuyableItem { Limit = 1, Alias = "ParticleDisruptor", ItemType = ItemType.ParticleDisruptor, Price = 1600, DisplayName = "Particle Disruptor" },
			new BuyableItem { Limit = 1, Alias = "GunCom45", ItemType = ItemType.GunCom45, Price = 1000, DisplayName = "COM-45 Pistol" },
			new BuyableItem { Limit = 1, Alias = "SCP1576", ItemType = ItemType.SCP1576, Price = 1300, DisplayName = "SCP-1576" },
			new BuyableItem { Limit = 1, Alias = "Jailbird", ItemType = ItemType.Jailbird, Price = 500, DisplayName = "Jailbird" },
			new BuyableItem { Limit = 1, Alias = "AntiSCP207", ItemType = ItemType.AntiSCP207, Price = 300, DisplayName = "Anti-SCP-207" },
			new BuyableItem { Limit = 1, Alias = "GunFRMG0", ItemType = ItemType.GunFRMG0, Price = 1800, DisplayName = "FRMG-0" },
			new BuyableItem { Limit = 1, Alias = "GunA7", ItemType = ItemType.GunA7, Price = 1400, DisplayName = "A7" },
			new BuyableItem { Limit = 1, Alias = "Lantern", ItemType = ItemType.Lantern, Price = 200, DisplayName = "Lantern" }, 
		};	
	
		public List<BuyablePerk> PerkItems { get; set; } = new List<BuyablePerk>
		{
			new BuyablePerk { Limit = 1, Alias = "speed", PerkType = PerkType.HyperactiveBehavior, Price = 2500, DisplayName = "Hyperactivity", Description = "" },
			new BuyablePerk { Limit = 1, Alias = "regen", PerkType = PerkType.EthericVitality, Price = 2800, DisplayName = "Etheric Vitality", Description = "" },
			new BuyablePerk { Limit = 1, Alias = "damage", PerkType = PerkType.ViolentImpulses, Price = 3000, DisplayName = "Violent Impulses", Description = "" },
			new BuyablePerk { Limit = 1, Alias = "bliss", PerkType = PerkType.BlissfulUnawareness, Price = 3400, DisplayName = "Blissful Unawareness", Description = "" },
			new BuyablePerk { Limit = 1, Alias = "revive", PerkType = PerkType.ResurgenceBeacon, Price = 4000, DisplayName = "Resurgence Beacon", Description = "" },
			new BuyablePerk { Limit = 1, Alias = "selfres", PerkType = PerkType.EtherealIntervention, Price = 4000, DisplayName = "Ethereal Intervention", Description = "" },
		};
	}
}
