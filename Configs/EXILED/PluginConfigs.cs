using System.ComponentModel;
using Exiled.API.Interfaces;

namespace FoundationFortune.Configs.EXILED
{
	public class PluginConfigs : IConfig
	{
		[Description("Plugin Settings")]
		public bool IsEnabled { get; set; } = true;
		public bool Debug { get; set; } = true;
		public bool DirectoryIterator { get; set; } = true;
		public bool DirectoryIteratorCheckDatabase { get; set; } = true;
		public bool DirectoryIteratorCheckAudio { get; set; } = true;

		[Description("Update Rate Settings")] 
		public float HintSystemUpdateRate { get; set; } = 0.5f;
		public float AnimatedHintUpdateRate { get; set; } = 0.5f;
		public float NPCLookatUpdateRate { get; set; } = 0.01f;

		[Description("Amount of Death Coins to drop. NOTE: the value of the coins will be divided by the amount of coins. so if there's 10 coins a coin will be worth a tenth of the player's on hold money account.")]
		public int DeathCoinsToDrop { get; set; } = 10;

		[Description("Selling Workstation Settings.")]
		public bool UseSellingWorkstation { get; set; } = false;
		public float SellingWorkstationRadius { get; set; } = 3f;
	}
}
