using CommandSystem;
using System;
using System.Text;
using FoundationFortune.Events;
using PluginAPI.Core;
using RemoteAdmin;

namespace FoundationFortune.Commands.Buy
{
     [CommandHandler(typeof(ClientCommandHandler))]
     [CommandHandler(typeof(RemoteAdminCommandHandler))]
     public class ParentBuyCommand : ParentCommand
     {
          public override string Command { get; } = "Buy";
          public override string[] Aliases { get; } = new string[] { "B" };
          public override string Description { get; } = "You buy stuff.. What else did you expect?";
          public override void LoadGeneratedCommands()
          {
               RegisterCommand(new BuyItemCommand());
               RegisterCommand(new BuyListCommand());
               RegisterCommand(new BuyPerk());
          }

          protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
          {
               StringBuilder builder = new();
               builder.AppendLine("<Input a valid subcommand:");
               builder.AppendLine("Item");
               builder.AppendLine("Perk");
               builder.AppendLine("List");
               response = builder.ToString();
               return false;
          }
     }
}
