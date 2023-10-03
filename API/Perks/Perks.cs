using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using FoundationFortune.Configs;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.Models.Classes;
using System.Linq;
using System;

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
                    Scp330Bag.AddSimpleRegeneration(ply.ReferenceHub, 4f, 75f);
                    break;
                case PerkType.MovementBoost:
                    ply.EnableEffect<MovementBoost>(0);
                    ply.ChangeEffectIntensity<MovementBoost>(25);
                    break;
            }
        }

        public bool GrantRevivalPerk(Player reviver, string targetName)
        {
            Player targetToRevive = Player.Get(targetName);

            if (targetToRevive == null) return false;

            if (targetToRevive.IsDead)
            {
                RevivePlayer(reviver, targetToRevive);
                return true;
            }
            else
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(reviver, FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer.Replace("%targetName%", targetName), 0, 3, false, false);
                return false;
            }
        }

        private void RevivePlayer(Player reviver, Player targetToRevive)
        {
            Npc buyingbot = FoundationFortune.Singleton.serverEvents.GetBuyingBotNearPlayer(reviver);
            VoiceChatSettings revivalVoiceChatSettings = FoundationFortune.Singleton.Config.VoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == VoiceChatUsageType.Revival);
            BuyingBot.PlayAudio(buyingbot, revivalVoiceChatSettings.AudioFile, revivalVoiceChatSettings.Volume, revivalVoiceChatSettings.Loop, revivalVoiceChatSettings.VoiceChat);

            var Config = FoundationFortune.Singleton.Config;

            if (Config.ResetRevivedInventory) targetToRevive.Role.Set(reviver.Role, RoleSpawnFlags.None);
            else targetToRevive.Role.Set(reviver.Role);

            targetToRevive.Health = Config.RevivedPlayerHealth;
            targetToRevive.Teleport(reviver.Position);

            if (Config.HuntReviver)
                FoundationFortune.Singleton.serverEvents.AddBounty(reviver, Config.RevivalBountyKillReward, TimeSpan.FromSeconds(Config.RevivalBountyTimeSeconds));

            foreach (var ply in Player.List.Where(p => !p.IsNPC))
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, FoundationFortune.Singleton.Translation.RevivalSuccess.Replace("%rolecolor%", reviver.Role.Color.ToHex()).Replace("%nickname%", reviver.Nickname).Replace("%target%", targetToRevive.Nickname), 0, 3, false, false);
            }
        }
    }
}
