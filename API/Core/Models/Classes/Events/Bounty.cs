﻿using System;

namespace FoundationFortune.API.Core.Models.Classes.Events
{
    public class Bounty
    {
        public Exiled.API.Features.Player Player { get; }
        public bool IsBountied { get; set; }
        public int Value { get; }
        public DateTime ExpirationTime { get; }

        public Bounty(Exiled.API.Features.Player player, bool isBountied, int value, DateTime expirationTime)
        {
            Player = player;
            IsBountied = isBountied;
            Value = value;
            ExpirationTime = expirationTime;
        }
    }
}