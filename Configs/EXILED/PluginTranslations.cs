using System;
using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace FoundationFortune.Configs.EXILED
{
	public class PluginTranslations : ITranslation
	{
		[Description("Server Events")]
		public string Kill { get; set; } = "<b><color=green>+$%killMoneyReward%</color> <color=#FFA500>+%killXPReward% (%multiplier%) EXP</color> %victim%'s Termination.</b>";
		public string Escape { get; set; } = "<b><color=green>+$%escapeReward%</color> Successfully Escaped.</b>";
		public string Death { get; set; } = "<b><color=red>$-%moneyBeforeDeath%.</color> You died.</b>";
		public string RoundEndWin { get; set; } = "<b><color=%winningFactionColor%>+$%winningAmount%</color> Winning Bonus.</b>\\n";
		public string RoundEndDraw { get; set; } = "<b><color=%drawFactionColor%>+$%drawAmount%</color> Draw Bonus.</b>\\n";
		public string RoundEndLoss { get; set; } = "<b><color=%losingFactionColor%>+$%losingAmount%</color> Losing Bonus.</b>\\n";

		[Description("Bot Proximity Hints")]
		public string SellingWorkstation { get; set; } = "<b>You're on a Selling Workstation.</b>\\n";
		public string BuyingBot { get; set; } = "\\n<b>You're around a buying bot. Type .buy list in the console.</b>";
		public string SellingBot { get; set; } = "\\n<b>You're around a Selling bot. Drop Items twice to sell them.</b>";

		[Description("Hint System Counters")]
		public string MoneyCounterSaved { get; set; } = "<b>Money Saved: <color=%rolecolor%>$%moneySaved%</color></b> -- ";
		public string MoneyCounterOnHold { get; set; } = "<b>Money On Hold: <color=%rolecolor%>$%moneyOnHold%</color></b>\\n";
		public string LevelCounter { get; set; } = " -- <b>Level: <color=%rolecolor%>$%curLevel%</color></b>";
		public string EXPCounter { get; set; } = " (<b>EXP: <color=%rolecolor%>%expCounter%</color></b>";
		public string PrestigeCounter { get; set; } = " <b>Prestige:<color=%rolecolor%> %prestigelevel%</color></b>)\\n";

		[Description("Hint System Events")]
		public string WrongBot { get; set; } = "<b><color=red>Wrong Bot.</color></b>\\n";
		public string SaleCancelled { get; set; } = "\\n<b><color=red>Item changed. Sale canceled.</color></b>\\n";
		public string ItemConfirmation { get; set; } = "\\n<b>This item is worth <color=green>%price%</color>, Confirm sale? (%time% seconds left)</b>\\n</align>";
		public string BuyItemSuccess { get; set; } = "\\n<b><color=#FFA500>+%xpReward% (%multiplier%) EXP</color> <color=red>-$%itemPrice%</color> Bought Item: %itemAlias%.</b>";
		public string BuyPerkSuccess { get; set; } = "\\n<b><color=#FFA500>+%xpReward% (%multiplier%) EXP</color> <color=red>-$%perkPrice%</color> Bought Perk: %perkAlias%.</b>\\n";
		public string SellItemSuccess { get; set; } = "\\n<b><color=#FFA500>+%xpReward% (%multiplier%) EXP</color> <color=green>+%price%$</color> Sold %itemName%.</b>\\n";

		[Description("Extraction Events")]
		public string ExtractionEvent = "<b>A <color=green>Money</color> Extraction Zone has opened up. \\nRoom: %room% Time Left: %time%</b>\\n";
		public string ExtractionTimer = "<b>You're in the extraction zone, extracting money in %time% seconds.</b>\\n";
		public string ExtractionNoMoney = "<b>You do not have any On Hold Money.</b>\\n";

		[Description("Death Coins")]
		public string DeathCoinPickup { get; set; } = "<b><color=green>+%coinValue%</color> Picked up Death Coin.</b>\\n";

		[Description("Item and Perk list return messages.")]
		public string ItemsList { get; set; } = "%buyableItemDisplayName% (%buyableItemAlias%): $%buyableItemPrice%";
		public string PerksList { get; set; } = "%perkItemDisplayName% (%perkItemAlias%) - %perkItemDescription%: $%perkItemPrice%";

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

		[Description("Perk bottles")]
		public string DrankPerkBottle { get; set; } = "<b><color=#FFA500>+%xpReward% (%multiplier%) EXP</color> You drank a Perk bottle. [<color=#FFC0CB>%type%</color>]</b>";
		public string HoldingPerkBottle { get; set; } = "<b>You are holding a Perk bottle. [<color=#FFC0CB>%type%</color>]</b>";
	}
}
