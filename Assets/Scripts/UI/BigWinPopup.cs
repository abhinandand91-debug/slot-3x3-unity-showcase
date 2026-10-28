using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>The "BIG WIN" congratulation panel shown for wins of 5000 coins or more.</summary>
    public class BigWinPopup : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text amountText;
        [SerializeField] private Button collectButton;
        [SerializeField] private RectTransform panel;

        private Coroutine routine;

        public bool IsOpen { get; private set; }

        public void Setup(GameObject popupRoot, Text amount, Button collect, RectTransform popupPanel)
        {
            root = popupRoot;
            amountText = amount;
            collectButton = collect;
            panel = popupPanel;
        }

        private void Awake()
        {
            root.SetActive(false);
            collectButton.onClick.AddListener(Close);
        }

        /// <summary>Opens the panel; it closes by itself after <paramref name="autoCloseSeconds"/>.</summary>
        public void Show(int coins, float autoCloseSeconds)
        {
            amountText.text = NumberFormat.Coins(coins);
            root.SetActive(true);
            IsOpen = true;
            GameServices.Sound.PlayBigWin();

            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(OpenRoutine(autoCloseSeconds));
        }

        private IEnumerator OpenRoutine(float autoCloseSeconds)
        {
            // pop in
            yield return Tween.Value(0.6f, 1f, 0.35f, s => panel.localScale = new Vector3(s, s, 1f), Ease.OutBounce);
            yield return new WaitForSeconds(autoCloseSeconds);
            Close();
        }

        public void Close()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
            root.SetActive(false);
            IsOpen = false;
        }
    }
}
