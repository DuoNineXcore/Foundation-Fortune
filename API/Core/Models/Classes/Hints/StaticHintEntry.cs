namespace FoundationFortune.API.Core.Models.Classes.Hints
{
    public class StaticHintEntry
    {
        public string Text { get; set; }
        public float Timestamp { get; set; }

        public StaticHintEntry(string text, float timestamp)
        {
            Text = text;
            Timestamp = timestamp;
        }
    }
}