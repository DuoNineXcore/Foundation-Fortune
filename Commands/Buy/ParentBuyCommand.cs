using CommandSystem;
using FoundationFortune.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace FoundationFortune.Commands.Buy
{
     [CommandHandler(typeof(ClientCommandHandler))]
     [CommandHandler(typeof(RemoteAdminCommandHandler))]
     public sealed class ParentBuyCommand : ParentCommand
     {
          public static List<PurchasesObject> PlayerLimits = new();

          public override string Command { get; } = "buy";
          public override string[] Aliases { get; } =  new string[] { "b" };
          public override string Description { get; } = "You buy stuff.. What else did you expect?";
          public override void LoadGeneratedCommands()
          {
               RegisterCommand(new BuyItemCommand());
               RegisterCommand(new BuyListCommand());
               RegisterCommand(new BuyPerk());
          }

          public ParentBuyCommand() => LoadGeneratedCommands();

          protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
          {
               response = "Input a valid subcommand: Item, Perk, List";
               return false;
          }
     }
}
