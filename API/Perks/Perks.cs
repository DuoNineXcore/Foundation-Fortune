using CustomPlayerEffects;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.API.Perks
{
    public class Perks
    {
        public void GrantPerk(Player ply, PerkType perk)
        {
            switch (perk)
            {
                case PerkType.Revival:
                    break;
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
    }
}
