using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using System.Text;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		public List<Bounty> BountiedPlayers { get; } = new List<Bounty>();

		public void AddBounty(Player player, int bountyPrice, TimeSpan duration)
		{
			PlayerVoiceChatSettings hunted = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
				.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunted);
			PlayerVoiceChatSettings hunter = FoundationFortune.Singleton.Config.PlayerVoiceChatSettings
				.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunter);
			
			DateTime expirationTime = DateTime.Now.Add(duration);
			BountiedPlayers.Add(new Bounty(player, true, bountyPrice, expirationTime));
			
			if (hunted != null) AudioPlayer.PlayTo(player, hunted.AudioFile, hunted.Volume, hunted.Loop, hunted.VoiceChat);
			foreach (Player ply in Player.List.Where(p => !p.IsNPC && p != player)) 
				if (hunter != null) AudioPlayer.PlayTo(ply, hunter.AudioFile, hunter.Volume, hunter.Loop, hunter.VoiceChat);

			Timing.CallDelayed((float)duration.TotalSeconds, () => StopBounty(player));
		}

		public void StopBounty(Player player)
		{
			Bounty bounty = BountiedPlayers.FirstOrDefault(b => b.Player == player);
			if (bounty != null) BountiedPlayers.Remove(bounty);
		}

        private void UpdateBountyMessages(Player ply, ref StringBuilder hintMessage)
        {
            Bounty bounty = BountiedPlayers.FirstOrDefault(b => b.Player == ply);

            if (bounty == null) return;
            TimeSpan timeLeft = bounty.ExpirationTime - DateTime.Now;
            string bountyMessage = ply.UserId == bounty.Player.UserId
	            ? FoundationFortune.Singleton.Translation.SelfBounty.Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"))
	            : FoundationFortune.Singleton.Translation.OtherBounty.Replace("%player%", bounty.Player.Nickname).Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"));
            hintMessage.Append($"{bountyMessage}");
        }
    }
}
