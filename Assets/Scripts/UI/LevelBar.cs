using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>Header level indicator: a progress bar that fills towards the next level and the level number.</summary>
    public class LevelBar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private RectTransform pointer;
        [SerializeField] private Text levelText;

        private PlayerWallet wallet;
        private float fillWidth;
        private float pointerStartX;

        public void Setup(Image fillImage, RectTransform pointerRect, Text level)
        {
            fill = fillImage;
            pointer = pointerRect;
            levelText = level;
        }

        private void Start()
        {
            wallet = PlayerWallet.Instance;
            fillWidth = ((RectTransform)fill.transform).rect.width;
            pointerStartX = pointer.anchoredPosition.x;

            wallet.LevelChanged += SetLevel;
            wallet.LevelProgressChanged += SetProgress;
            SetLevel(wallet.Level);
            SetProgress(wallet.LevelProgress);
        }

        private void OnDestroy()
        {
            if (wallet == null) return;
            wallet.LevelChanged -= SetLevel;
            wallet.LevelProgressChanged -= SetProgress;
        }

        private void SetLevel(int level)
        {
            levelText.text = "LV " + level;
        }

        private void SetProgress(float percent)
        {
            float k = Mathf.Clamp01(percent / 100f);
            fill.fillAmount = k;
            Vector2 p = pointer.anchoredPosition;
            p.x = pointerStartX + fillWidth * k;
            pointer.anchoredPosition = p;
            pointer.gameObject.SetActive(k > 0.05f);
        }
    }
}
