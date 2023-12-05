using System.Collections.Generic;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using PlayerStatsSystem;

namespace FoundationFortune.API.Features.Perks.Passive
{
    public class ViolentImpulses : IPassivePerk
    {
        public static readonly List<Player> ViolentImpulsesPlayers = new();
        private static readonly PlayerVoiceChatSettings ViolentImpulsesSfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.ViolentImpulses);

        public void ApplyPassiveEffect(Player player)
        {
            ViolentImpulsesPlayers.Add(player);
            player.ReferenceHub.playerStats.GetModule<StaminaStat>().ModifyAmount(0.1f);
            AudioPlayer.PlayTo(player, ViolentImpulsesSfx.AudioFile, ViolentImpulsesSfx.Volume, ViolentImpulsesSfx.Loop, true);
        }

        public PerkType PerkType { get; } = PerkType.ViolentImpulses;
    }
}