using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Draws a payline as a chain of sprite segments with a round dot at every joint.
    /// It can be shown, flashed (fading in and out) or "burned" (shown briefly).
    /// </summary>
    public class LineStroke : MonoBehaviour
    {
        private const float PixelsPerUnit = 100f;
        private const int BurnFrames = 15;

        private readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
        private Coroutine flashRoutine;
        private Coroutine burnRoutine;

        /// <summary>Builds the segments through the given world positions.</summary>
        public void Build(IList<Vector3> points, Sprite lineSprite, Sprite dotSprite, int sortingOrder, Material material)
        {
            Texture2D lineTexture = lineSprite.texture;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 direction = points[i + 1] - points[i];
                float length = direction.magnitude;
                float widthPixels = Mathf.Min(length * PixelsPerUnit, lineTexture.width);
                Sprite segment = Sprite.Create(lineTexture, new Rect(0, 0, widthPixels, lineTexture.height),
                                               new Vector2(0f, 0.5f), PixelsPerUnit);
                parts.Add(CreatePart("Segment " + i, segment, points[i], sortingOrder, material,
                                     Quaternion.FromToRotation(Vector3.right, direction)));
            }

            for (int i = 0; i < points.Count; i++)
                parts.Add(CreatePart("Dot " + i, dotSprite, points[i], sortingOrder + 1, material, Quaternion.identity));

            SetVisible(false);
        }

        private SpriteRenderer CreatePart(string partName, Sprite sprite, Vector3 position, int sortingOrder,
                                          Material material, Quaternion rotation)
        {
            var go = new GameObject(partName);
            go.transform.SetParent(transform, true);
            go.transform.position = position;
            go.transform.rotation = rotation;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            if (material != null) sr.sharedMaterial = material;
            return sr;
        }

        public void SetVisible(bool visible)
        {
            foreach (SpriteRenderer part in parts)
                if (part != null) part.gameObject.SetActive(visible);
        }

        /// <summary>Fades the line in and out in a loop until switched off.</summary>
        public void Flash(bool on)
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = null;
            SetAlpha(1f);
            if (on) flashRoutine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            float time = 0f;
            while (true)
            {
                time += Time.deltaTime;
                SetAlpha(0.5f * (Mathf.Cos(time * Mathf.PI * 2f) + 1f));
                yield return null;
            }
        }

        /// <summary>Shows the line for a short moment, e.g. when its selector button is pressed.</summary>
        public void Burn(float delay)
        {
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            SetVisible(false);
            burnRoutine = StartCoroutine(BurnRoutine(delay));
        }

        public void CancelBurn()
        {
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            SetVisible(false);
        }

        private IEnumerator BurnRoutine(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            SetVisible(true);
            for (int i = 0; i < BurnFrames; i++) yield return null;
            SetVisible(false);
            burnRoutine = null;
        }

        private void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer part in parts)
            {
                if (part == null) continue;
                Color c = part.color;
                c.a = alpha;
                part.color = c;
            }
        }
    }
}
