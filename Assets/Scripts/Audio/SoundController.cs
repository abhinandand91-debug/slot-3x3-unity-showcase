using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Plays the game's sounds: looping background music, the looping reel-spin sound and one-shot
    /// effects. Sound and music can be switched off independently; the choice is saved.
    /// </summary>
    public class SoundController : MonoBehaviour, ISoundPlayer
    {
        private const string SoundKey = "slot3x3_sound";
        private const string MusicKey = "slot3x3_music";

        [Header("Music")]
        [SerializeField] private AudioClip music;
        [Header("Effects")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip windowOpen;
        [SerializeField] private AudioClip winCoins;
        [SerializeField] private AudioClip winFreeSpin;
        [SerializeField] private AudioClip slotRotation;
        [SerializeField] private AudioClip slotLose;
        [SerializeField] private AudioClip bigWin;
        [SerializeField] private AudioClip bonus;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;


        private AudioSource musicSource;
        private AudioSource loopSource;
        private AudioSource effectSource;

        public bool SoundOn { get; private set; }
        public bool MusicOn { get; private set; }

        private void Awake()
        {
            // register as the game's sound player (see GameServices)
            GameServices.Sound = this;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.loop = true;
            loopSource.playOnAwake = false;

            effectSource = gameObject.AddComponent<AudioSource>();
            effectSource.playOnAwake = false;

            SoundOn = GameServices.Save.GetInt(SoundKey, 1) == 1;
            MusicOn = GameServices.Save.GetInt(MusicKey, 1) == 1;
        }

        private void Start()
        {
            PlayMusic();
        }

        private void OnDestroy()
        {
            // hand back a silent player so nobody calls into a destroyed object
            if (ReferenceEquals(GameServices.Sound, this)) GameServices.Sound = null;
        }

        #region settings
        public void SetSoundOn(bool on)
        {
            SoundOn = on;
            GameServices.Save.SetInt(SoundKey, on ? 1 : 0);
            if (!on) StopRotation();
        }

        public void SetMusicOn(bool on)
        {
            MusicOn = on;
            GameServices.Save.SetInt(MusicKey, on ? 1 : 0);
            if (on) PlayMusic(); else musicSource.Pause();
        }
        #endregion settings

        #region music
        public void PlayMusic()
        {
            if (!MusicOn || music == null) return;
            if (musicSource.clip != music) musicSource.clip = music;
            if (!musicSource.isPlaying) musicSource.Play();
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }
        #endregion music

        #region effects
        public void PlayButton() { PlayEffect(buttonClick, 0f); }
        public void PlayWindowOpen() { PlayEffect(windowOpen, 0f); }
        public void PlayWinCoins() { PlayEffect(winCoins, 0f); }
        public void PlayWinFreeSpin(float delay = 0f) { PlayEffect(winFreeSpin, delay); }
        public void PlayLose() { PlayEffect(slotLose, 0f); }
        public void PlayBigWin() { PlayEffect(bigWin, 0f); }
        public void PlayBonus() { PlayEffect(bonus, 0f); }

        private void PlayEffect(AudioClip clip, float delay)
        {
            if (!SoundOn || clip == null) return;
            if (delay <= 0f) effectSource.PlayOneShot(clip);
            else StartCoroutine(PlayDelayed(clip, delay));
        }

        private IEnumerator PlayDelayed(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (SoundOn) effectSource.PlayOneShot(clip);
        }
        #endregion effects

        #region reel loop
        public void PlayRotation()
        {
            if (!SoundOn || slotRotation == null) return;
            loopSource.clip = slotRotation;
            loopSource.Play();
        }

        public void StopRotation()
        {
            loopSource.Stop();
        }
        #endregion reel loop
    }
}
