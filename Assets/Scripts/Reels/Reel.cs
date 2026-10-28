using System;
using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// A single reel modelled as a vertical cylinder of tiles. The visible window shows
    /// <see cref="Rows"/> consecutive symbols of the reel strip.
    ///
    /// The reel keeps one continuous number, <c>scroll</c>: the (fractional) strip position of the
    /// bottom row. Every tile owns an absolute strip index and is placed on the cylinder from the
    /// difference to <c>scroll</c>, so spinning is just animating that single number.
    /// </summary>
    public class Reel : MonoBehaviour
    {
        [SerializeField, Tooltip("Row anchors of the visible window, from top to bottom")]
        private Transform[] rows;
        [SerializeField, Tooltip("Vertical size of one tile")]
        private float tileSizeY = 2.4f;
        [SerializeField, Tooltip("Extra space between two tiles")]
        private float gapY;

        private SlotConfig config;
        private ReelStrip strip;
        private SlotTile[] tiles;
        private Transform tilesRoot;
        private float anglePerTile;
        private float radius;
        private float baseAngle;       // angle of the bottom row tile when the reel is at rest
        private float scroll;          // fractional strip position of the bottom row
        private float lastScroll;
        private float spacingWorld;    // world distance between two neighbouring tiles
        private Coroutine spinRoutine;

        public int Rows { get { return rows.Length; } }

        /// <summary>Strip position (0..strip length-1) of the symbol in the bottom row.</summary>
        public int CurrentOrderPosition { get; private set; }
        public bool IsSpinning { get { return spinRoutine != null; } }
        public Transform RowAnchor(int row) { return rows[row]; }

        #region build
        public void Build(SlotConfig slotConfig, int reelIndex, SlotTile tilePrefab)
        {
            config = slotConfig;
            strip = config.reels[reelIndex];

            int tileCount = config.tilesPerReel;
            anglePerTile = 360f / tileCount;
            float anglePerTileRad = anglePerTile * Mathf.Deg2Rad;
            radius = ((tileSizeY + gapY) * 0.5f) / Mathf.Tan(anglePerTileRad * 0.5f);

            tilesRoot = new GameObject("Reel(" + name + ")").transform;
            tilesRoot.SetParent(transform, false);
            tilesRoot.localPosition = new Vector3(0f, 0f, radius); // cylinder axis

            // rest angle: put the bottom row's tile exactly where the bottom row anchor is
            Transform bottomRow = rows[rows.Length - 1];
            float bottomY = transform.InverseTransformPoint(bottomRow.position).y;
            baseAngle = Mathf.Abs(bottomY) < radius ? Mathf.Asin(bottomY / radius) * Mathf.Rad2Deg : 0f;

            tiles = new SlotTile[tileCount];
            for (int i = 0; i < tileCount; i++)
            {
                SlotTile tile = Instantiate(tilePrefab, tilesRoot);
                tile.name = "SlotTile " + i.ToString("00");
                tile.StripIndex = i;
                tiles[i] = tile;
            }

            spacingWorld = (tileSizeY + gapY) * transform.lossyScale.y;
            CurrentOrderPosition = strip.randomStart ? GameServices.Random.Range(0, strip.Length) : 0;
            SetScroll(CurrentOrderPosition, false);
        }
        #endregion build

        #region tile placement
        /// <summary>Moves the reel to a fractional strip position and refreshes tile positions and symbols.</summary>
        private void SetScroll(float value, bool allowBlur)
        {
            float speed = Mathf.Abs(value - lastScroll) * spacingWorld / Mathf.Max(Time.deltaTime, 0.0001f);
            bool blur = allowBlur && speed > 10f;
            lastScroll = scroll = value;

            int tileCount = tiles.Length;
            // tiles whose angle falls outside this window are on the back of the cylinder and get recycled
            float minOffset = (-180f - baseAngle) / anglePerTile;
            float maxOffset = minOffset + tileCount;

            for (int i = 0; i < tileCount; i++)
            {
                SlotTile tile = tiles[i];
                float offset = tile.StripIndex - scroll;
                if (offset < minOffset || offset >= maxOffset)
                {
                    int laps = Mathf.FloorToInt((offset - minOffset) / tileCount);
                    tile.StripIndex -= laps * tileCount;
                    offset = tile.StripIndex - scroll;
                }

                int id = strip.order[Mod(tile.StripIndex, strip.Length)];
                if (tile.SymbolId != id) tile.SetSymbol(id, config.symbols[id]);
                tile.SetBlur(blur);

                float angle = baseAngle + offset * anglePerTile;
                float rad = angle * Mathf.Deg2Rad;
                Transform t = tile.transform;
                t.localPosition = new Vector3(0f, radius * Mathf.Sin(rad), -radius * Mathf.Cos(rad));
                t.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }
        }

        private static int Mod(int value, int length)
        {
            int m = value % length;
            return m < 0 ? m + length : m;
        }
        #endregion tile placement

        #region reading the result
        /// <summary>Tile currently sitting in the given window row (0 = top). Only valid when the reel is at rest.</summary>
        public SlotTile GetTile(int row)
        {
            int stripIndex = Mathf.RoundToInt(scroll) + (rows.Length - 1 - row);
            foreach (SlotTile tile in tiles)
                if (tile.StripIndex == stripIndex) return tile;
            return null;
        }

        public int GetSymbolId(int row)
        {
            int stripIndex = CurrentOrderPosition + (rows.Length - 1 - row);
            return strip.order[Mod(stripIndex, strip.Length)];
        }

        /// <summary>Random strip position to stop on, so the outcome is decided before the reel moves.</summary>
        public int PickRandomStop()
        {
            return GameServices.Random.Range(0, strip.Length);
        }
        #endregion reading the result

        #region spinning
        /// <summary>
        /// Spins the reel and stops it with <paramref name="stopPosition"/> in the bottom row.
        /// The run has three parts: a short wind-back, the main run, and a small overshoot that settles.
        /// </summary>
        public void Spin(int stopPosition, Action onStopped)
        {
            if (spinRoutine != null) StopCoroutine(spinRoutine);
            spinRoutine = StartCoroutine(SpinRoutine(stopPosition, onStopped));
        }

        private IEnumerator SpinRoutine(int stopPosition, Action onStopped)
        {
            float inAngle = Mathf.Clamp(config.inRotAngle, 0f, 10f);
            float outAngle = Mathf.Clamp(config.outRotAngle, 0f, 10f);
            float inTime = Mathf.Clamp(config.inRotTime, 0f, 1f);
            float outTime = Mathf.Clamp(config.outRotTime, 0f, 1f);
            float random = Mathf.Clamp(config.mainRotateTimeRandomize, 0, 20) / 100f;
            float mainTime = GameServices.Random.Range(config.mainRotateTime * (1f - random), config.mainRotateTime * (1f + random));

            // strip positions to travel: up to the target symbol plus some full laps
            int toTarget = stopPosition > CurrentOrderPosition
                ? stopPosition - CurrentOrderPosition
                : strip.Length - CurrentOrderPosition + stopPosition;
            int total = toTarget + strip.Length * strip.laps;
            float start = scroll;
            float inUnits = inAngle / anglePerTile;
            float outUnits = outAngle / anglePerTile;
            float finish = start + total;

            // 1. wind back (the reel moves up a little)
            yield return Tween.Value(0f, inUnits, inTime, v => SetScroll(start - v, true), Ease.Linear, strip.spinStartDelay);
            // 2. main run, overshooting the target by the out angle
            yield return Tween.Value(start - inUnits, finish + outUnits, mainTime, v => SetScroll(v, true));
            // 3. settle back onto the target
            yield return Tween.Value(finish + outUnits, finish, outTime, v => SetScroll(v, true));

            SetScroll(finish, false);
            CurrentOrderPosition = Mod(stopPosition, strip.Length);
            spinRoutine = null;
            onStopped?.Invoke();
        }

        private void OnDisable()
        {
            if (spinRoutine != null) StopCoroutine(spinRoutine);
            spinRoutine = null;
        }
        #endregion spinning
    }
}
