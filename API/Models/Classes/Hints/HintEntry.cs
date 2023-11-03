namespace FoundationFortune.API.Models.Classes.Hints
{
    public class HintEntry
    {
        public string Text { get; set; }
        public float Timestamp { get; set; }
        public bool IsAnimated { get; set; }

        public HintEntry(string text, float timestamp, bool isAnimated)
        {
            Text = text;
            Timestamp = timestamp;
            IsAnimated = isAnimated;
        }
    }
}
