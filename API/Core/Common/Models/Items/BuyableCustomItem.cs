using FoundationFortune.API.Core.Common.Enums;

namespace FoundationFortune.API.Core.Common.Models.Items;

public class BuyableCustomItem
{
    public ItemType ItemType { get; set; }
    public CustomItemType CustomItemType { get; set; }
    public int Price { get; set; }
    public int Limit { get; set; }
    public string Description { get; set; }
    public string Alias { get; set; }
    public string DisplayName { get; set; }
}