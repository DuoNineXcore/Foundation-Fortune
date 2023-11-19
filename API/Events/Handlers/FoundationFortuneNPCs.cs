﻿using Exiled.Events.Features;
using FoundationFortune.API.Events.EventArgs;

namespace FoundationFortune.API.Events.Handlers
{
    public class FoundationFortuneNPCs
    {
        /// <summary>
        /// Event invoked after using a Foundation Fortune NPC.
        /// </summary>
        public static Event<UsedFoundationFortuneNPCEventArgs> UsedFoundationFortuneNPC { get; set; } = new();
        public static void OnUsedFoundationFortuneNPC(UsedFoundationFortuneNPCEventArgs ev) => UsedFoundationFortuneNPC.InvokeSafely(ev);

        /// <summary>
        /// Event invoked after buying a perk item with a Buying Bot NPC.
        /// </summary>
        public static Event<BoughtPerkEventArgs> BoughtPerk { get; set; } = new();
        public static void OnBoughtPerk(BoughtPerkEventArgs ev) => BoughtPerk.InvokeSafely(ev);

        /// <summary>
        /// Event invoked after buying a main game item with a Buying Bot NPC.
        /// </summary>
        public static Event<BoughtItemEventArgs> BoughtItem { get; set; } = new();
        public static void OnBoughtItem(BoughtItemEventArgs ev) => BoughtItem.InvokeSafely(ev);

        /// <summary>
        /// Event invoked after selling an item with a Selling Bot NPC.
        /// </summary>
        public static Event<SoldItemEventArgs> SoldItem { get; set; } = new();
        public static void OnSoldItem(SoldItemEventArgs ev) => SoldItem.InvokeSafely(ev);
    }
}