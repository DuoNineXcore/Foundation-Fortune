using FoundationFortune.API.Models.Enums.Perks;

namespace FoundationFortune.API.Models.Classes.Items
{
    public class PerkItem
    {
        public PerkType PerkType { get; set; }
        public int Price { get; set; }
        public int Limit { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}