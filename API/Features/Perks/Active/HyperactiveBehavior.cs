using System.Collections.Generic;
using Exiled.API.Enums;
using Exiled.API.Features;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using FoundationFortune.API.Features.Systems;
using MEC;

namespace FoundationFortune.API.Features.Perks.Active
{
    public class HyperactiveBehavior : IActivePerk
    {
        private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOn = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOn);
        private static readonly PlayerVoiceChatSettings HyperactiveBehaviorOff = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.HyperactiveBehaviorOff);
        public static readonly List<Player> HyperactiveBehaviorPlayers = new();

        public void ApplyPassiveEffect(Player player)
        {
            if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
            HyperactiveBehaviorPlayers.Add(player);
            if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
            else PerkSystem.ConsumedActivePerks[player][this]++;
        }

        public CoroutineHandle StartActivePerkAbility(Player player) => Timing.RunCoroutine(HyperactiveBehaviorCoroutine(player));
    
        private static IEnumerator<float> HyperactiveBehaviorCoroutine(Player player)
        {
            while (PerkSystem.HasPerk(player, PerkType.HyperactiveBehavior))
            {
                player.EnableEffect(EffectType.MovementBoost);
                var randomizedStamina = UnityEngine.Random.Range(150, 301);
                var randomizedMovementSpeed = UnityEngine.Random.Range(20, 51);

                player.Stamina = randomizedStamina;
                player.ChangeEffectIntensity(EffectType.MovementBoost, (byte)randomizedMovementSpeed);
                if (HyperactiveBehaviorOn != null) AudioPlayer.PlayTo(player, HyperactiveBehaviorOn.AudioFile, HyperactiveBehaviorOn.Volume, HyperactiveBehaviorOn.Loop, false);
                FoundationFortune.Instance.HintSystem.BroadcastHint(player, $"<b>+{randomizedMovementSpeed} Movement Speed, +{randomizedStamina} Stamina</b>");
                yield return Timing.WaitForSeconds(UnityEngine.Random.Range(10f, 15f));

                player.Stamina = 100;
                player.ChangeEffectIntensity(EffectType.MovementBoost, 0);
                if (HyperactiveBehaviorOff != null) AudioPlayer.PlayTo(player, HyperactiveBehaviorOff.AudioFile, HyperactiveBehaviorOff.Volume, HyperactiveBehaviorOff.Loop, false);
                yield return Timing.WaitForSeconds(5f);
            }
        }

        public PerkType PerkType { get; } = PerkType.HyperactiveBehavior;
        public string Alias { get; } = "Hyperactive Behavior";
    }
}