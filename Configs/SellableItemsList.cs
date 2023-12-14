using System.Collections.Generic;
using System.ComponentModel;
using FoundationFortune.API.Common.Interfaces.Configs;
using FoundationFortune.API.Common.Models.Items;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs;

public class SellableItemsList : IFoundationFortuneConfig
{
	[YamlIgnore] public string PropertyName { get; set; } = "Sellable Items Settings";
	
	[Description("List of items that can be sold.")]
	public List<SellableItem> SellableItems { get; set; } = new List<SellableItem>
	{
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardJanitor, Price = 50, DisplayName = "Janitor Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardScientist, Price = 100, DisplayName = "Scientist Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardResearchCoordinator, Price = 120, DisplayName = "Research Coordinator Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardZoneManager, Price = 150, DisplayName = "Zone Manager Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardGuard, Price = 200, DisplayName = "Guard Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFPrivate, Price = 250, DisplayName = "MTF Private Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardContainmentEngineer, Price = 300, DisplayName = "Containment Engineer Keycard" }, 
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFOperative, Price = 350, DisplayName = "MTF Operative Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardMTFCaptain, Price = 400, DisplayName = "MTF Captain Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardFacilityManager, Price = 450, DisplayName = "Facility Manager Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardChaosInsurgency, Price = 500, DisplayName = "Chaos Insurgency Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.KeycardO5, Price = 600, DisplayName = "O5 Keycard" },
		new SellableItem { Limit = 1, ItemType = ItemType.Radio, Price = 75, DisplayName = "Radio" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunCOM15, Price = 150, DisplayName = "COM-15 Pistol" },
		new SellableItem { Limit = 1, ItemType = ItemType.Medkit, Price = 100, DisplayName = "Medkit" },
		new SellableItem { Limit = 1, ItemType = ItemType.Flashlight, Price = 20, DisplayName = "Flashlight" },
		new SellableItem { Limit = 1, ItemType = ItemType.MicroHID, Price = 600, DisplayName = "Micro HID" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP500, Price = 800, DisplayName = "SCP-500" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP207, Price = 300, DisplayName = "SCP-207" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunE11SR, Price = 600, DisplayName = "E-11 SR" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunCrossvec, Price = 700, DisplayName = "Crossvec" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunFSP9, Price = 550, DisplayName = "FSP-9" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunLogicer, Price = 900, DisplayName = "Logicer" },
		new SellableItem { Limit = 1, ItemType = ItemType.GrenadeHE, Price = 200, DisplayName = "HE Grenade" },
		new SellableItem { Limit = 1, ItemType = ItemType.GrenadeFlash, Price = 150, DisplayName = "Flash Grenade" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunCOM18, Price = 450, DisplayName = "COM-18 Pistol" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP018, Price = 300, DisplayName = "SCP-018" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP268, Price = 500, DisplayName = "SCP-268" },
		new SellableItem { Limit = 1, ItemType = ItemType.Adrenaline, Price = 75, DisplayName = "Adrenaline" },
		new SellableItem { Limit = 1, ItemType = ItemType.Painkillers, Price = 60, DisplayName = "Painkillers" },
		new SellableItem { Limit = 1, ItemType = ItemType.Coin, Price = 1, DisplayName = "Coin" },
		new SellableItem { Limit = 1, ItemType = ItemType.ArmorLight, Price = 150, DisplayName = "Light Armor" },
		new SellableItem { Limit = 1, ItemType = ItemType.ArmorCombat, Price = 250, DisplayName = "Combat Armor" },
		new SellableItem { Limit = 1, ItemType = ItemType.ArmorHeavy, Price = 350, DisplayName = "Heavy Armor" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunRevolver, Price = 500, DisplayName = "Revolver" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunAK, Price = 700, DisplayName = "AK" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunShotgun, Price = 800, DisplayName = "Shotgun" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP330, Price = 350, DisplayName = "SCP-330" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP2176, Price = 450, DisplayName = "SCP-2176" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP244a, Price = 550, DisplayName = "SCP-244a" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP244b, Price = 550, DisplayName = "SCP-244b" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP1853, Price = 400, DisplayName = "SCP-1853" },
		new SellableItem { Limit = 1, ItemType = ItemType.ParticleDisruptor, Price = 800, DisplayName = "Particle Disruptor" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunCom45, Price = 500, DisplayName = "COM-45 Pistol" },
		new SellableItem { Limit = 1, ItemType = ItemType.SCP1576, Price = 650, DisplayName = "SCP-1576" },
		new SellableItem { Limit = 1, ItemType = ItemType.Jailbird, Price = 250, DisplayName = "Jailbird" },
		new SellableItem { Limit = 1, ItemType = ItemType.AntiSCP207, Price = 150, DisplayName = "Anti-SCP-207" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunFRMG0, Price = 900, DisplayName = "FRMG-0" },
		new SellableItem { Limit = 1, ItemType = ItemType.GunA7, Price = 700, DisplayName = "A7" },
		new SellableItem { Limit = 1, ItemType = ItemType.Lantern, Price = 10, DisplayName = "Lantern" },
	};
	
	public bool UseSellingWorkstation { get; set; } = false;
	public float SellingWorkstationRadius { get; set; } = 3f;
	public int DeathCoinsToDrop { get; set; } = 10;
}