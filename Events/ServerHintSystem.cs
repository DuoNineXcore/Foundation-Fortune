using Exiled.API.Features;
using FoundationFortune.API.Database;
using FoundationFortune.API.Models.Classes;
using FoundationFortune.API.Models.Enums;
using FoundationFortune.API.NPCs;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Exiled.API.Features.Items;
using Exiled.API.Enums;

namespace FoundationFortune.Events
{
    public partial class ServerEvents
    {
        private Dictionary<string, Queue<HintEntry>> recentHints = new();
        private Dictionary<string, bool> confirmSell = new();
        private Dictionary<string, float> dropTimestamp = new();
        private Dictionary<string, (Item item, int price)> itemsBeingSold = new();

        private IEnumerator<float> UpdateMoneyAndHints()
        {
            while (true)
            {
                foreach (Player ply in Player.List.Where(p => !p.IsDead && !p.IsNPC))
                {
                    int moneyOnHold = PlayerDataRepository.GetMoneyOnHold(ply.UserId);
                    int moneySaved = PlayerDataRepository.GetMoneySaved(ply.UserId);
                    HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

                    string hintMessage = "";

                    if (!PlayerDataRepository.GetHintMinmode(ply.UserId))
                    {
                        string moneySavedString = FoundationFortune.Singleton.Translation.MoneyCounterSaved
                            .Replace("%rolecolor%", ply.Role.Color.ToHex())
                            .Replace("%moneySaved%", moneySaved.ToString());
                        string moneyHoldString = FoundationFortune.Singleton.Translation.MoneyCounterOnHold
                            .Replace("%rolecolor%", ply.Role.Color.ToHex())
                            .Replace("%moneyOnHold%", moneyOnHold.ToString());

                        hintMessage += $"\n<align={hintAlignment}>{moneySavedString}{moneyHoldString}<align=left>\n";
                    }

                    try
                    {
                        UpdateExtractionEventHint(ply, ref hintMessage);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    HandleWorkstationMessages(ply, ref hintMessage);
                    HandleBuyingBotMessages(ply, ref hintMessage);

                    Bounty bounty = BountiedPlayers.FirstOrDefault(b => b.Player == ply);
                    if (bounty != null)
                    {
                        TimeSpan timeLeft = bounty.ExpirationTime - DateTime.Now;
                        string bountyMessage = ply.UserId == bounty.Player.UserId
                            ? FoundationFortune.Singleton.Translation.SelfBounty.Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"))
                            : FoundationFortune.Singleton.Translation.OtherBounty.Replace("%player%", bounty.Player.Nickname).Replace("%duration%", timeLeft.ToString(@"hh\:mm\:ss"));

                        hintMessage += $"<align={hintAlignment}>\n{bountyMessage}</align>";
                    }

                    string recentHintsText = GetRecentHints(ply.UserId);
                    if (!string.IsNullOrEmpty(recentHintsText))
                    {
                        hintMessage += $"<align={hintAlignment}>\n{recentHintsText}</align>";
                    }

                    ply.ShowHint(hintMessage, 2);

                    if (confirmSell.ContainsKey(ply.UserId) && Time.time - dropTimestamp[ply.UserId] >= FoundationFortune.Singleton.Config.SellingConfirmationTime)
                    {
                        confirmSell.Remove(ply.UserId);
                        dropTimestamp.Remove(ply.UserId);
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        private void HandleWorkstationMessages(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);

            if (IsPlayerOnSellingWorkstation(ply))
            {
                if (!confirmSell.ContainsKey(ply.UserId))
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align>";
                }
                else if (confirmSell[ply.UserId])
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingWorkstation}</align>";

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        hintMessage += $"<align={hintAlignment}>\n{FoundationFortune.Singleton.Translation.ItemConfirmation.Replace("%price%", price.ToString()).Replace("%time%", GetConfirmationTimeLeft(ply).ToString())}</align>";
                    }
                }
            }
        }

        private void HandleBuyingBotMessages(Player ply, ref string hintMessage)
        {
            HintAlign? hintAlignment = PlayerDataRepository.GetUserHintAlign(ply.UserId);
            Npc npc = GetBuyingBotNearPlayer(ply);

            if (IsPlayerNearBuyingBot(ply, npc))
            {
                BuyingBot.LookAt(npc, ply.Position);
                hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.BuyingBot}</align>";
            }
            else if (IsPlayerNearSellingBot(ply))
            {
                BuyingBot.LookAt(npc, ply.Position);
                if (!confirmSell.ContainsKey(ply.UserId)) hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingBot}</align>";

                else if (confirmSell[ply.UserId])
                {
                    hintMessage += $"<align={hintAlignment}>{FoundationFortune.Singleton.Translation.SellingBot}</align>";

                    if (itemsBeingSold.TryGetValue(ply.UserId, out var soldItemData))
                    {
                        int price = soldItemData.price;
                        string confirmationHint = FoundationFortune.Singleton.Translation.ItemConfirmation
                            .Replace("%price%", price.ToString())
                            .Replace("%time%", GetConfirmationTimeLeft(ply));

                        hintMessage += $"<align={hintAlignment}>\n{confirmationHint}</align>";
                    }
                }
            }
        }

        public string GetConfirmationTimeLeft(Player ply)
        {
            if (dropTimestamp.ContainsKey(ply.UserId))
            {
                float timeLeft = FoundationFortune.Singleton.Config.SellingConfirmationTime - (Time.time - dropTimestamp[ply.UserId]);
                if (timeLeft > 0)
                {
                    return timeLeft.ToString("F0");
                }
            }
            return "0";
        }

        public string GetRecentHints(string userId)
        {
            if (recentHints.ContainsKey(userId))
            {
                var currentHints = recentHints[userId];
                while (currentHints.Count > 0 && (Time.time - currentHints.Peek().Timestamp) > FoundationFortune.Singleton.Config.MaxHintAge)
                {
                    currentHints.Dequeue();
                }

                return string.Join("\n", currentHints.Select(entry => entry.Text));
            }

            return "";
        }

        public void EnqueueHint(Player player, string hint, int reward, float duration, bool transferToSavings, bool transferAllToSavings)
        {
            float expirationTime = Time.time + duration;

            if (!recentHints.ContainsKey(player.UserId)) recentHints[player.UserId] = new Queue<HintEntry>();
            if (transferAllToSavings) PlayerDataRepository.TransferMoney(player.UserId, true);
            else if (transferToSavings) PlayerDataRepository.ModifyMoney(player.UserId, reward, false, false, true);
            else PlayerDataRepository.ModifyMoney(player.UserId, reward, false, true, false);
            recentHints[player.UserId].Enqueue(new HintEntry(hint, expirationTime, reward));

            while (recentHints[player.UserId].Count > FoundationFortune.Singleton.Config.MaxHintsToShow) recentHints[player.UserId].Dequeue();
        }
    }
}
