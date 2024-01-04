using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace FoundationFortune.Configs.EXILED;

public class PluginConfigs : IConfig
{
	[Description("Plugin Settings")]
	public bool IsEnabled { get; set; } = true;
	public bool Debug { get; set; } = true;
		
	[Description("Directory Iterator Settings (plugin settings part 2)")]
	public bool DirectoryIterator { get; set; } = true;
	public bool DirectoryIteratorCheckAudio { get; set; } = true;
	public bool DirectoryIteratorCreateLogFiles { get; set; } = true;
	
	public List<string> RequiredFolders { get; set; }= new()
	{
		"\\Sound Files",
		"\\Sound Files\\PlayerVoiceChatUsageType",
		"\\Sound Files\\NPCVoiceChatUsageType"
	};

	public List<string> RequiredFiles { get; set; } = new()
	{
		"\\PlayerVoiceChatUsageType\\BlissfulUnawareness.ogg",
		"\\PlayerVoiceChatUsageType\\EtherealIntervention.ogg",
		"\\PlayerVoiceChatUsageType\\EthericVitality.ogg",
		"\\PlayerVoiceChatUsageType\\Hunted.ogg",
		"\\PlayerVoiceChatUsageType\\Hunter.ogg",
		"\\PlayerVoiceChatUsageType\\HyperactiveBehaviorOff.ogg",
		"\\PlayerVoiceChatUsageType\\HyperactiveBehaviorOn.ogg",
		"\\PlayerVoiceChatUsageType\\ResurgenceBeacon.ogg",
		"\\PlayerVoiceChatUsageType\\ViolentImpulses.ogg",
		"\\NPCVoiceChatUsageType\\Buying.ogg",
		"\\NPCVoiceChatUsageType\\BuyingBotInRange.ogg",
		"\\NPCVoiceChatUsageType\\BuyingRange.ogg",
		"\\NPCVoiceChatUsageType\\NotEnoughMoney.ogg",
		"\\NPCVoiceChatUsageType\\Selling.ogg",
		"\\NPCVoiceChatUsageType\\SellingBotInRange.ogg",
		"\\NPCVoiceChatUsageType\\WrongBuyingBot.ogg"
	};

	public string DirectoryIteratorFileURL { get; set; } = "https://github.com/DuoNineXcore/Foundation-Fortune-Assets/releases/latest/download/AudioFiles.zip";
	
	[Description("Update Rate Settings")] 
	public float HintSystemUpdateRate { get; set; } = 0.5f;
	public float NpcLookatUpdateRate { get; set; } = 0.01f;
}