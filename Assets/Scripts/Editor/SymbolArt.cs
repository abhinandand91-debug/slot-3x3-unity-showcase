using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// Draws the reel symbols from code: a coloured card with a short label in a tiny pixel font.
    /// Each symbol also gets a "blur" twin (a vertical smear) that the reels show while they spin.
    /// </summary>
    public static class SymbolArt
    {
        private const string Folder = PlaceholderArt.Folder + "Symbols/";
        private const int Width = 300, Height = 260;

        /// <summary>Card colour and label for every symbol file name the config asks for.</summary>
        private static readonly Dictionary<string, (Color color, string label)> Symbols = new Dictionary<string, (Color, string)>
        {
            { "10", (new Color(0.30f, 0.55f, 0.95f), "10") },
            { "J", (new Color(0.25f, 0.70f, 0.45f), "J") },
            { "Q", (new Color(0.85f, 0.35f, 0.65f), "Q") },
            { "K", (new Color(0.95f, 0.55f, 0.20f), "K") },
            { "A", (new Color(0.85f, 0.25f, 0.25f), "A") },
            { "1Bar", (new Color(0.45f, 0.45f, 0.50f), "BAR") },
            { "2Bar", (new Color(0.35f, 0.35f, 0.42f), "2BAR") },
            { "3Bar", (new Color(0.25f, 0.25f, 0.32f), "3BAR") },
            { "Bell", (new Color(0.95f, 0.75f, 0.15f), "BELL") },
            { "7", (new Color(0.80f, 0.10f, 0.15f), "7") },
            { "Wild", (new Color(0.55f, 0.20f, 0.85f), "WILD") },
            { "Scatter", (new Color(0.10f, 0.65f, 0.70f), "SCAT") },
            { "FreeSpin", (new Color(0.20f, 0.75f, 0.30f), "FREE") },
            { "Jackpot", (new Color(0.98f, 0.62f, 0.05f), "JP") },
        };

        /// <summary>The blur file names use slightly different words than the symbols themselves.</summary>
        private static readonly Dictionary<string, string> BlurAliases = new Dictionary<string, string>
        {
            { "Bar", "1Bar" },
            { "Free Speen", "FreeSpin" },
        };

        /// <summary>Returns (and on first use draws) the sprite for a file such as "7.png" or "Symbol Blur 7.png".</summary>
        public static Sprite Get(string file)
        {
            string path = Folder + file;
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            string name = Path.GetFileNameWithoutExtension(file);
            bool blur = name.StartsWith("Symbol Blur ");
            if (blur) name = name.Substring("Symbol Blur ".Length);
            if (BlurAliases.TryGetValue(name, out string alias)) name = alias;

            if (!Symbols.TryGetValue(name, out var look))
            {
                Debug.LogError("No placeholder look for symbol " + file);
                look = (Color.gray, "?");
            }

            Color32[] pixels = Card(look.color, look.label);
            if (blur) pixels = VerticalSmear(pixels, Width, Height, 18);
            return PlaceholderArt.SaveSprite(path, pixels, Width, Height, 100f);
        }

        /// <summary>Draws a single symbol as a texture (used for the app icon).</summary>
        public static Texture2D DrawTexture(string symbolName)
        {
            var look = Symbols[symbolName];
            var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            texture.SetPixels32(Card(look.color, look.label));
            texture.Apply();
            return texture;
        }

        #region drawing
        private static Color32[] Card(Color color, string label)
        {
            Color top = Color.Lerp(color, Color.white, 0.25f);
            Color bottom = Color.Lerp(color, Color.black, 0.30f);
            Color32[] px = PlaceholderArt.Rounded(Width, Height, 36f, 14f, top, bottom, Color.white, 6);
            PixelFont.DrawCentered(px, Width, Height, label, Color.white, Mathf.RoundToInt(Width * 0.72f), Mathf.RoundToInt(Height * 0.55f));
            return px;
        }

        /// <summary>Averages each pixel with its neighbours above and below, like motion blur on a spinning reel.</summary>
        private static Color32[] VerticalSmear(Color32[] src, int w, int h, int radius)
        {
            var dst = new Color32[src.Length];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    int count = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int yy = y + k;
                        if (yy < 0 || yy >= h) continue;
                        Color32 c = src[yy * w + x];
                        r += c.r; g += c.g; b += c.b; a += c.a;
                        count++;
                    }
                    dst[y * w + x] = new Color32((byte)(r / count), (byte)(g / count), (byte)(b / count), (byte)(a / count));
                }
            return dst;
        }
        #endregion drawing
    }

    /// <summary>A tiny 5x7 pixel font, enough to write labels on generated art without importing a font file.</summary>
    public static class PixelFont
    {
        private const int GlyphW = 5, GlyphH = 7, Spacing = 1;

        // each glyph is 7 rows of 5 characters, '#' = filled
        private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
        {
            { '0', new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." } },
            { '1', new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." } },
            { '2', new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" } },
            { '3', new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." } },
            { '7', new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." } },
            { 'A', new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" } },
            { 'B', new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." } },
            { 'C', new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." } },
            { 'D', new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." } },
            { 'E', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" } },
            { 'F', new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." } },
            { 'I', new[] { ".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###." } },
            { 'J', new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." } },
            { 'K', new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" } },
            { 'L', new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" } },
            { 'P', new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." } },
            { 'Q', new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" } },
            { 'R', new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" } },
            { 'S', new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." } },
            { 'T', new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." } },
            { 'W', new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" } },
            { '?', new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." } },
        };

        /// <summary>Writes <paramref name="text"/> centred in the image, scaled up to fit the given box.</summary>
        public static void DrawCentered(Color32[] px, int w, int h, string text, Color color, int maxWidth, int maxHeight)
        {
            int textW = text.Length * (GlyphW + Spacing) - Spacing;
            int scale = Mathf.Max(1, Mathf.Min(maxWidth / textW, maxHeight / GlyphH));
            int x0 = (w - textW * scale) / 2;
            int y0 = (h - GlyphH * scale) / 2;

            for (int i = 0; i < text.Length; i++)
            {
                if (!Glyphs.TryGetValue(char.ToUpperInvariant(text[i]), out string[] rows)) continue;
                int gx = x0 + i * (GlyphW + Spacing) * scale;
                for (int row = 0; row < GlyphH; row++)
                    for (int col = 0; col < GlyphW; col++)
                    {
                        if (rows[row][col] != '#') continue;
                        // textures start at the bottom row, glyph rows start at the top
                        int py = y0 + (GlyphH - 1 - row) * scale;
                        PlaceholderArt.DrawRect(px, w, h, gx + col * scale, py, scale, scale, color);
                    }
            }
        }
    }
}
