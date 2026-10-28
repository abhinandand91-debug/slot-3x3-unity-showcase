namespace Slot3x3
{
    /// <summary>
    /// Everything the game can ask the audio system to do. Gameplay and UI classes talk to this interface,
    /// not to <see cref="SoundController"/> directly, so audio can be muted, replaced or faked in tests.
    /// </summary>
    public interface ISoundPlayer
    {
        bool SoundOn { get; }
        bool MusicOn { get; }
        void SetSoundOn(bool on);
        void SetMusicOn(bool on);

        void PlayMusic();
        void PauseMusic();

        void PlayButton();
        void PlayWindowOpen();
        void PlayWinCoins();
        void PlayWinFreeSpin(float delay = 0f);
        void PlayLose();
        void PlayBigWin();
        void PlayBonus();

        void PlayRotation();
        void StopRotation();
    }

    /// <summary>A sound player that does nothing. Used before the real one exists, and in tests.</summary>
    public class SilentSoundPlayer : ISoundPlayer
    {
        public bool SoundOn { get { return false; } }
        public bool MusicOn { get { return false; } }
        public void SetSoundOn(bool on) { }
        public void SetMusicOn(bool on) { }
        public void PlayMusic() { }
        public void PauseMusic() { }
        public void PlayButton() { }
        public void PlayWindowOpen() { }
        public void PlayWinCoins() { }
        public void PlayWinFreeSpin(float delay = 0f) { }
        public void PlayLose() { }
        public void PlayBigWin() { }
        public void PlayBonus() { }
        public void PlayRotation() { }
        public void StopRotation() { }
    }
}
