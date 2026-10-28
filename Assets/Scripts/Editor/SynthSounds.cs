using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Slot3x3.EditorTools
{
    /// <summary>
    /// Writes simple synthesised sound effects as WAV files, so the project ships with no licensed audio.
    /// Each sound is a short list of notes; swap the files in Assets/Audio/Generated for real audio at any time.
    /// </summary>
    public static class SynthSounds
    {
        private const string Folder = "Assets/Audio/Generated/";
        private const int SampleRate = 44100;

        /// <summary>One tone: frequency in Hz (0 = noise), length and volume.</summary>
        private struct Note
        {
            public float Hz, Seconds, Volume;
            public Note(float hz, float seconds, float volume = 0.5f) { Hz = hz; Seconds = seconds; Volume = volume; }
        }

        /// <summary>Returns the clip with this name, writing it first if it does not exist yet.</summary>
        public static AudioClip Get(string name)
        {
            string path = Folder + name + ".wav";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllBytes(path, ToWav(Render(Recipe(name))));
                AssetDatabase.ImportAsset(path);
            }
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        /// <summary>The notes that make up each named sound.</summary>
        private static Note[] Recipe(string name)
        {
            switch (name)
            {
                case "button": return new[] { new Note(880f, 0.05f, 0.4f) };
                case "window": return new[] { new Note(660f, 0.06f, 0.35f), new Note(990f, 0.08f, 0.35f) };
                case "spin": return new[] { new Note(0f, 1.2f, 0.12f) };
                case "lose": return new[] { new Note(330f, 0.12f, 0.35f), new Note(262f, 0.2f, 0.35f) };
                case "win": return new[] { new Note(523f, 0.08f), new Note(659f, 0.08f), new Note(784f, 0.16f) };
                case "bigwin": return new[] { new Note(523f, 0.1f), new Note(659f, 0.1f), new Note(784f, 0.1f), new Note(1047f, 0.35f) };
                case "bonus": return new[] { new Note(784f, 0.07f), new Note(988f, 0.07f), new Note(1175f, 0.07f), new Note(1568f, 0.2f) };
                case "music":
                    // a slow, quiet arpeggio that loops
                    float[] melody = { 262f, 330f, 392f, 330f, 294f, 349f, 440f, 349f };
                    var notes = new Note[melody.Length * 2];
                    for (int i = 0; i < notes.Length; i++) notes[i] = new Note(melody[i % melody.Length], 0.4f, 0.12f);
                    return notes;
                default: throw new ArgumentException("Unknown sound " + name);
            }
        }

        /// <summary>Turns notes into samples: a soft square-ish wave with a quick fade in and out per note.</summary>
        private static float[] Render(Note[] notes)
        {
            int total = 0;
            foreach (Note n in notes) total += (int)(n.Seconds * SampleRate);

            var samples = new float[total];
            var noise = new System.Random(1);
            int offset = 0;
            foreach (Note n in notes)
            {
                int length = (int)(n.Seconds * SampleRate);
                for (int i = 0; i < length; i++)
                {
                    float t = i / (float)SampleRate;
                    float wave = n.Hz <= 0f
                        ? (float)(noise.NextDouble() * 2.0 - 1.0)
                        : Mathf.Sin(2f * Mathf.PI * n.Hz * t) + 0.3f * Mathf.Sin(6f * Mathf.PI * n.Hz * t);
                    float envelope = Mathf.Min(1f, i / 200f) * Mathf.Min(1f, (length - i) / 800f);
                    samples[offset + i] = wave * envelope * n.Volume * 0.7f;
                }
                offset += length;
            }
            return samples;
        }

        /// <summary>Encodes mono float samples as a 16-bit PCM WAV file.</summary>
        private static byte[] ToWav(float[] samples)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                int dataSize = samples.Length * 2;
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + dataSize);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);              // format chunk size
                writer.Write((short)1);        // PCM
                writer.Write((short)1);        // mono
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);  // bytes per second
                writer.Write((short)2);        // bytes per sample
                writer.Write((short)16);       // bits per sample
                writer.Write("data".ToCharArray());
                writer.Write(dataSize);
                foreach (float s in samples) writer.Write((short)(Mathf.Clamp(s, -1f, 1f) * short.MaxValue));
                return stream.ToArray();
            }
        }
    }
}
