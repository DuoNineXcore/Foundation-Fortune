using MEC;

namespace FoundationFortune.API.Common.Models.Events;

public class ExtractionTimerData
{
    public CoroutineHandle CoroutineHandle { get; set; }
    public float StartTime { get; set; }
}