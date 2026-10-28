using System;
using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Tiny coroutine based tween helpers. Run them with <c>StartCoroutine(Tween.Value(...))</c>
    /// on any MonoBehaviour; stopping the coroutine cancels the tween.
    /// </summary>
    public static class Tween
    {
        /// <summary>Interpolates a float from <paramref name="from"/> to <paramref name="to"/> over <paramref name="duration"/> seconds.</summary>
        public static IEnumerator Value(float from, float to, float duration, Action<float> onUpdate,
                                        Ease ease = Ease.Linear, float delay = 0f, Action onComplete = null)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float k = Easing.Evaluate(ease, time / duration);
                onUpdate?.Invoke(Mathf.LerpUnclamped(from, to, k));
                yield return null;
            }

            onUpdate?.Invoke(to);
            onComplete?.Invoke();
        }

        /// <summary>Waits for <paramref name="seconds"/> of scaled time, then invokes the callback.</summary>
        public static IEnumerator Wait(float seconds, Action onComplete)
        {
            yield return new WaitForSeconds(seconds);
            onComplete?.Invoke();
        }
    }
}
