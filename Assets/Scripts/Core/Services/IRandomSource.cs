namespace Slot3x3
{
    /// <summary>
    /// Random numbers for game outcomes. Reels ask this interface instead of UnityEngine.Random,
    /// so a test can plug in a seeded source and get the same spins every run.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>A whole number from min (inclusive) to max (exclusive).</summary>
        int Range(int min, int max);

        /// <summary>A float from min to max (both inclusive).</summary>
        float Range(float min, float max);
    }

    /// <summary>The normal source: Unity's global random generator.</summary>
    public class UnityRandomSource : IRandomSource
    {
        public int Range(int min, int max) { return UnityEngine.Random.Range(min, max); }
        public float Range(float min, float max) { return UnityEngine.Random.Range(min, max); }
    }

    /// <summary>A repeatable source for tests and replays: the same seed always gives the same numbers.</summary>
    public class SeededRandomSource : IRandomSource
    {
        private readonly System.Random random;

        public SeededRandomSource(int seed) { random = new System.Random(seed); }

        public int Range(int min, int max) { return random.Next(min, max); }
        public float Range(float min, float max) { return min + (float)random.NextDouble() * (max - min); }
    }
}
