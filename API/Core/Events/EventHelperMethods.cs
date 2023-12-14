using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Common.Enums.NPCs;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Common.Models.Items;
using FoundationFortune.API.Common.Models.NPCs;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneAudio;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneItems;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortuneNPCs;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Events.Handlers;
using FoundationFortune.API.Features.NPCs;

namespace FoundationFortune.API.Core.Events;

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
        UsedFoundationFortuneNpcEventArgs eventArgs = new(player, npc, npcType, outcome);
        FoundationFortuneNPCEvents.OnUsedFoundationFortuneNPC(eventArgs);
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
        FoundationFortunePerkEvents.OnUsedFoundationFortunePerk(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player buys a perk from a Buying Bot.
    /// </summary>
    /// <param name="player">The player who bought the perk.</param>
    /// <param name="perk">The bought perk.</param>
    public static void RegisterOnBoughtPerk(Player player, BuyablePerk perk)
    {
        Npc npc = NPCHelperMethods.GetNearestBuyingBot(player);
        BoughtPerkEventArgs eventArgs = new(player, npc, perk);
        FoundationFortuneItemEvents.OnBoughtPerk(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player buys an item from a Buying Bot.
    /// </summary>
    /// <param name="player">The player who bought the item.</param>
    /// <param name="item">The bought item.</param>
    public static void RegisterOnBoughtItem(Player player, BuyableItem item)
    {
        Npc npc = NPCHelperMethods.GetNearestBuyingBot(player);
        BoughtItemEventArgs eventArgs = new(player, npc, item);
        FoundationFortuneItemEvents.OnBoughtItem(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player sells an item to a Selling Bot.
    /// </summary>
    /// <param name="player">The player who sold the item.</param>
    /// <param name="sellableItem">The type of sellable item.</param>
    /// <param name="item">The sold item.</param>
    public static void RegisterOnSoldItem(Player player, SellableItem sellableItem, Item item)
    {
        Npc npc = NPCHelperMethods.GetNearestSellingBot(player);
        SoldItemEventArgs eventArgs = new(player, npc, sellableItem, item);
        FoundationFortuneItemEvents.OnSoldItem(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player has played an audio through the API.
    /// </summary>
    /// <param name="player">The player who played audio.</param>
    /// <param name="voiceChatSettings">The voice chat settings for the player.</param>
    public static void RegisterOnPlayerPlayedAudio(Player player, PlayerVoiceChatSettings voiceChatSettings)
    {
        PlayerPlayedAudioEventArgs eventArgs = new PlayerPlayedAudioEventArgs(player, voiceChatSettings);
        FoundationFortuneAudioEvents.OnPlayerPlayedAudio(eventArgs);
    }

    /// <summary>
    /// Registers the event when a player is currently playing an audio through the API.
    /// </summary>
    /// <param name="player">The player who is playing audio.</param>
    /// <param name="voiceChatSettings">The voice chat settings for the player.</param>
    public static void RegisterOnPlayerPlayingAudio(Player player, PlayerVoiceChatSettings voiceChatSettings)
    {
        PlayerPlayingAudioEventArgs eventArgs = new PlayerPlayingAudioEventArgs(player, voiceChatSettings);
        FoundationFortuneAudioEvents.OnPlayerPlayingAudio(eventArgs);
    }

    /// <summary>
    /// Registers the event when an NPC has played an audio through the API.
    /// </summary>
    /// <param name="pair">The pair of player and music bot associated with the event.</param>
    /// <param name="voiceChatSettings">The voice chat settings for the NPC.</param>
    public static void RegisterOnNPCPlayedAudio(PlayerMusicBotPair pair, NPCVoiceChatSettings voiceChatSettings)
    {
        NPCPlayedAudioEventArgs eventArgs = new NPCPlayedAudioEventArgs(pair, voiceChatSettings);
        FoundationFortuneAudioEvents.OnNPCPlayedAudio(eventArgs);
    }

    /// <summary>
    /// Registers the event when an NPC is currently playing an audio through the API.
    /// </summary>
    /// <param name="pair">The pair of player and music bot associated with the event.</param>
    /// <param name="voiceChatSettings">The voice chat settings for the NPC.</param>
    public static void RegisterOnNPCPlayingAudio(PlayerMusicBotPair pair, NPCVoiceChatSettings voiceChatSettings)
    {
        NPCPlayingAudioEventArgs eventArgs = new NPCPlayingAudioEventArgs(pair, voiceChatSettings);
        FoundationFortuneAudioEvents.OnNPCPlayingAudio(eventArgs);
    }
}