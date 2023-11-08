using Exiled.API.Features;
using Exiled.API.Features.Items;
using FoundationFortune.API.Events.EventArgs;
using FoundationFortune.API.Models.Enums.NPCs;
using FoundationFortune.API.Models.Enums.Perks;
using FoundationFortune.API.NPCs;

namespace FoundationFortune.API.Events;

public static class EventHelperMethods
{
    public static void RegisterOnUsedFoundationFortuneNPC(Player player, NpcType npcType, NpcUsageOutcome outcome)
    {
        Npc npc = npcType == NpcType.Buying ? NPCHelperMethods.GetNearestBuyingBot(player) : NPCHelperMethods.GetNearestSellingBot(player);
        UsedFoundationFortuneNPCEventArgs eventArgs = new(player, npc, npcType, outcome);
        Handlers.FoundationFortuneNPC.OnUsedFoundationFortuneNPC(eventArgs);
    }
    
    public static void RegisterOnUsedFoundationFortunePerk(Player player, PerkType perkType, Item item)
    {
        UsedFoundationFortunePerkEventArgs eventArgs = new(player, perkType, item);
        Handlers.FoundationFortunePerks.OnUsedFoundationFortunePerk(eventArgs);
    }
}