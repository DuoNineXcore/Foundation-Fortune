using FoundationFortune.API.Common.Enums.Systems.PerkSystem;

namespace FoundationFortune.API.Common.Models.Items;

public class BuyablePerk
{
    public PerkType PerkType { get; set; }
    public int Price { get; set; }
    public int Limit { get; set; }
    public string Alias { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
}