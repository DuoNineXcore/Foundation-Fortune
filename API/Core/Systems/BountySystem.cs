using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Player;
using FoundationFortune.API.Common.Models.Events;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Features;
using MEC;

namespace FoundationFortune.API.Core.Systems;

public static class BountySystem
{
	public static List<Bounty> BountiedPlayers { get; } = new List<Bounty>();

	public static void AddBounty(Player player, int bountyPrice, TimeSpan duration)
	{
		PlayerVoiceChatSettings hunted = FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunted);
		PlayerVoiceChatSettings hunter = FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunter);
			
		DateTime expirationTime = DateTime.Now.Add(duration);
		BountiedPlayers.Add(new Bounty(player, true, bountyPrice, expirationTime));
			
		if (hunted != null) AudioPlayer.PlayTo(player, hunted.AudioFile, hunted.Volume, hunted.Loop, true);
		foreach (Player ply in Player.List.Where(p => !p.IsNPC && p != player)) if (hunter != null) AudioPlayer.PlayTo(ply, hunter.AudioFile, hunter.Volume, hunter.Loop, true);

		Timing.CallDelayed((float)duration.TotalSeconds, () => StopBounty(player));
	}

	public static void StopBounty(Player player)
	{
		Bounty bounty = BountiedPlayers.FirstOrDefault(b => b.Player == player);
		if (bounty != null) BountiedPlayers.Remove(bounty);
	}

	public static void UpdateBountyMessages(Player ply, ref StringBuilder hintMessage)
	{
		Bounty bounty = BountiedPlayers.FirstOrDefault(b => b.Player == ply);
		if (bounty == null) return;
		TimeSpan timeLeft = bounty.ExpirationTime - DateTime.Now;
		ZoneType zone = bounty.Player.Zone;
		string bountyMessage = ply.UserId == bounty.Player.UserId
			? FoundationFortune.Instance.Translation.SelfBounty.Replace("%duration%", timeLeft.ToString(@"mm\:ss"))
			: FoundationFortune.Instance.Translation.OtherBounty.Replace("%player%", bounty.Player.Nickname)
				.Replace("%duration%", timeLeft.ToString(@"mm\:ss"))
				.Replace("%price%", bounty.Value.ToString())
				.Replace("%zone%", zone.ToString());
            
		hintMessage.Append($"{bountyMessage}");
	}
}