using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>
    /// Shown when the player cannot afford a spin. For development it offers a free refill so testing never
    /// dead-ends; turn <c>SlotConfig.allowDevRefill</c> off for a release and wire a real shop here instead.
    /// </summary>
    public class OutOfCreditsPopup : PopupWindow
    {
        [SerializeField] private Button refillButton;
        [SerializeField] private Text refillLabel;

        public void SetupRefill(Button refill, Text label)
        {
            refillButton = refill;
            refillLabel = label;
        }

        public void Init(SlotConfig config, PlayerWallet wallet)
        {
            refillLabel.text = "GET " + NumberFormat.Coins(config.devRefillCoins);
            refillButton.onClick.AddListener(() =>
            {
                wallet.AddCoins(config.devRefillCoins);
                GameServices.Sound.PlayWinCoins();
                Close();
            });
        }
    }
}
