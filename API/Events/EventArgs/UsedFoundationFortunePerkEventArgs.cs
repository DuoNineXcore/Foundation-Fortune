using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Interfaces;
using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune.API.Events.EventArgs
{
    public class UsedFoundationFortunePerkEventArgs : IExiledEvent
    {
        public UsedFoundationFortunePerkEventArgs(Player player, PerkType perkType, Item item)
        {
            Player = player;
            PerkType = perkType;
            Item = item;
        }
    
        public PerkType PerkType { get; }
        public Item Item { get; }
        public Player Player { get; }
    }
}
