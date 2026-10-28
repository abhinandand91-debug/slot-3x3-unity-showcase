using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Scales an object to follow the screen's aspect ratio, so the background and the machine keep
    /// filling the view on wide and narrow displays. <c>heightWidth</c> blends between no scaling (0)
    /// and following the aspect ratio completely (1).
    /// </summary>
    public class SceneScaler : MonoBehaviour
    {
        [SerializeField] private float baseRatio = 1.334f;
        [SerializeField, Range(0f, 1f)] private float heightWidth = 1f;
        [SerializeField, Range(0f, 1f)] private float additionalScale = 1f;

        private int width;
        private int height;

        public void Setup(float designRatio, float blend)
        {
            baseRatio = designRatio;
            heightWidth = blend;
        }

        private void Start()
        {
            Apply();
        }

        private void Update()
        {
            if (width != Screen.width || height != Screen.height) Apply();
        }

        private void Apply()
        {
            width = Screen.width;
            height = Screen.height;
            if (height == 0) return;
            ApplyRatio(width / (float)height);
        }

        /// <summary>Scales for the given screen aspect ratio (width / height).</summary>
        public void ApplyRatio(float ratio)
        {
            float scale = ratio / Mathf.Clamp(baseRatio, 0.3f, 2.5f);
            scale = Mathf.Lerp(1f, scale, heightWidth) * additionalScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
