using System.Collections;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>A short message shown over the reels, e.g. "You have no money."</summary>
    public class MessageToast : MonoBehaviour
    {
        [SerializeField] private TextMesh label;

        private Coroutine routine;

        public void Setup(TextMesh text)
        {
            label = text;
        }

        private void Awake()
        {
            label.gameObject.SetActive(false);
        }

        public void Show(string message, float seconds)
        {
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(ShowRoutine(message, seconds));
        }

        private IEnumerator ShowRoutine(string message, float seconds)
        {
            label.text = message;
            label.gameObject.SetActive(true);
            yield return new WaitForSeconds(seconds);
            label.gameObject.SetActive(false);
            routine = null;
        }
    }
}
