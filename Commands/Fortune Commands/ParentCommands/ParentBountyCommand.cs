using CommandSystem;
using System;
using FoundationFortune.Commands.FortuneCommands.BountyCommands;

namespace FoundationFortune.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class ParentBountyCommand : ParentCommand
    {
        public override string Command { get; } = "ffbounty";
        public override string[] Aliases { get; } = { "ffbt" };
        public override string Description { get; } = "Manage Foundation Fortune's Bounty System.";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new AddBounty());
            RegisterCommand(new RemoveBounty());
            RegisterCommand(new FlushAllBounties());
        }

        public ParentBountyCommand() => LoadGeneratedCommands();

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "fuck you.";
            return false;
        }
    }
}
