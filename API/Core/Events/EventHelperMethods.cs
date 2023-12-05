using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Core.Events.EventArgs;
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Core.Models.Classes.Items;
using FoundationFortune.API.Core.Models.Enums.NPCs;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using FoundationFortune.API.Features.NPCs;

namespace FoundationFortune.API.Core.Events
{
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
            Npc npc = npcType == NpcType.Buying ? NpcHelperMethods.GetNearestBuyingBot(player) : NpcHelperMethods.GetNearestSellingBot(player);
            UsedFoundationFortuneNpcEventArgs eventArgs = new(player, npc, npcType, outcome);
            FoundationFortuneNPCs.OnUsedFoundationFortuneNPC(eventArgs);
        }

        /// <summary>
        /// Registers the event when a player uses a Foundation Fortune Perk.
        /// </summary>
        /// <param name="player">The player who used the perk.</param>
        /// <param name="perk">The perk used in its interface form.</param>
        /// <param name="item">The item associated with the perk.</param>
        public static void RegisterOnUsedFoundationFortunePerk(Player player, IPerk perk, Item item)
        {
            UsedFoundationFortunePerkEventArgs eventArgs = new(player, perk, item);
            FoundationFortunePerks.OnUsedFoundationFortunePerk(eventArgs);
        }

        /// <summary>
        /// Registers the event when a player buys a perk from a Buying Bot.
        /// </summary>
        /// <param name="player">The player who bought the perk.</param>
        /// <param name="perk">The bought perk.</param>
        public static void RegisterOnBoughtPerk(Player player, BuyablePerk perk)
        {
            Npc npc = NpcHelperMethods.GetNearestBuyingBot(player);
            BoughtPerkEventArgs eventArgs = new(player, npc, perk);
            FoundationFortuneNPCs.OnBoughtPerk(eventArgs);
        }

        /// <summary>
        /// Registers the event when a player buys an item from a Buying Bot.
        /// </summary>
        /// <param name="player">The player who bought the item.</param>
        /// <param name="item">The bought item.</param>
        public static void RegisterOnBoughtItem(Player player, BuyableItem item)
        {
            Npc npc = NpcHelperMethods.GetNearestBuyingBot(player);
            BoughtItemEventArgs eventArgs = new(player, npc, item);
            FoundationFortuneNPCs.OnBoughtItem(eventArgs);
        }

        /// <summary>
        /// Registers the event when a player sells an item to a Selling Bot.
        /// </summary>
        /// <param name="player">The player who sold the item.</param>
        /// <param name="sellableItem">The type of sellable item.</param>
        /// <param name="item">The sold item.</param>
        public static void RegisterOnSoldItem(Player player, SellableItem sellableItem, Item item)
        {
            Npc npc = NpcHelperMethods.GetNearestSellingBot(player);
            SoldItemEventArgs eventArgs = new(player, npc, sellableItem, item);
            FoundationFortuneNPCs.OnSoldItem(eventArgs);
        }
    }
}