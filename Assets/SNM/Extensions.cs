using System;
using System.Collections;
using UnityEngine;

namespace SNM
{
    public static class Extensions
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

        public static Coroutine WaitUntil(this MonoBehaviour context, Func<bool> condition, Action callback)
        {
            return context.StartCoroutine(WaitUntil(condition, callback));
        }

        private static IEnumerator WaitUntil(Func<bool> condition, Action callback)
        {
            yield return new WaitUntil(condition);
            callback?.Invoke();
        }
    }
}