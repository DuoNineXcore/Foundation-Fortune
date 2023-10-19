using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using System.Linq;
using FoundationFortune.API.Database;

namespace FoundationFortune.Commands.FortuneCommands.DatabaseCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class FlushDatabase : ICommand
    {
        public string Command { get; } = "ff_flushdatabase";
        public string Description { get; } = "Flush the database, removing all stored data.";
        public string[] Aliases { get; } = new string[] {};

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            var pluginTranslations = FoundationFortune.Singleton.Translation;
            Player p = Player.Get(sender);

            if (!sender.CheckPermission("ff.database.flush") || !PlayerDataRepository.GetPluginAdmin(p.UserId))
            {
                response = "You do not have permission to use this command.";
                return false;
            }

            foreach (var player in Player.List.Where(player => !player.IsNPC))
            {
                int? moneyOnHold = PlayerDataRepository.GetMoneyOnHold(player.UserId);
                int? moneySaved = PlayerDataRepository.GetMoneySaved(player.UserId);

                string FlushedDatabase = pluginTranslations.FlushedDatabase
                    .Replace("%moneyOnHold%", moneyOnHold.ToString())
                    .Replace("%moneySaved%", moneySaved.ToString());
                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"{FlushedDatabase}", 5f);
                PlayerDataRepository.EmptyMoney(player.UserId, true, true);
            }

            response = "Flushed Database.";
            return true;
        }
    }
}
