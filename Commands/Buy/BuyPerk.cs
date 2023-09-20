using CommandSystem;
using CustomPlayerEffects;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using InventorySystem;
using System;
using System.Linq;

namespace FoundationFortune.Commands.Buy
{
     internal class BuyPerk : ICommand
     {
          public string Command { get; } = "Perk";
          public string Description { get; } = "Buy a perk!";
          public string[] Aliases { get; } = new string[] { "p" };

          public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
          {
               Player player = Player.Get(sender);

               if (FoundationFortune.Singleton.serverEvents.IsPlayerOnSellingWorkstation(player) || FoundationFortune.Singleton.serverEvents.IsPlayerOnBuyingBotRadius(player))
               {
                    if (arguments.Count < 1)
                    {
                         response = "You must specify an item to buy!";
                         return false;
                    }
                    if (!Enum.TryParse(arguments.At(0), ignoreCase: true, out PerkType perkType))
                    {
                         response = "You must specify a valid item!";
                         return false;
                    }
                    PerkItem perk = FoundationFortune.Singleton.Config.PerkItems.Where(p => p.PerkType == perkType).FirstOrDefault();
                    if (perk == null)
                    {
                         response = "That is not a purchaseable perk!";
                         return false;
                    }
                    int money = PlayerDataRepository.GetMoneySaved(player.UserId);
                    if (money < perk.Price)
                    {
                         response = $"You are missing ${perk.Price - money}!";
                         return false;
                    }
                    PlayerDataRepository.SubtractMoneySaved(player.UserId, perk.Price);
                    GrantPerk(player, perkType);
                    response = $"You have successfully bought a {perk.DisplayName} for ${perk.Price}";
                    return true;
               }
               else
               {
                    response = "You must be at a buying station to buy a perk!";
                    return false;
               }
          }


          private void GrantPerk(Player player, PerkType perk)
          {
               switch(perk)
               {
                    case PerkType.Revival:
                         break;

                    case PerkType.ExtraHP:
                         player.MaxHealth += 50;
                         break;

                    case PerkType.AHPBoost:
                         player.AddAhp(50, decay: 0);
                         break;

                    case PerkType.Invisibility:
                         player.EnableEffect<Invisible>(0);
                         break;

                    case PerkType.Regeneration:
                         player.EnableEffect<Vitality>(30);
                         break;

                    case PerkType.MovementBoost:
                         player.EnableEffect<MovementBoost>(150);
                         player.ChangeEffectIntensity<MovementBoost>(30);
                         break;

                    default: 
                         Log.Warn($"{perk} has not been implemented!");
                         break;
               }
          }
     }
}
