﻿using MEC;

namespace FoundationFortune.API.Core.Models.Classes.Events
{
    public class ExtractionTimerData
    {
        public CoroutineHandle CoroutineHandle { get; set; }
        public float StartTime { get; set; }
    }
}