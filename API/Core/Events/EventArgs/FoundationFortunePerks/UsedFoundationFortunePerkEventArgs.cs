using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Core.Models.Interfaces.Perks;

namespace FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks
{
    public class UsedFoundationFortunePerkEventArgs : IExiledEvent
    {
        public UsedFoundationFortunePerkEventArgs(Player player, IPerk perk, Item item)
        {
            Player = player;
            Perk = perk;
            Item = item;
        }
    
        public IPerk Perk { get; }
        public Item Item { get; }
        public Player Player { get; }
    }
}
