using System.Collections.Generic;
using System.Linq;
using CustomPlayerEffects;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using FoundationFortune.API.Core;
using FoundationFortune.API.Core.Models.Classes.Player;
using FoundationFortune.API.Core.Models.Enums.Player;
using FoundationFortune.API.Core.Models.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Models.Interfaces.Perks;
using FoundationFortune.API.Features.Systems;
using MEC;

namespace FoundationFortune.API.Features.Perks.Active
{
    public class EthericVitality : IActivePerk
    {
        public static readonly List<Player> EthericVitalityPlayers = new();
        private static readonly PlayerVoiceChatSettings EthericVitalitySfx = AudioPlayer.GetVoiceChatSettings(PlayerVoiceChatUsageType.EthericVitality);

        public void ApplyPassiveEffect(Player player)
        {
            if (!PerkSystem.ConsumedActivePerks.ContainsKey(player)) PerkSystem.ConsumedActivePerks.Add(player, new Dictionary<IActivePerk, int>());
            if (!PerkSystem.ConsumedActivePerks[player].ContainsKey(this)) PerkSystem.ConsumedActivePerks[player][this] = 1;
            else PerkSystem.ConsumedActivePerks[player][this]++;
            EthericVitalityPlayers.Add(player);
        }

        public CoroutineHandle StartActivePerkAbility(Player player) =>  Timing.RunCoroutine(EthericVitalityCoroutine(player));

        private static IEnumerator<float> EthericVitalityCoroutine(Player player)
        {
            const float healthRegenRate = 2f;
            const float healingInterval = 1f;

            AudioPlayer.PlaySpecialAudio(player, EthericVitalitySfx.AudioFile, EthericVitalitySfx.Volume, EthericVitalitySfx.Loop, EthericVitalitySfx.VoiceChat);
            while (PerkSystem.HasPerk(player, PerkType.EthericVitality) && player.Health > 0f)
            {
                player.Heal(healthRegenRate);
                player.EnableEffect<SpawnProtected>(1.5f);
                foreach (var otherPlayer in Player.List.Where(p => p != player && p.Role.Team == player.Role.Team && IsPlayerInHealingAura(p, player, 5f)))
                {
                    otherPlayer.Heal(healthRegenRate); 
                    otherPlayer.EnableEffect<SpawnProtected>(1.5f);   
                }
                yield return Timing.WaitForSeconds(healingInterval);
            }
        }

        private static bool IsPlayerInHealingAura(IPosition targetPlayer, IPosition sourcePlayer, float radius)
        {
            float distanceSqr = (targetPlayer.Position - sourcePlayer.Position).sqrMagnitude;
            return distanceSqr <= radius * radius;
        }

        public PerkType PerkType { get; } = PerkType.EthericVitality;
        public string Alias { get; } = "Etheric Vitality";
    }
}