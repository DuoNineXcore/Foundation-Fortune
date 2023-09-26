using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.NPCs;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using FoundationFortune.Configs;
using System.Linq;

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
            Npc buyingbot = FoundationFortune.Singleton.serverEvents.GetBuyingBotNearPlayer(reviver);
            VoiceChatSettings revivalVoiceChatSettings = FoundationFortune.Singleton.Config.VoiceChatSettings.FirstOrDefault(settings => settings.VoiceChatUsageType == VoiceChatUsageType.Revival);

            if (revivalVoiceChatSettings != null)
            {
                BuyingBot.PlayAudio(buyingbot, revivalVoiceChatSettings.AudioFile, revivalVoiceChatSettings.Volume, revivalVoiceChatSettings.Loop, revivalVoiceChatSettings.VoiceChat);
                RevivePlayer(reviver, targetToRevive);
                return true;
            }
            else
            {
                FoundationFortune.Singleton.serverEvents.EnqueueHint(reviver, $"{FoundationFortune.Singleton.Translation.RevivalNoDeadPlayer}", 0, 3, false, false);
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
                FoundationFortune.Singleton.serverEvents.EnqueueHint(ply, $"{FoundationFortune.Singleton.Translation.RevivalSuccess}", 0, 3, false, false);
            }
        }
    }
}
