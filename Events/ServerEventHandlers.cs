﻿using FoundationFortune.API.Database;
using MEC;
using PlayerRoles;
using PlayerStatsSystem;
using UnityEngine;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Scp049;
using Exiled.Events.EventArgs.Scp0492;
using FoundationFortune.API.NPCs;
using Discord;
using System.Linq;
using Mono.Cecil.Cil;
using Exiled.Events.EventArgs.Server;

namespace FoundationFortune.Events
{
    public enum HintAlign
    {
        Center, 
        Right, 
        Left,
        TopLeft,
        TopRight
    }

    public partial class ServerEvents
    {
        private CoroutineHandle moneyHintCoroutine;

        public void RoundStart()
        {
            foreach (Player p in Player.List)
            {
                if (!moneyHintCoroutine.IsRunning) moneyHintCoroutine = Timing.RunCoroutine(UpdateMoneyAndHints(p));
            }

            InitializeWorkstationPositions();
            InitializeBuyingBots();
        }

        public void RoundEnd(RoundEndedEventArgs ev)
        {
            if (moneyHintCoroutine.IsRunning) Timing.KillCoroutines(moneyHintCoroutine);
        }

        public void RegisterInDatabase(VerifiedEventArgs ev)
        {
            var existingPlayer = PlayerDataRepository.GetPlayerById(ev.Player.UserId);
            if (existingPlayer == null && !ev.Player.IsNPC)
            {
                var newPlayer = new PlayerData
                {
                    Username = ev.Player.DisplayNickname,
                    UserId = ev.Player.UserId,
                    MoneyOnHold = 0,
                    MoneySaved = 0,
                    HintMinmode = false,
                    HintAlign = HintAlign.Center
                };
                PlayerDataRepository.InsertPlayer(newPlayer);
            }
            if (Round.IsStarted) moneyHintCoroutine = Timing.RunCoroutine(UpdateMoneyAndHints(ev.Player));
        }

        public void SpawningNpc(SpawningEventArgs ev)
        {
            if (ev.Player.IsNPC)
            {
                RoundSummary.singleton.Network_chaosTargetCount -= 1;
            }
        }

        public void LeavingNPC(LeftEventArgs ev)
        {
            if (ev.Player.IsNPC)
            {
                RoundSummary.singleton.Network_chaosTargetCount += 1;
            }
        }

        public void KillingReward(DiedEventArgs ev)
        {
            if (ev.Attacker != null && ev.Attacker != ev.Player && ev.Attacker.IsScp)
            {
                var config = FoundationFortune.Singleton.Config;
                var killHint = config.KillHint.Replace("[victim]", ev.Player.Nickname);
                EnqueueHint(ev.Attacker, killHint, config.KillReward, config.MaxHintAge, config.KillRewardTransfer, config.KillRewardTransferAll);
            }
        }

        public void EscapingReward(EscapingEventArgs ev)
        {
            var config = FoundationFortune.Singleton.Config;
            EnqueueHint(ev.Player, $"{config.EscapeHint}", config.EscapeReward, config.MaxHintAge, config.EscapeRewardTransfer, config.EscapeRewardTransferAll);
        }

        public void SellingItem(DroppingItemEventArgs ev)
        {
            if (!IsPlayerOnSellingWorkstation(ev.Player) && !IsPlayerOnBuyingBotRadius(ev.Player))
            {
                ev.IsAllowed = true;
                return;
            }

            if (IsPlayerNearSellingBot(ev.Player))
            {
                if (!confirmSell.ContainsKey(ev.Player.UserId))
                {
                    foreach (var sellableItem in FoundationFortune.Singleton.Config.SellableItems)
                    {
                        if (ev.Item.Type == sellableItem.ItemType)
                        {
                            itemsBeingSold[ev.Player.UserId] = (ev.Item, sellableItem.Price);

                            confirmSell[ev.Player.UserId] = true;
                            dropTimestamp[ev.Player.UserId] = Time.time;
                            ev.IsAllowed = false;
                            return;
                        }
                    }
                }

                if (confirmSell.TryGetValue(ev.Player.UserId, out bool isConfirming) && dropTimestamp.TryGetValue(ev.Player.UserId, out float dropTime))
                {
                    if (isConfirming && Time.time - dropTime <= FoundationFortune.Singleton.Config.SellingConfirmationTime)
                    {
                        if (itemsBeingSold.TryGetValue(ev.Player.UserId, out var soldItemData))
                        {
                            Item soldItem = soldItemData.item;
                            int price = soldItemData.price;

                            if (soldItem == ev.Item)
                            {
                                Npc buyingbot = GetBuyingBotNearPlayer(ev.Player);
                                BuyingBot.PlayAudio(buyingbot, "BuySuccess.ogg", 50, false, VoiceChat.VoiceChatChannel.Mimicry);
                                EnqueueHint(ev.Player, $"<b><size=24><color=green>+{price}$</color> Sold {FoundationFortune.Singleton.Config.SellableItems.Find(x => x.ItemType == ev.Item.Type).DisplayName}.</color></b></size>", price, 3, false, false);
                                ev.Player.RemoveItem(ev.Item);
                            }
                            else
                            {
                                EnqueueHint(ev.Player, "<b><size=24><color=red>Item changed. Sale canceled.</color></b></size>", 0, 3, false, false);
                            }

                            itemsBeingSold.Remove(ev.Player.UserId);
                            ev.IsAllowed = true;
                            return;
                        }
                    }
                }
            }
            else
            {
                ev.IsAllowed = true;
                EnqueueHint(ev.Player, "<b><size=24><color=red>This is not a selling bot.</color></b></size>", 0, 3, false, false);
            }

            ev.IsAllowed = true;
        }

        public void FuckYourAbility(ActivatingSenseEventArgs ev)
        {
            if (ev.Target != null)
            {
                if (ev.Target.IsNPC)
                {
                    ev.IsAllowed = false;
                }
            }
        }

        public void FuckYourOtherAbility(TriggeringBloodlustEventArgs ev)
        {
            if (ev.Target != null)
            {
                if (ev.Target.IsNPC)
                {
                    ev.IsAllowed = false;
                }
            }
        }
    }
}
