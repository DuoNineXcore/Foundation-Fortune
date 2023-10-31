/*
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using RemoteAdmin;
using System;
using FoundationFortune.API.Models;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintAnimCommand : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_hintanim";
        public string Description { get; } = "Change your hint animation.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<right/left/center>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 1 || !Enum.TryParse(args.At(0), true, out HintAnim anim))
            {
                response = "Invalid usage. Correct usage: ff_hintanim <right/left/center>";
                return false;
            }

            Player player = Player.Get(playerSender);

            PlayerDataRepository.SetHintAnim(player.UserId, anim);
            response = $"Your hint animation has been set to {anim}.";
            return true;
        }
    }
}
*/
