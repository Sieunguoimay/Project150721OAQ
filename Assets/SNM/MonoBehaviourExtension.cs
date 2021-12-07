using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityExtension
{
    public static void Delay(this MonoBehaviour mb, float duration, Action onDone)
    {
        mb.StartCoroutine(Delay(duration, onDone));
    }

    private static IEnumerator Delay(float duration, Action onDone)
    {
        yield return new WaitForSeconds(duration);
        onDone?.Invoke();
    }

    public static void ExecuteInNextFrame(this MonoBehaviour mb, Action onDone)
    {
        mb.StartCoroutine(ExecuteInNextFrame(onDone));
    }

    private static IEnumerator ExecuteInNextFrame(Action onDone)
    {
        yield return null;
        onDone?.Invoke();
    }
}