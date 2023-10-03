using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using FoundationFortune.API.Models.Classes;

namespace FoundationFortune.Events
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
    }
}
