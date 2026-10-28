using UnityEngine;

namespace Slot3x3
{
    /// <summary>Plays a looping flip-book of sprites on a SpriteRenderer (used for the spinning coins).</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 14f;
        [SerializeField, Tooltip("Frame to start on, so a group of coins does not spin in lockstep")]
        private int startFrame;

        private SpriteRenderer spriteRenderer;
        private float time;

        public void Setup(Sprite[] animationFrames, float fps, int firstFrame)
        {
            frames = animationFrames;
            framesPerSecond = fps;
            startFrame = firstFrame;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            time += Time.deltaTime;
            int index = (startFrame + (int)(time * framesPerSecond)) % frames.Length;
            spriteRenderer.sprite = frames[index];
        }
    }
}
