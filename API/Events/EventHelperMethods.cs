using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Events.EventArgs;
using FoundationFortune.API.Models.Classes.Items;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.NPCs;

namespace FoundationFortune.API.Events;

public static class EventHelperMethods
{
    /// <summary>
    /// Registers the event when a player uses a Foundation Fortune NPC.
    /// </summary>
    /// <param name="player">The player who used the NPC.</param>
    /// <param name="npcType">The type of NPC (Buying or Selling).</param>
    /// <param name="outcome">The outcome after the player used the NPC.</param>
    public static void RegisterOnUsedFoundationFortuneNPC(Player player, NpcType npcType, NpcUsageOutcome outcome)
    {
        Npc npc = npcType == NpcType.Buying ? NPCHelperMethods.GetNearestBuyingBot(player) : NPCHelperMethods.GetNearestSellingBot(player);
        UsedFoundationFortuneNPCEventArgs eventArgs = new(player, npc, npcType, outcome);
        Handlers.FoundationFortuneNPCs.OnUsedFoundationFortuneNPC(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player uses a Foundation Fortune Perk.
    /// </summary>
    /// <param name="player">The player who used the perk.</param>
    /// <param name="perkType">The type of perk used.</param>
    /// <param name="item">The item associated with the perk.</param>
    public static void RegisterOnUsedFoundationFortunePerk(Player player, PerkType perkType, Item item)
    {
        UsedFoundationFortunePerkEventArgs eventArgs = new(player, perkType, item);
        Handlers.FoundationFortunePerks.OnUsedFoundationFortunePerk(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player buys a perk from a buying bot.
    /// </summary>
    /// <param name="player">The player who bought the perk.</param>
    /// <param name="perk">The bought perk.</param>
    public static void RegisterOnBoughtPerk(Player player, BuyablePerk perk)
    {
        Npc npc = NPCHelperMethods.GetNearestBuyingBot(player);
        BoughtPerkEventArgs eventArgs = new(player, npc, perk);
        Handlers.FoundationFortuneNPCs.OnBoughtPerk(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player buys an item from a buying bot.
    /// </summary>
    /// <param name="player">The player who bought the item.</param>
    /// <param name="item">The bought item.</param>
    public static void RegisterOnBoughtItem(Player player, BuyableItem item)
    {
        Npc npc = NPCHelperMethods.GetNearestBuyingBot(player);
        BoughtItemEventArgs eventArgs = new(player, npc, item);
        Handlers.FoundationFortuneNPCs.OnBoughtItem(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player sells an item to a selling bot.
    /// </summary>
    /// <param name="player">The player who sold the item.</param>
    /// <param name="sellableItem">The type of sellable item.</param>
    /// <param name="item">The sold item.</param>
    public static void RegisterOnSoldItem(Player player, SellableItem sellableItem, Item item)
    {
        Npc npc = NPCHelperMethods.GetNearestSellingBot(player);
        SoldItemEventArgs eventArgs = new(player, npc, sellableItem, item);
        Handlers.FoundationFortuneNPCs.OnSoldItem(eventArgs);
    }
}