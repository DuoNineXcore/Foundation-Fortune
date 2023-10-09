using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System.Linq;
using FoundationFortune.Commands.FortuneCommands.HintCommands;
using FoundationFortune.Commands.FortuneCommands.DatabaseCommands;

namespace FoundationFortune.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ParentDatabaseCommand : ParentCommand
    {
        public override string Command { get; } = "ffdatabase";
        public override string[] Aliases { get; } = { "ffdb" };
        public override string Description { get; } = "Manage Foundation Fortune's Database.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new AddMoney());
            RegisterCommand(new RemoveMoney());
            RegisterCommand(new FlushDatabase());
        }

        public ParentDatabaseCommand() => LoadGeneratedCommands();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "fuck you.";
            return false;
        }
    }
}
