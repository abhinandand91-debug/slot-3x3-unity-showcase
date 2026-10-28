using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>The settings window: switch sound effects and music on or off.</summary>
    public class SettingsPopup : PopupWindow
    {
        [SerializeField] private Button soundToggle;
        [SerializeField] private Button musicToggle;
        [SerializeField] private Sprite onSprite;
        [SerializeField] private Sprite offSprite;

        [SerializeField, Tooltip("Developer buttons; only visible in the editor and development builds")]
        private GameObject devPanel;
        [SerializeField] private Button addCoinsButton;
        [SerializeField] private Button resetDailyButton;
        [SerializeField] private Text addCoinsLabel;

        public void SetupDev(GameObject panel, Button addCoins, Text addCoinsText, Button resetDaily)
        {
            devPanel = panel;
            addCoinsButton = addCoins;
            addCoinsLabel = addCoinsText;
            resetDailyButton = resetDaily;
        }

        /// <summary>Connects the developer buttons (add coins, reset the daily reward).</summary>
        public void InitDev(SlotConfig config, PlayerWallet wallet, DailyRewardController daily)
        {
            devPanel.SetActive(Debug.isDebugBuild);
            addCoinsLabel.text = "ADD " + NumberFormat.Coins(config.devAddCoins);
            addCoinsButton.onClick.AddListener(() => wallet.AddCoins(config.devAddCoins));
            resetDailyButton.onClick.AddListener(daily.ResetData);
        }

        public void SetupToggles(Button sound, Button music, Sprite on, Sprite off)
        {
            soundToggle = sound;
            musicToggle = music;
            onSprite = on;
            offSprite = off;
        }

        protected override void Awake()
        {
            base.Awake();
            soundToggle.onClick.AddListener(() =>
            {
                GameServices.Sound.SetSoundOn(!GameServices.Sound.SoundOn);
                Refresh();
            });
            musicToggle.onClick.AddListener(() =>
            {
                GameServices.Sound.SetMusicOn(!GameServices.Sound.MusicOn);
                Refresh();
            });
        }

        public override void Open()
        {
            Refresh();
            base.Open();
        }

        private void Refresh()
        {
            ISoundPlayer sound = GameServices.Sound;
            soundToggle.image.sprite = sound.SoundOn ? onSprite : offSprite;
            musicToggle.image.sprite = sound.MusicOn ? onSprite : offSprite;
        }
    }
}
