using Exiled.API.Interfaces;
using System.ComponentModel;

namespace FoundationFortune.Configs
{
	public class PluginTranslations : ITranslation
	{
		[Description("Server Events")]
		public string Kill { get; set; } = "<b><color=green>+$300</color> %victim%'s Termination.</b>\\n";
		public string Escape { get; set; } = "<b><color=green>+$300</color> Successfully Escaped.</b>\\n";
		public string Death { get; set; } = "<b><color=red>$-%moneyBeforeDeath%.</color> You died.</b>\\n";
		public string RoundEndWin { get; set; } = "<b><color=%winningFactionColor%>+$%winningAmount%</color> Winning Bonus.</b>\\n";
		public string RoundEndDraw { get; set; } = "<b><color=%drawFactionColor%>+$%drawAmount%</color> Draw Bonus.</b>\\n";
		public string RoundEndLoss { get; set; } = "<b><color=%losingFactionColor%>+$%losingAmount%</color> Losing Bonus.</b>\\n";

		[Description("Bot Proximity Hints")]
		public string SellingWorkstation { get; set; } = "<b>You're on a Selling Workstation.</b>\\n";
		public string BuyingBot { get; set; } = "<b>You're around a buying bot. Type .buy list in the console.</b>\\n";
		public string SellingBot { get; set; } = "<b>You're around a Selling bot. Drop Items twice to sell them.</b>\\n";

		[Description("Hint System Counters")]
		public string MoneyCounterSaved { get; set; } = "<b>Money Saved: <color=%rolecolor%>$%moneySaved%</color></b>\\n";
		public string MoneyCounterOnHold { get; set; } = "<b>Money On Hold: <color=%rolecolor%>$%moneyOnHold%</color></b>\\n";

		[Description("Hint System Events")]
		public string WrongBot { get; set; } = "<b><color=red>This is not a selling bot.</color></b>\\n";
		public string SaleCancelled { get; set; } = "<b><color=red>Item changed. Sale canceled.</color></b>\\n";
		public string SellSuccess { get; set; } = "<b><color=green>+%price%$</color> Sold %itemName%.</color></b>\\n";
		public string ItemConfirmation { get; set; } = "<b>This item is worth <color=green>%price%</color>, Confirm sale? (%time% seconds left)</b>\\n</align>";
		public string BuyItemSuccess { get; set; } = "<b><color=red>-$%itemPrice%</color> Bought %itemAlias%.</b>\\n";

		[Description("Extraction Events")]
		public string ExtractionEvent = "<b>A <color=green>Money</color> Extraction Zone has opened up. \\nRoom: %room% Time Left: %time%</b>\\n";
		public string ExtractionTimer = "<b>You're in the extraction zone, extracting money in %time% seconds.</b>\\n";
		public string ExtractionNoMoney = "<b>You do not have any On Hold Money.</b>\\n";

		[Description("Death Coins")]
		public string DeathCoinPickup { get; set; } = "<b><color=green>+%coinValue%</color> Picked up Death Coin.</b>\\n";

		[Description("Item and Perk list return messages.")]
		public string ItemsList { get; set; } = "\\nItems available for purchase:\\n%buyableItemDisplayName% (%buyableItemAlias%): $%buyableItemPrice%";
		public string PerksList { get; set; } = "\\nPerks available for purchase:\\n%perkItemDisplayName% (%perkItemAlias%) - %perkItemDescription%: $%perkItemPrice%";

		[Description("Revival System Hints")]
		public string RevivalNoDeadPlayer { get; set; } = "<b>No dead player with Name: '%targetName%' found nearby to revive.</b>\\n";
		public string RevivalSuccess { get; set; } = "<b><color=%rolecolor%>%nickname%</color> Has Revived <color=%rolecolor%>%target%</color></b>\\n";

		[Description("Bounty System Hints")]
		public string SelfBounty { get; set; } = "<b><color=red>You're Being Hunted!</color> Reward: %bountyReward% Duration: %duration%</b>\\n";
		public string OtherBounty { get; set; } = "<b><color=red>%player% is being Hunted!</color></b>\\nReward: %bountyReward% Duration: %duration%";
		public string BountyFinished { get; set; } = "<b><color=%victimrolecolor%>%victim%</color> has been killed by <color=%attackercolor%>%attacker%</color>, Bounty Finished.</b>\\n";
		public string BountyPlayerDied { get; set; } = "<b>%victim% Died from unknown reasons, Bounty Finished.</b>\\n";
		public string BountyKill { get; set; } = "<b><color=green>+%bountyPrice%$</color> You killed %victim%. Bounty Finished.</b>\\n";

		[Description("Database Operation Hints")]
		public string FlushedDatabase { get; set; } = "<b><color=red>-$%moneyOnHold% (On Hold) -$%moneySaved% (Saved)</color> Database Flushed.</b>\\n";
		public string SelfAddMoney { get; set; } = "<b><color=green>$%amount%.</color> Admin Command.</b>\\n";
		public string AllAddMoney { get; set; } = "<b><color=green>$%amount%.</color> Admin Command.</b>\\n";
		public string SteamIDAddMoney { get; set; } = "<b><color=green>$%amount%.</color> Admin Command.</b>\\n";
		public string SelfRemoveMoney { get; set; } = "<b><color=red>-$%amount%.</color> Admin Command.</b>\\n";
		public string AllRemoveMoney { get; set; } = "<b><color=red>-$%amount%.</color> Admin Command.</b>\\n";
		public string SteamIDRemoveMoney { get; set; } = "<b><color=red>-$%amount%.</color> Admin Command.</b>\\n";
	}
}
