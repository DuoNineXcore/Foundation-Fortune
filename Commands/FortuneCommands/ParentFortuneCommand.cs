//using CommandSystem;
//using System;
//using System.Text;

//namespace FoundationFortune.Commands.FortuneCommands
//{
//     [CommandHandler(typeof(ClientCommandHandler))]
//     [CommandHandler(typeof(RemoteAdminCommandHandler))]
//     public class ParentFortuneCommand : ParentCommand
//     {
//          public override string Command { get; } = "foundationfortune";
//          public override string[] Aliases { get; } = { "ff" };
//          public override string Description { get; } = "Manage FoundationFortune NPCs and Database.";
//          public override void LoadGeneratedCommands()
//          {
//               RegisterCommand(new DatabaseCommand());
//               RegisterCommand(new HintCommand());
//               RegisterCommand(new NpcCommand());
//          }

//          protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
//          {
//               StringBuilder builder = new();
//               builder.AppendLine("Input a valid subcommand:");
//               builder.AppendLine("Database");
//               builder.AppendLine("Hint");
//               builder.AppendLine("Npc");
//               response = builder.ToString();
//               return false;
//          }
//     }
//}