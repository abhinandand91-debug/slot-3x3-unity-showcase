using System;
using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// A button made of a sprite and a 2D collider that lives in the game world (not in a canvas).
    /// <see cref="PointerInput"/> feeds it pointer events. It swaps between a normal and a pressed sprite
    /// and can optionally stay pressed after a click (used for the auto spin toggle).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class SceneButton : MonoBehaviour
    {
        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite pressedSprite;
        [SerializeField, Tooltip("Stay pressed after a click until Release() is called")]
        private bool toggle;
        [SerializeField, Tooltip("Fire the click when the pointer goes down instead of up")]
        private bool clickOnPress;

        public bool interactable = true;

        /// <summary>Raised when the button is clicked.</summary>
        public event Action<SceneButton> Clicked;

        public bool Pressed { get; private set; }

        private SpriteRenderer spriteRenderer;
        private bool pointerDown;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetSprites(Sprite normal, Sprite pressed)
        {
            normalSprite = normal;
            pressedSprite = pressed;
        }

        public int SortingOrder { get { return spriteRenderer != null ? spriteRenderer.sortingOrder : 0; } }

        internal void PointerDown()
        {
            if (!interactable) return;
            pointerDown = true;
            GameServices.Sound.PlayButton();
            if (toggle) Pressed = !Pressed;
            if (pressedSprite != null) spriteRenderer.sprite = pressedSprite;
            if (clickOnPress) Clicked?.Invoke(this);
        }

        internal void PointerUp(bool insideButton)
        {
            if (!pointerDown) return;
            pointerDown = false;
            if (!toggle) Pressed = false;
            ApplySprite();
            if (insideButton && !clickOnPress && interactable) Clicked?.Invoke(this);
        }

        /// <summary>Returns a toggle button to its normal state.</summary>
        public void Release()
        {
            Pressed = false;
            ApplySprite();
        }

        /// <summary>Shows the pressed sprite without raising any event (used for selected payline buttons).</summary>
        public void SetPressedVisual(bool pressed)
        {
            Pressed = pressed;
            ApplySprite();
        }

        private void ApplySprite()
        {
            if (spriteRenderer != null && normalSprite != null && pressedSprite != null)
                spriteRenderer.sprite = Pressed ? pressedSprite : normalSprite;
        }
    }
}
