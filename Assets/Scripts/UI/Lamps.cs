using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    public enum LampsMode { Random, All, Sequence, AllOn, AllOff }

    /// <summary>
    /// The marquee light bulbs around the machine. Each bulb is a sprite that is switched on with a
    /// quick fade in and out; the mode decides the pattern (random twinkle, chase sequence, ...).
    /// </summary>
    public class Lamps : MonoBehaviour
    {
        [SerializeField] private List<SpriteRenderer> bulbs = new List<SpriteRenderer>();
        [SerializeField] private LampsMode mode = LampsMode.Random;

        private int litCount;
        private LampsMode shownMode = LampsMode.AllOff;
        private readonly List<Coroutine> fades = new List<Coroutine>();

        public LampsMode Mode { get { return mode; } set { mode = value; } }

        public void Setup(List<SpriteRenderer> lampSprites)
        {
            bulbs = lampSprites;
        }

        private void Start()
        {
            SwitchOffAll();
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (true)
            {
                if (shownMode != mode)
                {
                    SwitchOffAll();
                    yield return new WaitForSeconds(0.02f);
                }

                switch (mode)
                {
                    case LampsMode.Random:
                        shownMode = mode;
                        if (litCount < 5) Light(Random.Range(0, bulbs.Count - 1), Random.Range(1, 4));
                        yield return new WaitForSeconds(0.05f);
                        break;

                    case LampsMode.All:
                        shownMode = mode;
                        for (int i = 0; i < bulbs.Count; i++) Light(i, 1f);
                        yield return new WaitForSeconds(1.5f);
                        break;

                    case LampsMode.Sequence:
                        shownMode = mode;
                        for (int i = 0; i < bulbs.Count; i++)
                        {
                            Light(i, 0.3f);
                            yield return new WaitForSeconds(0.05f);
                        }
                        yield return new WaitForSeconds(0.05f);
                        break;

                    case LampsMode.AllOn:
                        if (shownMode != mode) SwitchOnAll();
                        shownMode = mode;
                        yield return new WaitForSeconds(0.05f);
                        break;

                    default:
                        shownMode = mode;
                        yield return new WaitForSeconds(0.05f);
                        break;
                }
            }
        }

        private void Light(int index, float duration)
        {
            if (index < 0 || index >= bulbs.Count || bulbs[index].gameObject.activeSelf) return;
            fades.Add(StartCoroutine(FadeBulb(bulbs[index], duration)));
        }

        /// <summary>Fades a bulb in, keeps it lit, then fades it out and switches it off.</summary>
        private IEnumerator FadeBulb(SpriteRenderer bulb, float duration)
        {
            const float fadeShare = 0.1f;
            float fadeTime = fadeShare * duration;
            float total = duration + 2f * fadeTime;

            bulb.gameObject.SetActive(true);
            litCount++;

            float time = 0f;
            while (time < total)
            {
                time += Time.deltaTime;
                float progress = time / total;
                float alpha = progress <= fadeShare ? progress / fadeShare : (1f - progress) / fadeShare;
                if (bulb != null) bulb.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                yield return null;
            }

            if (bulb != null)
            {
                bulb.color = new Color(1f, 1f, 1f, 0f);
                bulb.gameObject.SetActive(false);
            }
            litCount--;
        }

        private void SwitchOffAll()
        {
            StopFades();
            foreach (SpriteRenderer bulb in bulbs)
            {
                bulb.color = new Color(1f, 1f, 1f, 0f);
                bulb.gameObject.SetActive(false);
            }
        }

        private void SwitchOnAll()
        {
            StopFades();
            foreach (SpriteRenderer bulb in bulbs)
            {
                bulb.color = Color.white;
                bulb.gameObject.SetActive(true);
            }
        }

        private void StopFades()
        {
            foreach (Coroutine fade in fades)
                if (fade != null) StopCoroutine(fade);
            fades.Clear();
            litCount = 0;
        }
    }
}
