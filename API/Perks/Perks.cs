using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

namespace FoundationFortune.API.Perks
{
    public class Perks
    {
        public void GrantPerk(Player ply, PerkType perk)
        {
            switch (perk)
            {
                case PerkType.ExtraHP:
                    ply.Heal(50, true);
                    ply.MaxHealth += 50;
                    break;
                case PerkType.AHPBoost:
                    ply.AddAhp(50, decay: 0);
                    break;
                case PerkType.Invisibility:
                    ply.EnableEffect<Invisible>(30);
                    break;
                case PerkType.Regeneration:
                    ply.EnableEffect<Vitality>(30);
                    break;
                case PerkType.MovementBoost:
                    ply.EnableEffect<MovementBoost>(150);
                    ply.ChangeEffectIntensity<MovementBoost>(30);
                    break;
            }
        }

        public bool GrantRevivalPerk(Player reviver, string targetName)
        {
            Player targetToRevive = Player.Get(targetName);
            Npc buyingbot = FoundationFortune.Singleton.serverEvents.GetBuyingBotNearPlayer(reviver);

            if (targetToRevive != null && targetToRevive.Role == RoleTypeId.Spectator)
            {
                BuyingBot.PlayAudio(buyingbot, "BuySuccess", 50, false, VoiceChat.VoiceChatChannel.Intercom);
                RevivePlayer(reviver, targetToRevive);
                return true;
            }
            else
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(reviver, $"<b><size=24>No dead player with Name: '{targetName}' found nearby to revive.</b></size>", 0, 3, false, false);
                return false;
            }
        }

        private void RevivePlayer(Player reviver, Player targetToRevive)
        {
            targetToRevive.Role.Set(reviver.Role);
            targetToRevive.Heal(50);
            targetToRevive.Teleport(reviver.Position);

            foreach (var ply in Player.List)
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"<color={reviver.Role.Color.ToHex()}>{reviver.Nickname}</color> Has Revived {targetToRevive.Nickname}", 0, 3, false, false);
            }
        }
    }
}
