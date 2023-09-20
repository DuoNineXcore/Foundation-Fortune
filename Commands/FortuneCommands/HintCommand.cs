using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands
{
     internal class HintCommand : ICommand
     {
          public string Command { get; } = "hint";
          public string Description { get; } = "Hint stuff";
          public string[] Aliases { get; } = new string[] { "h"};

          public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
          {
               response = "Not implemented!";
               return false;
          }
     }
}
