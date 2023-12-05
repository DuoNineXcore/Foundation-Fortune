using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using FoundationFortune.API.Core.Database;
using FoundationFortune.API.Core.Models.Enums.Systems.QuestSystem;
using FoundationFortune.API.Features.Systems.EventBasedSystems;

namespace FoundationFortune.Commands.FortuneCommands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public sealed class Quest : ICommand, IUsageProvider
    {
        public string Command { get; } = "quest";
        public string[] Aliases { get; } = { string.Empty };
        public string Description { get; } = "quest system lol";
        public string[] Usage { get; } = { "<number>" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (arguments.Count == 0)
            {
                List<QuestType> randomQuests = QuestRotation.GetShuffledQuestsForUser(player.UserId);
                string questList = string.Join("\n", randomQuests.Select((questType, index) =>
                {
                    string questDescription = GetQuestDescription(questType);
                    string questStatus = QuestSystem.IsQuestActive(player.UserId, questType) ? " (Ongoing)" : "";
                    return $"Quest {index + 1}: {questDescription}{questStatus}";
                }));

                string resetMessage = QuestRotation.MaxRotations - QuestRotation.RotationCounter == 1 ? "round" : "rounds";
                response = $"Choose a Quest by typing: {Command} 1-3\n{questList}\nQuests will reset in {QuestRotation.MaxRotations - QuestRotation.RotationCounter} {resetMessage}.";
                return true;
            }

            if (int.TryParse(arguments.At(0), out int selectedQuestNumber))
            {
                if (selectedQuestNumber is >= 1 and <= 3)
                {
                    QuestType selectedQuestType = QuestRotation.GetShuffledQuestsForUser(player.UserId)[selectedQuestNumber - 1];

                    if (QuestSystem.IsQuestActive(player.UserId)) response = $"You already have an ongoing quest.";
                    else
                    {
                        QuestSystem.EnableQuest(player.UserId, selectedQuestType);
                        response = $"You are now doing the {selectedQuestType} Quest.";
                    }
                    return true;
                }
            }

            response = "Invalid quest selection. Use the command without arguments to see available quests.";
            return false;
        }

        private static string GetQuestDescription(QuestType questType)
        {
            return questType switch
            {
                QuestType.GetAKillstreak => "Kill a specific number of enemies.",
                QuestType.KillZombies => "Kill a certain number of zombies.",
                QuestType.UnlockGenerators => "Unlock a specified number of generators.",
                QuestType.BuyItems => "Purchase a set of items.",
                QuestType.UseEtherealIntervention => "Use Ethereal Intervention a certain number of times.",
                QuestType.CollectMoneyFromDeathCoins => "Collect a specified amount of money from Death Coins.",
                QuestType.ThrowGhostlights => "Throw a certain number of Ghostlights.",
                _ => $"what the fuck is {questType}?????????"
            };
        }
    }
}
