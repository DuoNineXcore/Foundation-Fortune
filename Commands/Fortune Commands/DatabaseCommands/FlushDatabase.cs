using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using Exiled.API.Features;
using System.Linq;
using FoundationFortune.API.Database;

namespace FoundationFortune.Commands.FortuneCommands.DatabaseCommands
{
    internal class FlushDatabase : ICommand
    {
        public string Command { get; } = "flush";
        public string Description { get; } = "Flush the database, removing all stored data.";
        public string[] Aliases { get; } = new string[] { "flushdatabase", "cleardata", "resetdata" };

        public bool Execute(ArraySegment<string> args, ICommandSender sender, out string response)
        {
            var pluginTranslations = FoundationFortune.Singleton.Translation;
            if (!sender.CheckPermission("ff.database.flush"))
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
                FoundationFortune.Singleton.serverEvents.EnqueueHint(player, $"{FlushedDatabase}", 0, 5f, false, false);
                PlayerDataRepository.EmptyMoney(player.UserId, true, true);
            }

            response = "Flushed Database.";
            return true;
        }
    }
}
