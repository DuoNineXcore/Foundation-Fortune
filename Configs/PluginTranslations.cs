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
		[Description("RAAAAAAHHHHHHHHHHHHHHHHHHHHHHHHHHH")]
		public string KillHint { get; set; } = "<b><size=24><color=green>+$300</color> Killed %victim%. </b></size>";
		public string EscapeHint { get; set; } = "<b><size=24><color=green>+$300</color> Escaped.</b></size>";

		[Description("idk")]
		public string SellingWorkstation { get; set; } = "<b><size=24>You're on a Selling Workstation.</size></b>";
		public string BuyingBot { get; set; } = "<b><size=24>You're around a buying bot. Type .buy list in the console.</size></b>";
		public string SellingBot { get; set; } = "<b><size=24>You're around a Selling bot. Drop Items twice to sell them.</size></b>";

		[Description("Hint System")]
		public string DefaultHint { get; set; } = "<b><size=24>Money On Hold: <color=%rolecolor%>%moneyOnHold%</color></size>\n<size=24>Money Saved: <color=%rolecolor%>%moneySaved%</color></size></b>";
		public string WrongBot { get; set; } = "<b><size=24><color=red>This is not a selling bot.</color></b></size>";
		public string SaleCancelled { get; set; } = "<b><size=24><color=red>Item changed. Sale canceled.</color></b></size>";
		public string SellSuccess { get; set; } = "<b><size=24><color=green>+%price%$</color> Sold %itemName%.</color></b></size>";
		public string ItemConfirmation { get; set; } = "<b><size=24>This item is worth <color=green>%price%</color>, Confirm sale? (%time% seconds left)</size></b></align>";
		public string BuyItemSuccess { get; set; } = "<b><size=24><color=red>-${buyItem.Price}</color> Bought {buyItem.DisplayName}</b></size>";
		public string Death { get; set; } = "<b><size=24><color=red>$-%moneyBeforeDeath%.</color> You died.</b></size>";
		public string DeathCoinPickup { get; set; } = "<b><size=24><color=green>+%coinValue%</color> Picked up Death Coin.</b></size>";
		public string RevivalNoDeadPlayer { get; set; } = "<b><size=24>No dead player with Name: '%targetName%' found nearby to revive.</b></size>";
		public string RevivalSuccess { get; set; } = "<b><size=24><color=%rolecolor%>%nickname%</color> Has Revived <color=%rolecolor%>%target%</color></b></size>";

		[Description("FF Database Hints")]
		public string FlushedDatabase { get; set; } = "<b><size=24><color=red>-${moneyOnHold} (On Hold) -${moneySaved} (Saved)</color> Database Flushed.</size></b>";
		public string SelfAddMoney { get; set; } = "<b><size=24><color=green>${amount}.</color> Admin Command.</size></b>";
		public string AllAddMoney { get; set; } = "<b><size=24><color=green>${amount}.</color> Admin Command.</size></b>";
		public string SteamIDAddMoney { get; set; } = "<b><size=24><color=green>${amount}.</color> Admin Command.</size></b>";
		public string SelfRemoveMoney { get; set; } = "<b><size=24><color=red>-${amount}.</color> Admin Command.</size></b>";
		public string AllRemoveMoney { get; set; } = "<b><size=24><color=red>-${amount}.</color> Admin Command.</size></b>";
		public string SteamIDRemoveMoney { get; set; } = "<b><size=24><color=red>-${amount}.</color> Admin Command.</size></b>";
	}
}
