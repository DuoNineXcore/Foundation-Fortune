using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using FoundationFortune.API.Core.Common.Abstract.Perks;
using FoundationFortune.API.Core.Common.Enums.Systems.PerkSystem;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Events.EventArgs.FoundationFortunePerks;
using FoundationFortune.API.Core.Systems;
using InventorySystem.Items;

namespace FoundationFortune.API.Features.Perks.Passive;

public class TouchOfMidas : PassivePerkBase
{
    public override PerkType PerkType => PerkType.TouchOfMidas;
    public override string Alias => "Touch of Midas";
    public override void ApplyPassiveEffect(Player player) => PerkSystem.PerkPlayers[this.PerkType].Add(player);

    public override void SubscribeEvents()
    {
        base.SubscribeEvents();
        Exiled.Events.Handlers.Player.Hurting += Hurting;
        Exiled.Events.Handlers.Player.PickingUpItem += PickingUpitem;
    }

    public override void UnsubscribeEvents()
    {
        base.UnsubscribeEvents();
        Exiled.Events.Handlers.Player.Hurting -= Hurting;
        Exiled.Events.Handlers.Player.PickingUpItem -= PickingUpitem;
    }

    protected override void UsedFoundationFortunePerk(UsedFoundationFortunePerkEventArgs ev)
    {
        if (ev.Player.Inventory.UserInventory.Items.Count > 4 && ev.Perk.PerkType == this.PerkType)
        {
            int itemsToDropCount = ev.Player.Inventory.UserInventory.Items.Count - 4;

            for (ushort i = 0; i < itemsToDropCount; i++)
            {
                ItemBase itemToDrop = ev.Player.Inventory.UserInventory.Items[i];
                ev.Player.Inventory.CmdDropItem(itemToDrop.ItemSerial, false);
            }
        }
        
        base.UsedFoundationFortunePerk(ev);
    }

    private void PickingUpitem(PickingUpItemEventArgs ev)
    {
        if (!PerkSystem.HasPerk(ev.Player, this.PerkType) || ev.Player.Inventory.UserInventory.Items.Count > 4) return;
        ev.IsAllowed = false;
        FoundationFortune.Instance.HintSystem.BroadcastHint(ev.Player, "<b>You have reached your item limit [Touch of Midas]</b>");
    }

    private void Hurting(HurtingEventArgs ev)
    {
        if (PerkSystem.HasPerk(ev.Attacker, this.PerkType)) return;
        if (ev.Player != ev.Attacker) ev.Amount *= 0.5f;
        PlayerStatsRepository.ModifyMoney(ev.Player.UserId, (int)ev.DamageHandler.Damage);
    }
}