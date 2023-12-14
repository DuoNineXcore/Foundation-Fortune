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
	public bool DirectoryIteratorCheckDatabase { get; set; } = true;
	public bool DirectoryIteratorCheckAudio { get; set; } = true;
	public bool DirectoryIteratorCreateLogFiles { get; set; } = true;

	[Description("Update Rate Settings")] 
	public float HintSystemUpdateRate { get; set; } = 0.5f;
	public float NpcLookatUpdateRate { get; set; } = 0.01f;
}