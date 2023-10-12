using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Enums;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationFortune.Commands.FortuneCommands.HintCommands.Local
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class HintOpacity : ICommand, IUsageProvider
    {
        public string Command { get; } = "ff_hintopacity";
        public string Description { get; } = "Change your hint opacity.";
        public string[] Aliases { get; } = new string[] {};
        public string[] Usage { get; } = new string[] { "<opacity>" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "Only players can use this command.";
                return false;
            }

            if (args.Count < 1 || !int.TryParse(args.At(0), out int newOpacity))
            {
                response = "Invalid usage. Correct usage: ff_hintopacity <opacity>";
                return false;
            }

            if (newOpacity < 0 || newOpacity > 100)
            {
                response = "Hint opacity must be between 0 and 100.";
                return false;
            }

            Player player = Player.Get(playerSender);
            PlayerDataRepository.SetHintOpacity(player.UserId, newOpacity);
            response = $"Your hint opacity has been set to {newOpacity}.";
            return true;
        }
    }

}
