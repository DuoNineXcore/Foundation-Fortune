using Exiled.API.Enums;
using Exiled.API.Interfaces;
using PlayerRoles;
using System.Collections.Generic;
using UnityEngine;
using VoiceChat;
using System.ComponentModel;

namespace FoundationFortune.Configs
{
	public class PluginTranslations : ITranslation
	{
		[Description("Server Events")]
		public string Kill { get; set; } = "\\n<b><color=green>+$300</color> %victim%'s Termination.</b>";
		public string Escape { get; set; } = "\\n<b><color=green>+$300</color> Successfully Escaped.</b>";
        public string Death { get; set; } = "\\n<b><color=red>$-%moneyBeforeDeath%.</color> You died.</b>";

        [Description("Bot Proximity Hints")]
		public string SellingWorkstation { get; set; } = "\\n<b>You're on a Selling Workstation.</b>";
		public string BuyingBot { get; set; } = "\\n<b>You're around a buying bot. Type .buy list in the console.</b>";
		public string SellingBot { get; set; } = "\\n<b>You're around a Selling bot. Drop Items twice to sell them.</b>";

		[Description("Hint System Counters")]
		public string MoneyCounterSaved { get; set; } = "\\n<b>Money Saved: <color=%rolecolor%>$%moneySaved%</color></b>";
        public string MoneyCounterOnHold { get; set; } = "\\n\\n<b>Money On Hold: <color=%rolecolor%>$%moneyOnHold%</color></b>";

        [Description("Hint System Events")]
		public string WrongBot { get; set; } = "\\n<b><color=red>This is not a selling bot.</color></b>";
		public string SaleCancelled { get; set; } = "\\n<b><color=red>Item changed. Sale canceled.</color></b>";
		public string SellSuccess { get; set; } = "\\n<b><color=green>+%price%$</color> Sold %itemName%.</color></b>";
		public string ItemConfirmation { get; set; } = "\\n<b>This item is worth <color=green>%price%</color>, Confirm sale? (%time% seconds left)</b></align>";
		public string BuyItemSuccess { get; set; } = "\\n<b><color=red>-$%itemPrice%</color> Bought %itemName%.</b>";

		[Description("Extraction Event")]
        public string ExtractionEvent = "\\n<b>A <color=green>Money</color> Extraction Zone has opened up. \\nRoom: %room% Time Left: %time%</b>";
		public string ExtractionTimer = "\\n<b>You're in the extraction zone, extracting money in %time% seconds.</b>";
        public string ExtractionStart = "\\n<b>Extracting Money.</b>";
		public string ExtractionNoMoney = "\\n<b>You do not have any On Hold Money.</b>";

        [Description("Death Coins")]
		public string DeathCoinPickup { get; set; } = "\\n<b><color=green>+%coinValue%</color> Picked up Death Coin.</b>";

		[Description("Revival System Hints")]
		public string RevivalNoDeadPlayer { get; set; } = "\\n<b>No dead player with Name: '%targetName%' found nearby to revive.</b>";
		public string RevivalSuccess { get; set; } = "\\n<b><color=%rolecolor%>%nickname%</color> Has Revived <color=%rolecolor%>%target%</color></b>";

        [Description("Bounty System Hints")]
        public string SelfBounty { get; set; } = "\\n<b><color=red>You're Being Hunted!</color>\\nReward: $3000 Duration: %duration%</b>";
        public string OtherBounty { get; set; } = "\\n<b><color=red>%player% is being Hunted!</color></b>\\nReward: $3000 Duration: %duration%";
        public string BountyFinished { get; set; } = "\\n<b><color=%victimrolecolor%>%victim%</color> has been killed by <color=%attackercolor%>%attacker%</color>, Bounty Finished.</b>";
		public string BountyPlayerDied { get; set; } = "\\n<b>%victim% Died from unknown reasons, Bounty Finished.</b>";
        public string BountyKill { get; set; } = "\\n<b><color=green>+%bountyPrice%$</color> You killed %victim%. Bounty Finished.</b>";

        [Description("Database Operation Hints")]
		public string FlushedDatabase { get; set; } = "\\n<b><color=red>-$%moneyOnHold% (On Hold) -$%moneySaved% (Saved)</color> Database Flushed.</b>";
		public string SelfAddMoney { get; set; } = "\\n<b><color=green>$%amount%.</color> Admin Command.</b>";
		public string AllAddMoney { get; set; } = "\\n<b><color=green>$%amount%.</color> Admin Command.</b>";
		public string SteamIDAddMoney { get; set; } = "\\n<b><color=green>$%amount%.</color> Admin Command.</b>";
		public string SelfRemoveMoney { get; set; } = "\\n<b><color=red>-$%amount%.</color> Admin Command.</b>";
		public string AllRemoveMoney { get; set; } = "\\n<b><color=red>-$%amount%.</color> Admin Command.</b>";
		public string SteamIDRemoveMoney { get; set; } = "\\n<b><color=red>-$%amount%.</color> Admin Command.</b>";
	}
}
