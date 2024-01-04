using System;

namespace FoundationFortune.API.Core.Common.Models.Hints;

public class StaticHintEntry
{
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }

    public StaticHintEntry(string text, DateTime timestamp)
    {
        Text = text;
        Timestamp = timestamp;
    }
}