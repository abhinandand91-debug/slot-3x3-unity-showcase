using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Shows a win to the player: sparkles on the winning symbols right away, then walks through the
    /// winning lines one by one (line flashes, symbols pulse) and keeps looping until the next spin.
    /// </summary>
    public class WinPresenter : MonoBehaviour
    {
        [SerializeField] private LinesController lines;
        [SerializeField] private GameObject starParticles;
        [SerializeField, Tooltip("Seconds each winning line stays highlighted (at least 3)")]
        private float lineShowTime = 5f;
        [SerializeField, Tooltip("Pulses each winning symbol plays while its line is shown")]
        private int pulses = 3;

        private WinEvaluator evaluator;
        private Coroutine sequence;
        private readonly List<SlotTile> sparkled = new List<SlotTile>();

        public void Init(WinEvaluator winEvaluator)
        {
            evaluator = winEvaluator;
        }

        private void OnValidate()
        {
            lineShowTime = Mathf.Max(3f, lineShowTime);
        }

        /// <summary>Sparkles on every winning symbol and, optionally, all winning lines shown at once.</summary>
        public void ShowEffects(bool flashAllLines, bool symbolParticles)
        {
            lines.HideAll();

            foreach (PaylineView line in lines.Lines)
            {
                if (!line.IsWinning) continue;
                line.SetVisible(flashAllLines);
                line.Flash(flashAllLines);
                if (symbolParticles) Sparkle(line.Win.Tiles);
            }
            if (symbolParticles)
            {
                Sparkle(evaluator.ScatterTiles);
                Sparkle(evaluator.JackpotTiles);
            }
        }

        private void Sparkle(List<SlotTile> tiles)
        {
            foreach (SlotTile tile in tiles)
            {
                tile.ShowParticles(true, starParticles);
                sparkled.Add(tile);
            }
        }

        /// <summary>
        /// Highlights each winning line in turn. <paramref name="onFirstPassDone"/> fires once every win
        /// has been shown; after that the sequence repeats until <see cref="Cancel"/> is called.
        /// </summary>
        public void ShowSequence(bool flashLines, Action onFirstPassDone)
        {
            Cancel(false);
            sequence = StartCoroutine(SequenceRoutine(flashLines, onFirstPassDone));
        }

        private IEnumerator SequenceRoutine(bool flashLines, Action onFirstPassDone)
        {
            bool firstPass = true;
            while (true)
            {
                foreach (PaylineView line in lines.Lines)
                {
                    if (!line.IsWinning) continue;
                    if (flashLines) { line.SetVisible(true); line.Flash(true); }
                    PulseTiles(line.Win.Tiles);
                    yield return new WaitForSeconds(lineShowTime);
                    StopPulse(line.Win.Tiles);
                    if (flashLines) { line.Flash(false); line.SetVisible(false); }
                }

                if (evaluator.ScatterTiles.Count > 0)
                {
                    PulseTiles(evaluator.ScatterTiles);
                    yield return new WaitForSeconds(lineShowTime);
                    StopPulse(evaluator.ScatterTiles);
                }

                if (evaluator.JackpotTiles.Count > 0)
                {
                    PulseTiles(evaluator.JackpotTiles);
                    yield return new WaitForSeconds(lineShowTime);
                    StopPulse(evaluator.JackpotTiles);
                }

                if (firstPass)
                {
                    firstPass = false;
                    onFirstPassDone?.Invoke();
                }
                yield return null;
            }
        }

        private void PulseTiles(List<SlotTile> tiles)
        {
            foreach (SlotTile tile in tiles) tile.PlayWin(pulses);
        }

        private static void StopPulse(List<SlotTile> tiles)
        {
            foreach (SlotTile tile in tiles) tile.StopWin();
        }

        /// <summary>Stops the presentation and clears sparkles, pulses and line highlights.</summary>
        public void Cancel(bool clearSparkles = true)
        {
            if (sequence != null) StopCoroutine(sequence);
            sequence = null;

            foreach (PaylineView line in lines.Lines)
            {
                if (line.IsWinning) StopPulse(line.Win.Tiles);
                line.Flash(false);
            }
            StopPulse(evaluator.ScatterTiles);
            StopPulse(evaluator.JackpotTiles);

            if (clearSparkles)
            {
                foreach (SlotTile tile in sparkled)
                    if (tile != null) tile.ShowParticles(false, null);
                sparkled.Clear();
            }
        }
    }
}
