namespace FoundationFortune.API;

using System.Collections.Generic;
using MEC;

public static class CoroutineManager
{
    public static List<CoroutineHandle> Coroutines = new();

    public static void KillCoroutines()
    {
        foreach (CoroutineHandle coroutine in Coroutines) Timing.KillCoroutines(coroutine);
        Coroutines.Clear();
    }
}