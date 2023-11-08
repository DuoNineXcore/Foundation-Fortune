using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using System.Text;
using Exiled.API.Enums;
using FoundationFortune.API.Models;
using FoundationFortune.API.Models.Classes.Events;
using FoundationFortune.API.Models.Classes.Player;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Enums.Player;

// ReSharper disable once CheckNamespace
// STFU!!!!!!!!!!!!!!!!
namespace FoundationFortune.API
{
	public partial class FoundationFortuneAPI
	{
		public List<Bounty> BountiedPlayers { get; } = new List<Bounty>();

		public void AddBounty(Player player, int bountyPrice, TimeSpan duration)
		{
			PlayerVoiceChatSettings hunted = FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings
				.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunted);
			PlayerVoiceChatSettings hunter = FoundationFortune.VoiceChatSettings.PlayerVoiceChatSettings
				.FirstOrDefault(settings => settings.VoiceChatUsageType == PlayerVoiceChatUsageType.Hunter);
			
			DateTime expirationTime = DateTime.Now.Add(duration);
			BountiedPlayers.Add(new Bounty(player, true, bountyPrice, expirationTime));
			
			if (hunted != null) AudioPlayer.PlayTo(player, hunted.AudioFile, hunted.Volume, hunted.Loop, true);
			foreach (Player ply in Player.List.Where(p => !p.IsNPC && p != player)) 
				if (hunter != null) AudioPlayer.PlayTo(ply, hunter.AudioFile, hunter.Volume, hunter.Loop, true);

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
            ZoneType zone = bounty.Player.Zone;
            string bountyMessage = ply.UserId == bounty.Player.UserId
	            ? FoundationFortune.Singleton.Translation.SelfBounty.Replace("%duration%", timeLeft.ToString(@"mm\:ss"))
	            : FoundationFortune.Singleton.Translation.OtherBounty.Replace("%player%", bounty.Player.Nickname)
		            .Replace("%duration%", timeLeft.ToString(@"mm\:ss"))
		            .Replace("%price%", bounty.Value.ToString())
					.Replace("%zone%", zone.ToString());
            
            hintMessage.Append($"{bountyMessage}");
        }
    }
}
