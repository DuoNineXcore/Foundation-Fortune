namespace FoundationFortune.API;

using System.Collections.Generic;
using MEC;

public static class CoroutineManager
{
    public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

    public static CoroutineHandle StartCoroutine(IEnumerator<float> coroutine)
    {
        CoroutineHandle handle = Timing.RunCoroutine(coroutine);
        Coroutines.Add(handle);
        return handle;
    }

    public static void StopCoroutine(CoroutineHandle handle)
    {
        Timing.KillCoroutines(handle);
        Coroutines.Remove(handle);
    }

    public static void StopAllCoroutines()
    {
        foreach (CoroutineHandle coroutine in Coroutines) Timing.KillCoroutines(coroutine);
        Coroutines.Clear();
    }
}
