using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// One face of a reel cylinder. It shows a symbol sprite (or its blurred twin while the reel
    /// is moving fast) and knows how to play the "this tile is part of a win" feedback.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SlotTile : MonoBehaviour
    {
        private const float PulseScale = 1.1f;
        private const float PulseHalfTime = 0.15f;

        private SpriteRenderer spriteRenderer;
        private SymbolDef symbol;
        private bool blurred;
        private GameObject particles;
        private Coroutine pulse;

        /// <summary>Index of the symbol on the SlotConfig.symbols list.</summary>
        public int SymbolId { get; private set; } = -1;

        /// <summary>Absolute position of this tile on the reel strip (used by the reel to place it).</summary>
        public int StripIndex { get; set; }

        public Sprite CurrentSprite { get { return symbol != null ? symbol.sprite : null; } }
        public int SortingOrder { get { return spriteRenderer.sortingOrder; } }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSymbol(int id, SymbolDef def)
        {
            SymbolId = id;
            symbol = def;
            Refresh();
        }

        public void SetBlur(bool value)
        {
            if (blurred == value) return;
            blurred = value;
            Refresh();
        }

        private void Refresh()
        {
            if (symbol == null) return;
            spriteRenderer.sprite = blurred && symbol.blur != null ? symbol.blur : symbol.sprite;
        }

        #region win feedback
        public void ShowParticles(bool show, GameObject particlesPrefab)
        {
            if (show)
            {
                if (particles != null || particlesPrefab == null) return;
                particles = Instantiate(particlesPrefab, transform.position, transform.rotation, transform.parent);
                particles.transform.localScale = transform.localScale;
            }
            else if (particles != null)
            {
                Destroy(particles);
                particles = null;
            }
        }

        /// <summary>Pulses the tile a few times to draw the eye to it.</summary>
        public void PlayWin(int pulses = 3)
        {
            StopWin();
            pulse = StartCoroutine(PulseRoutine(pulses));
        }

        public void StopWin()
        {
            if (pulse != null) StopCoroutine(pulse);
            pulse = null;
            transform.localScale = Vector3.one;
        }

        private IEnumerator PulseRoutine(int pulses)
        {
            for (int i = 0; i < pulses; i++)
            {
                yield return Tween.Value(1f, PulseScale, PulseHalfTime, SetUniformScale, Ease.InSine);
                yield return Tween.Value(PulseScale, 1f, PulseHalfTime, SetUniformScale, Ease.InSine);
            }
            transform.localScale = Vector3.one;
            pulse = null;
        }

        private void SetUniformScale(float s)
        {
            transform.localScale = new Vector3(s, s, 1f);
        }
        #endregion win feedback

        private void OnDisable()
        {
            StopWin();
        }
    }
}
