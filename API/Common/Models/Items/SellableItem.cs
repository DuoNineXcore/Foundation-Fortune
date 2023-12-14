namespace FoundationFortune.API.Common.Models.Items;

public class SellableItem
{
    public ItemType ItemType { get; set; }
    public int Price { get; set; }
    public int Limit { get; set; }
    public string Alias { get; set; }
    public string DisplayName { get; set; }
}