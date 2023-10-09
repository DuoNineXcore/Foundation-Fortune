using CommandSystem;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using Exiled.API.Features;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.Commands.FortuneCommands.HintCommands;

namespace FoundationFortune.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ParentHintCommand : ParentCommand
    {
        public override string Command { get; } = "ffhint";
        public override string[] Aliases { get; } = { "ffht" };
        public override string Description { get; } = "Manage Foundation Fortune's Hint System.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new HintAlignment());
            RegisterCommand(new HintMinmode());
        }

        public ParentHintCommand() => LoadGeneratedCommands();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "fuck you.";
            return false;
        }
    }
}

