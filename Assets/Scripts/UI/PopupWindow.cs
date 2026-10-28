using UnityEngine;
using UnityEngine.UI;

namespace Slot3x3
{
    /// <summary>A modal window: a dimmed backdrop with a panel and a close button that can be opened and closed.</summary>
    public class PopupWindow : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Button closeButton;

        public bool IsOpen { get { return root.activeSelf; } }

        public void Setup(GameObject windowRoot, Button close)
        {
            root = windowRoot;
            closeButton = close;
        }

        protected virtual void Awake()
        {
            root.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        public virtual void Open()
        {
            root.SetActive(true);
            GameServices.Sound.PlayWindowOpen();
        }

        public virtual void Close()
        {
            root.SetActive(false);
            GameServices.Sound.PlayWindowOpen();
        }
    }
}
