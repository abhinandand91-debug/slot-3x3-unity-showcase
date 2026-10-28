using System.IO;
using UnityEditor;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// Placeholder art drawn entirely from code (no licensed images). Every sprite the scene builder asks for
    /// lives in <see cref="Folder"/> as a PNG that this class painted. The generated PNGs are committed, so the
    /// layout never changes; delete one and it is drawn again on the next scene build.
    /// </summary>
    public static class PlaceholderArt
    {
        public const string Folder = "Assets/Art/Generated/";

        // size used when a sprite has never been generated before
        private const int DefaultSize = 128;

        // palette: charcoal and teal with amber accents
        private static readonly Color Ink = new Color(0.08f, 0.10f, 0.14f);
        private static readonly Color Slate = new Color(0.16f, 0.20f, 0.27f);
        private static readonly Color Teal = new Color(0.10f, 0.62f, 0.66f);
        private static readonly Color TealLight = new Color(0.30f, 0.85f, 0.88f);
        private static readonly Color Amber = new Color(0.98f, 0.70f, 0.15f);
        private static readonly Color Coral = new Color(0.90f, 0.32f, 0.30f);
        private static readonly Color Green = new Color(0.25f, 0.75f, 0.35f);
        private static readonly Color Paper = new Color(0.90f, 0.92f, 0.95f);

        /// <summary>
        /// Returns the placeholder sprite for a logical art path such as "Assets/Art/Hud/Star GUI.png".
        /// Only the file name matters: it picks the generated PNG and tells <see cref="Draw"/> what to paint.
        /// </summary>
        public static Sprite Resolve(string artPath)
        {
            string file = Path.GetFileName(artPath);

            // slot symbols have their own painter
            if (artPath.Contains("/Symbols/")) return SymbolArt.Get(file);

            string path = Folder + file;
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            Debug.LogWarning("Drawing new placeholder " + file + " at the default size of " + DefaultSize + " px");
            return SaveSprite(path, Draw(file.ToLowerInvariant(), DefaultSize, DefaultSize), DefaultSize, DefaultSize, 100f);
        }

        /// <summary>Writes pixels to a PNG and imports it as a single, centred sprite.</summary>
        public static Sprite SaveSprite(string path, Color32[] pixels, int w, int h, float pixelsPerUnit)
        {
            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>A dark display plate of the given world size (in units at 100 pixels per unit), used behind readouts.</summary>
        public static Sprite Plate(float widthUnits, float heightUnits)
        {
            int w = Mathf.RoundToInt(widthUnits * 100f), h = Mathf.RoundToInt(heightUnits * 100f);
            string path = Folder + "Plate_" + w + "x" + h + ".png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            Color32[] pixels = Rounded(w, h, h * 0.3f, 0f, new Color(0.04f, 0.07f, 0.10f), new Color(0.10f, 0.15f, 0.20f), Teal, 4);
            return SaveSprite(path, pixels, w, h, 100f);
        }

        #region what to draw
        private static Color32[] Draw(string file, int w, int h)
        {
            bool hover = file.Contains("hover") || file.Contains("pressed");

            if (file.StartsWith("background")) return Gradient(w, h, new Color(0.05f, 0.17f, 0.24f), new Color(0.02f, 0.06f, 0.10f), true);
            if (file.StartsWith("slot machine") && file.Contains("lamps")) return MarqueeStrip(w, h);
            if (file.StartsWith("slot machine")) return Rounded(w, h, Mathf.Min(w, h) * 0.08f, 0f, Slate, Ink, Teal, 14);
            if (file.StartsWith("machine background")) return Rounded(w, h, 30f, 0f, new Color(0.07f, 0.12f, 0.18f), new Color(0.04f, 0.07f, 0.11f), Teal, 4);
            if (file.StartsWith("reel violet")) return Rounded(w, h, 40f, 0f, Paper, new Color(0.62f, 0.66f, 0.72f), Slate, 6);
            if (file.StartsWith("reelmask")) return Rounded(w, h, 40f, 0f, Color.white, Color.white, Color.white, 0);

            if (file.StartsWith("lamp light pop")) return Glow(w, h, new Color(1f, 0.85f, 0.35f));
            if (file.StartsWith("lamp light")) return Bulb(w, h);
            if (file.StartsWith("part_star")) return Glow(w, h, Color.white);
            if (file.StartsWith("line-circle")) return Disc(w, h, Color.white, 0.5f);
            if (file.StartsWith("line.png")) return Rounded(w, h, h * 0.5f, 0f, Color.white, Color.white, Color.white, 0);

            if (file.StartsWith("coin spin")) return SpinningCoin(w, h, file);
            if (file.StartsWith("coin-bigwin")) return Coin(w, h, 1f);
            if (file.StartsWith("win pop up coins fly")) return Scatter(w, h);
            if (file.StartsWith("win pop up coins stack")) return Stack(w, h);

            if (file.StartsWith("hold button red")) return Button(w, h, hover ? new Color(1f, 0.55f, 0.45f) : Coral);
            if (file.StartsWith("line botton"))
                return Rounded(w, h, h * 0.2f, h * 0.24f, Color.Lerp(hover ? TealLight : Teal, Color.white, 0.3f), Color.Lerp(hover ? TealLight : Teal, Color.black, 0.35f), Ink, 3);
            if (file.StartsWith("hold button") || file.StartsWith("machine button"))
                return Button(w, h, hover ? TealLight : Teal);

            if (file.StartsWith("bottom gui plate")) return Rounded(w, h, 20f, 0f, Ink, Slate, Teal, 4);
            if (file.StartsWith("progress bar empty")) return ProgressSlot(w, h);
            if (file.StartsWith("progress bar fulltrim")) return Rounded(w, h, h * 0.4f, 0f, Green, new Color(0.15f, 0.55f, 0.25f), Green, 0);
            if (file.StartsWith("progress bar spot")) return Glow(w, h, Color.white);
            if (file.StartsWith("star gui")) return Disc(w, h, Amber, 1f);
            if (file.StartsWith("icon chip")) return Chip(w, h);
            if (file.StartsWith("button menu")) return Menu(w, h, hover: false);
            if (file.StartsWith("button shop") || file.StartsWith("button big") || file.StartsWith("button square"))
                return Button(w, h, hover ? TealLight : Teal);
            if (file.StartsWith("button exit")) return ExitButton(w, h, hover);
            if (file.StartsWith("toggle button on")) return Toggle(w, h, true);
            if (file.StartsWith("toggle button off")) return Toggle(w, h, false);

            if (file.StartsWith("graybkg")) return Fill(w, h, new Color(0f, 0f, 0f, 0.8f));
            if (file.StartsWith("round pop up plate")) return Rounded(w, h, 80f, 0f, new Color(0.12f, 0.20f, 0.30f), Ink, Amber, 10);
            if (file.StartsWith("pop up")) return Rounded(w, h, 40f, 0f, new Color(0.14f, 0.17f, 0.24f), new Color(0.07f, 0.09f, 0.13f), Teal, 8);
            if (file.StartsWith("lamp light pop")) return Glow(w, h, new Color(1f, 0.85f, 0.35f));

            // anything unexpected still gets a visible placeholder
            return Rounded(w, h, 12f, 0f, Slate, Ink, Amber, 3);
        }
        #endregion what to draw

        #region drawing primitives
        private static Color32[] Fill(int w, int h, Color color)
        {
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = color;
            return px;
        }

        private static Color32[] Gradient(int w, int h, Color top, Color bottom, bool dots)
        {
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                Color row = Color.Lerp(bottom, top, y / (float)(h - 1));
                for (int x = 0; x < w; x++) px[y * w + x] = row;
            }
            if (!dots) return px;

            // a few soft discs so the backdrop is not flat
            var rng = new System.Random(7);
            for (int d = 0; d < 14; d++)
            {
                float cx = (float)rng.NextDouble() * w, cy = (float)rng.NextDouble() * h, r = (0.05f + (float)rng.NextDouble() * 0.12f) * h;
                for (int y = Mathf.Max(0, (int)(cy - r)); y < Mathf.Min(h, (int)(cy + r)); y++)
                    for (int x = Mathf.Max(0, (int)(cx - r)); x < Mathf.Min(w, (int)(cx + r)); x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) / r;
                        if (dist >= 1f) continue;
                        Color c = px[y * w + x];
                        px[y * w + x] = Color.Lerp(c, new Color(0.15f, 0.45f, 0.55f), 0.18f * (1f - dist));
                    }
            }
            return px;
        }

        /// <summary>Rounded rectangle with a vertical gradient and an optional border; anti-aliased.</summary>
        internal static Color32[] Rounded(int w, int h, float radius, float margin, Color top, Color bottom, Color border, int borderWidth)
        {
            var px = new Color32[w * h];
            radius = Mathf.Min(radius, Mathf.Min(w, h) * 0.5f);
            Vector2 half = new Vector2(w * 0.5f - margin, h * 0.5f - margin);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    Vector2 p = new Vector2(x + 0.5f - w * 0.5f, y + 0.5f - h * 0.5f);
                    float d = RoundedBoxDistance(p, half, radius);
                    float alpha = Mathf.Clamp01(0.5f - d);
                    if (alpha <= 0f) { px[y * w + x] = new Color32(0, 0, 0, 0); continue; }

                    Color fill = Color.Lerp(bottom, top, y / (float)(h - 1));
                    if (borderWidth > 0 && d > -borderWidth) fill = border;
                    fill.a = alpha;
                    px[y * w + x] = fill;
                }
            return px;
        }

        private static float RoundedBoxDistance(Vector2 p, Vector2 half, float radius)
        {
            Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - (half - Vector2.one * radius);
            return Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
        }

        internal static Color32[] Disc(int w, int h, Color color, float fill)
        {
            var px = new Color32[w * h];
            float r = Mathf.Min(w, h) * 0.5f * fill;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(w * 0.5f, h * 0.5f));
                    Color c = color;
                    c.a = Mathf.Clamp01(r - d + 0.5f);
                    px[y * w + x] = c;
                }
            return px;
        }

        internal static Color32[] Glow(int w, int h, Color color)
        {
            var px = new Color32[w * h];
            float r = Mathf.Min(w, h) * 0.5f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(w * 0.5f, h * 0.5f)) / r;
                    Color c = color;
                    c.a = Mathf.Clamp01(1f - d) * Mathf.Clamp01(1f - d) * 1.4f;
                    c.a = Mathf.Clamp01(c.a);
                    px[y * w + x] = c;
                }
            return px;
        }

        /// <summary>A slim dark strip that carries a row of marquee bulbs.</summary>
        private static Color32[] MarqueeStrip(int w, int h)
        {
            var px = new Color32[w * h];
            int barH = Mathf.RoundToInt(h * 0.34f), y0 = (h - barH) / 2 + Mathf.RoundToInt(h * 0.1f); // bulbs sit a little above the centre
            Color32[] bar = Rounded(w, barH, barH * 0.5f, 0f, new Color(0.14f, 0.18f, 0.25f), new Color(0.05f, 0.07f, 0.11f), Amber, 3);
            Blit(px, w, h, bar, w, barH, 0, y0);
            return px;
        }

        /// <summary>A bright marquee bulb: hot core with a soft halo.</summary>
        private static Color32[] Bulb(int w, int h)
        {
            var px = new Color32[w * h];
            float halo = Mathf.Min(w, h) * 0.30f, core = Mathf.Min(w, h) * 0.12f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(w * 0.5f, h * 0.5f));
                    float a = d < core ? 1f : Mathf.Clamp01(1f - (d - core) / (halo - core)) * 0.55f;
                    Color c = Color.Lerp(new Color(1f, 0.8f, 0.3f), Color.white, d < core ? 0.7f : 0f);
                    c.a = a;
                    px[y * w + x] = c;
                }
            return px;
        }

        private static Color32[] Button(int w, int h, Color baseColor)
        {
            Color top = Color.Lerp(baseColor, Color.white, 0.35f);
            Color bottom = Color.Lerp(baseColor, Color.black, 0.35f);
            return Rounded(w, h, Mathf.Min(w, h) * 0.28f, 3f, top, bottom, Ink, 4);
        }

        private static Color32[] ProgressSlot(int w, int h)
        {
            Color32[] px = Rounded(w, h, h * 0.3f, 0f, Slate, Ink, Teal, 5);
            // light inner slot where the fill bar travels
            DrawRect(px, w, h, 78, h - 107, 470, 58, Paper);
            return px;
        }

        internal static void DrawRect(Color32[] px, int w, int h, int x0, int y0, int rw, int rh, Color color)
        {
            for (int y = Mathf.Max(0, y0); y < Mathf.Min(h, y0 + rh); y++)
                for (int x = Mathf.Max(0, x0); x < Mathf.Min(w, x0 + rw); x++)
                    px[y * w + x] = color;
        }

        private static Color32[] Chip(int w, int h)
        {
            Color32[] px = Disc(w, h, Coral, 1f);
            Color32[] ring = Disc(w, h, Paper, 0.62f);
            for (int i = 0; i < px.Length; i++) if (ring[i].a > 0) px[i] = Color32.Lerp(px[i], ring[i], ring[i].a / 255f);
            return px;
        }

        private static Color32[] Menu(int w, int h, bool hover)
        {
            Color32[] px = Button(w, h, hover ? TealLight : Teal);
            for (int bar = 0; bar < 3; bar++)
                DrawRect(px, w, h, (int)(w * 0.3f), (int)(h * 0.3f + bar * h * 0.17f), (int)(w * 0.4f), (int)(h * 0.09f), Paper);
            return px;
        }

        private static Color32[] ExitButton(int w, int h, bool hover)
        {
            Color32[] px = Disc(w, h, hover ? new Color(1f, 0.5f, 0.45f) : Coral, 0.95f);
            // an X from two diagonal strokes
            float thick = w * 0.05f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = x - w * 0.5f, dy = y - h * 0.5f;
                    bool inside = Mathf.Abs(dx) < w * 0.22f && Mathf.Abs(dy) < h * 0.22f;
                    if (inside && (Mathf.Abs(dx - dy) < thick || Mathf.Abs(dx + dy) < thick)) px[y * w + x] = Paper;
                }
            return px;
        }

        private static Color32[] Toggle(int w, int h, bool on)
        {
            Color32[] px = Rounded(w, h, h * 0.45f, 4f, on ? Green : Slate, on ? new Color(0.15f, 0.5f, 0.22f) : Ink, Ink, 4);
            float knobX = on ? w * 0.72f : w * 0.28f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (Vector2.Distance(new Vector2(x, y), new Vector2(knobX, h * 0.5f)) < h * 0.3f) px[y * w + x] = Paper;
            return px;
        }

        private static Color32[] Coin(int w, int h, float widthFactor)
        {
            var px = new Color32[w * h];
            float rx = w * 0.5f * widthFactor, ry = h * 0.5f;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float nx = (x + 0.5f - w * 0.5f) / Mathf.Max(rx, 1f), ny = (y + 0.5f - h * 0.5f) / ry;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d > 1f) { px[y * w + x] = new Color32(0, 0, 0, 0); continue; }
                    Color c = d > 0.82f ? new Color(0.75f, 0.5f, 0.05f) : Color.Lerp(Amber, new Color(1f, 0.9f, 0.45f), 1f - d);
                    px[y * w + x] = c;
                }
            return px;
        }

        private static Color32[] SpinningCoin(int w, int h, string file)
        {
            // frame number 1..7 spins the coin through one turn: width follows |cos|
            int frame = int.Parse(Path.GetFileNameWithoutExtension(file).Replace("coin spin ", "")) - 1;
            float factor = Mathf.Max(0.12f, Mathf.Abs(Mathf.Cos(frame / 7f * Mathf.PI)));
            return Coin(w, h, factor);
        }

        private static Color32[] Scatter(int w, int h)
        {
            Color32[] px = new Color32[w * h];
            var rng = new System.Random(3);
            for (int i = 0; i < 9; i++)
            {
                int s = (int)(h * (0.14f + 0.08f * rng.NextDouble()));
                Color32[] coin = Coin(s, s, 1f);
                int ox = (int)(rng.NextDouble() * (w - s)), oy = (int)(rng.NextDouble() * (h - s));
                Blit(px, w, h, coin, s, s, ox, oy);
            }
            return px;
        }

        private static Color32[] Stack(int w, int h)
        {
            Color32[] px = new Color32[w * h];
            int s = (int)(h * 0.6f);
            for (int i = 0; i < 7; i++)
                Blit(px, w, h, Coin(s, s, 1f), s, s, (int)(w * 0.1f + i * (w * 0.8f - s) / 6f), (int)(h * 0.05f + (i % 2) * h * 0.18f));
            return px;
        }

        internal static void Blit(Color32[] dst, int dw, int dh, Color32[] src, int sw, int sh, int ox, int oy)
        {
            for (int y = 0; y < sh; y++)
                for (int x = 0; x < sw; x++)
                {
                    int tx = ox + x, ty = oy + y;
                    if (tx < 0 || ty < 0 || tx >= dw || ty >= dh || src[y * sw + x].a == 0) continue;
                    dst[ty * dw + tx] = src[y * sw + x];
                }
        }
        #endregion drawing primitives
    }
}
