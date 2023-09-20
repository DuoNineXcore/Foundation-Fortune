using CommandSystem;
using System;

namespace FoundationFortune.Commands.FortuneCommands
{
     internal class NpcCommand : ICommand
     {
          public string Command { get; } = "npc";
          public string Description { get; } = "Npc stuff";
          public string[] Aliases { get; } = new string[] { "n"};

          public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
          {
               response = "Not implemented!";
               return false;
          }
     }
}
