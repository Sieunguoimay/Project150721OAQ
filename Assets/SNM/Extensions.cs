using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif
using UnityEngine;

namespace SNM
{
    public static class Extensions
    {
        public static Coroutine Delay(this MonoBehaviour mb, float duration, Action onDone)
        {
            return mb.StartCoroutine(Delay(duration, onDone));
        }

        private static IEnumerator Delay(float duration, Action onDone)
        {
            yield return new WaitForSeconds(duration);
            onDone?.Invoke();
        }

        public static Coroutine TimingForLoop(this MonoBehaviour mb, float duration, int n, Action<int> onIteration)
        {
            if (n <= 0) return null;
            if (n == 1)
            {
                onIteration?.Invoke(0);
                return null;
            }

            var index = 0;
            return mb.TimeProgress(duration, p =>
            {
                if (!(p >= index * (1f / (n - 1)))) return;

                onIteration?.Invoke(index);
                index++;
            });
        }

        public static Coroutine TimeProgress(this MonoBehaviour mb, float duration, Action<float> onProgress)
            => mb.StartCoroutine(TimeProgress(duration, onProgress));

        private static IEnumerator TimeProgress(float duration, Action<float> onProgress)
        {
            var time = 0f;
            while (time < duration)
            {
                onProgress?.Invoke(time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            onProgress?.Invoke(1f);
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
#if UNITY_EDITOR

        public static AnimatorController GetAnimatorController(this Animator animator)
        {
            if (animator.runtimeAnimatorController is AnimatorController ac)
            {
                return ac;
            }

            if (animator.runtimeAnimatorController is AnimatorOverrideController aoc)
            {
                return aoc.runtimeAnimatorController as AnimatorController;
            }

            return null;
        }
#endif
    }
}