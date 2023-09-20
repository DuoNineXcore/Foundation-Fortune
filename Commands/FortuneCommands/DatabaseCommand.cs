using CommandSystem;
using System;
using System.Text;

namespace FoundationFortune.Commands.FortuneCommands
{
     internal class DatabaseCommand : ICommand
     {
          public string Command { get; } = "database";
          public string Description { get; } = "Database stuff";
          public string[] Aliases { get; } = new string[] { "d" };

          public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
          {
               response = "Not implemented!";
               return false;
          }
     }
}
