namespace Slot3x3
{
    /// <summary>
    /// One place that hands out the shared services (saving, randomness, sound).
    /// Classes ask for an interface here instead of creating or finding the concrete class themselves,
    /// so each service can be swapped in one line: tests set a <see cref="MemorySaveStore"/> or a
    /// <see cref="SeededRandomSource"/>, and the real <see cref="SoundController"/> registers itself on Awake.
    /// </summary>
    public static class GameServices
    {
        private static ISoundPlayer sound;

        public static ISaveStore Save { get; set; } = new PlayerPrefsSaveStore();
        public static IRandomSource Random { get; set; } = new UnityRandomSource();

        /// <summary>The active sound player; a silent one until the scene's SoundController wakes up.</summary>
        public static ISoundPlayer Sound
        {
            get { return sound ?? (sound = new SilentSoundPlayer()); }
            set { sound = value; }
        }
    }
}
