using CommandSystem;
using System;

namespace FoundationFortune.Commands.FortuneCommands
{
     [CommandHandler(typeof(ClientCommandHandler))]
     [CommandHandler(typeof(RemoteAdminCommandHandler))]
     public class ParentFortuneCommand : ParentCommand
     {
          public override string Command { get; } = "foundationfortune";
          public override string[] Aliases { get; } = { "ff" };
          public override string Description { get; } = "Manage FoundationFortune Hints, NPCs and Database.";
          public override void LoadGeneratedCommands()
          {
               RegisterCommand(new DatabaseCommand());
               RegisterCommand(new HintCommand());
               RegisterCommand(new NpcCommand());
          }
          public ParentFortuneCommand() => LoadGeneratedCommands();

          protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
          {
               response = "Input a valid subcommand: database, hint or npc";
               return false;
          }
    }
}