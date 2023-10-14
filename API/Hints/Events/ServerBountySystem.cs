using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using System.Text;

namespace FoundationFortune.API.HintSystem
{
	public partial class ServerEvents
	{
		public List<Bounty> BountiedPlayers { get; } = new List<Bounty>();

		public void AddBounty(Player player, int bountyPrice, TimeSpan duration)
		{
			DateTime expirationTime = DateTime.Now.Add(duration);
			BountiedPlayers.Add(new Bounty(player, true, bountyPrice, expirationTime));

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

            if (bounty != null)
            {
                TimeSpan timeLeft = bounty.ExpirationTime - DateTime.Now;
                string bountyMessage = ply.UserId == bounty.Player.UserId
                    ? FoundationFortune.Singleton.Translation.SelfBounty.Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"))
                    : FoundationFortune.Singleton.Translation.OtherBounty.Replace("%player%", bounty.Player.Nickname).Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"));
                hintMessage.Append($"{bountyMessage}");
            }
        }
    }
}
