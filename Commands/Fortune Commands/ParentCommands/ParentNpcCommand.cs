using CommandSystem;
using System;
using FoundationFortune.Commands.FortuneCommands.NpcCommands;

namespace FoundationFortune.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ParentNpcCommand : ParentCommand
    {
        public override string Command { get; } = "ffnpc";
        public override string[] Aliases { get; } = { "npc" };
        public override string Description { get; } = "Manage Foundation Fortune's Npc System.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new NpcAdd());
            RegisterCommand(new NpcFlush());
            RegisterCommand(new NpcRemove());
            RegisterCommand(new NpcList());
        }

        public ParentNpcCommand() => LoadGeneratedCommands();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "fuck you.";
            return false;
        }
    }
}
