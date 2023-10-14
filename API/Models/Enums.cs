namespace FoundationFortune.API.Models.Enums
{
    public enum HintAlign
    {
        Center, Right, Left    
    }

    public enum HintAnim
    {
        Center, Right, Left, None
    }

    public enum NPCVoiceChatUsageType
    { 
        Selling, Buying, NotEnoughMoney, WrongBuyingBot, BuyingBotInRange, SellingBotInRange
    }

    public enum PlayerVoiceChatUsageType
    {
        Hunted, Hunter, BlissfulUnawareness, ResurgenceBeacon, EtherealIntervention,
    }

    public enum PerkType
    {
        OvershieldedProtection,
        BoostedResilience,
        ConcealedPresence,
        EthericVitality,
        Hyperactivity,
        BlissfulUnawareness,
        ExtrasensoryPerception,
        ResurgenceBeacon,
        EtherealIntervention
    }
}
