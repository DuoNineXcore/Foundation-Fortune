using Exiled.API.Features;
using FoundationFortune.API.Common.Enums.Player;
using FoundationFortune.API.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Common.Interfaces.Perks;
using FoundationFortune.API.Common.Models.Player;
using FoundationFortune.API.Core.Systems;
using PlayerStatsSystem;

namespace FoundationFortune.API.Features.Perks.Passive;

public class ViolentImpulses : IPassivePerk
{
    private static readonly PlayerVoiceChatSettings ViolentImpulsesSfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.ViolentImpulses);

    public void ApplyPassiveEffect(Player player)
    {
        PerkSystem.PerkPlayers[PerkType.ViolentImpulses].Add(player);
        player.ReferenceHub.playerStats.GetModule<StaminaStat>().ModifyAmount(0.1f);
        AudioPlayer.PlayTo(player, ViolentImpulsesSfx.AudioFile, ViolentImpulsesSfx.Volume, ViolentImpulsesSfx.Loop, true);
    }

    public PerkType PerkType { get; } = PerkType.ViolentImpulses;
    public string Alias { get; } = "Violent Impulses";
}