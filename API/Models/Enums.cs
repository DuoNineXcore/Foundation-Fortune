namespace FoundationFortune.API.Models
{
    public enum HintAlign
    {
        Center, Right, Left,
    }

    public enum BotType
    {
        Buying, Selling, Music,
    }

    public enum HintAnim
    {
        Center, Right, Left, None,
    }

    public enum PlayerTeamConditions
    {
        Winning, Losing, Draw
    }

    public enum PlayerVoiceChatUsageType
    {
        Hunted, Hunter, BlissfulUnawareness, ResurgenceBeacon, EtherealIntervention,
    }

    public enum NPCVoiceChatUsageType
    {
        Selling, Buying, NotEnoughMoney, WrongBuyingBot, BuyingBotInRange, SellingBotInRange,
    }

    public enum PerkType
    {
        EthericVitality, HyperactiveBehavior, ViolentImpulses, BlissfulUnawareness, ExtrasensoryPerception, ResurgenceBeacon, EtherealIntervention,
    }
}
