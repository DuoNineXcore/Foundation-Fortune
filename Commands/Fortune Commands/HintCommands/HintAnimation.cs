using CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class SetHintAnim : ICommand
    {
        public string Command { get; } = "ff_sethintanim";
        public string Description { get; } = "Change your hint animation settings.";
        public string[] Aliases { get; } = new string[] { string.Empty };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            response = "Done";
            return true;
        }
    }
}
