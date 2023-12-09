using System.Collections.Generic;
using Exiled.API.Enums;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces;
using FoundationFortune.API.Core.Models.Interfaces.Configs;
using YamlDotNet.Serialization;

namespace FoundationFortune.Configs
{
    public class PerkSystemSettings : IFoundationFortuneConfig
    {
        [YamlIgnore] public string PropertyName { get; set; } = "Perk System Settings";

        public float ViolentImpulsesDamageMultiplier { get; set; } = 1.2f;
        public float ViolentImpulsesRecoilAnimationTime { get; set; } = 3f;
        public float ViolentImpulsesRecoilZAxis { get; set; } = 3f;
        public float ViolentImpulsesRecoilUpKick { get; set; } = 3f;
        public float ViolentImpulsesRecoilFovKick { get; set; } = 3f;
        public float ViolentImpulsesRecoilSideKick { get; set; } = 3f;
        public bool HuntReviver { get; set; } = true;
        public int RevivedPlayerHealth { get; set; } = 30;
        public bool ResetRevivedInventory { get; set; } = false;
        public int RevivalBountyKillReward { get; set; } = 5000;
        public int RevivalBountyTimeSeconds { get; set; } = 300;

        public List<RoomType> ForbiddenEtherealInterventionRoomTypes { get; set; } = new()
        {
            RoomType.EzCollapsedTunnel,
            RoomType.HczTestRoom,
            RoomType.Hcz049,
            RoomType.Lcz173,
            RoomType.HczTesla,
            RoomType.HczHid,
            RoomType.Lcz330
        };
    
        public Dictionary<PerkType, string> PerkCounterEmojis { get; set; } = new Dictionary<PerkType, string>
        {
            { PerkType.ViolentImpulses, "🔪" }, 
            { PerkType.EthericVitality, "❤️" },
            { PerkType.HyperactiveBehavior, "🏃" },
            { PerkType.BlissfulUnawareness, "💞" },
            { PerkType.ExtrasensoryPerception, "◎" },
            { PerkType.EtherealIntervention, "✚" } 
        };
    }
}