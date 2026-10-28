using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Turns mouse and touch input into <see cref="SceneButton"/> events. The topmost button
    /// (highest sorting order) under the pointer receives the press, and the click is only
    /// delivered if the pointer is released over the same button.
    /// </summary>
    public class PointerInput : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        private SceneButton pressed;

        private void Awake()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                pressed = ButtonAt(Input.mousePosition);
                if (pressed != null) pressed.PointerDown();
            }
            else if (Input.GetMouseButtonUp(0) && pressed != null)
            {
                SceneButton under = ButtonAt(Input.mousePosition);
                pressed.PointerUp(under == pressed);
                pressed = null;
            }
        }

        private SceneButton ButtonAt(Vector3 screenPosition)
        {
            Vector2 world = worldCamera.ScreenToWorldPoint(screenPosition);
            SceneButton best = null;
            foreach (Collider2D hit in Physics2D.OverlapPointAll(world))
            {
                SceneButton button = hit.GetComponent<SceneButton>();
                if (button != null && button.isActiveAndEnabled && (best == null || button.SortingOrder > best.SortingOrder))
                    best = button;
            }
            return best;
        }
    }
}
