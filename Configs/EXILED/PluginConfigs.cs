﻿using System.ComponentModel;
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

		[Description("Selling Workstation Settings.")]
		public bool UseSellingWorkstation { get; set; } = false;
		public float SellingWorkstationRadius { get; set; } = 3f;
	}
}
