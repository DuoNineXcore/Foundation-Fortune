namespace FoundationFortune.API;

using System.Collections.Generic;
using MEC;

public static class CoroutineManager
{
    public static List<CoroutineHandle> Coroutines = new List<CoroutineHandle>();

    /// <summary>
    /// Starts a coroutine and returns a handle to control its execution.
    /// </summary>
    /// <param name="coroutine">The coroutine to be executed.</param>
    /// <returns>A handle to control the execution of the coroutine.</returns>
    public static CoroutineHandle StartCoroutine(IEnumerator<float> coroutine)
    {
        CoroutineHandle handle = Timing.RunCoroutine(coroutine);
        Coroutines.Add(handle);
        return handle;
    }

    /// <summary>
    /// Stops the specified coroutine using its handle.
    /// </summary>
    /// <param name="handle">The handle of the coroutine to be stopped.</param>
    public static void StopCoroutine(CoroutineHandle handle)
    {
        Timing.KillCoroutines(handle);
        Coroutines.Remove(handle);
    }

    /// <summary>
    /// Stops all running coroutines and clears the list of coroutine handles.
    /// </summary>
    public static void StopAllCoroutines()
    {
        foreach (CoroutineHandle coroutine in Coroutines) Timing.KillCoroutines(coroutine);
        Coroutines.Clear();
    }
}
