using UnityEngine;

namespace Slot3x3
{
    /// <summary>
    /// Stretches the header plate to the full width of the view. The header lives on a world-space canvas that is
    /// exactly as wide as a 4:3 screen, so on wider screens the plate would otherwise stop short of the edges.
    /// </summary>
    public class HeaderFitter : MonoBehaviour
    {
        [SerializeField] private RectTransform plate;
        [SerializeField] private Camera viewCamera;
        [SerializeField, Tooltip("Canvas pixels per world unit")]
        private float pixelsPerUnit = 100f;

        private float lastAspect;
        private float designWidth;

        public void Setup(RectTransform headerPlate, Camera cam)
        {
            plate = headerPlate;
            viewCamera = cam;
        }

        private void Start()
        {
            designWidth = plate.sizeDelta.x;
            Apply(viewCamera.aspect);
        }

        private void Update()
        {
            if (!Mathf.Approximately(viewCamera.aspect, lastAspect)) Apply(viewCamera.aspect);
        }

        /// <summary>Fits the plate to a view with the given aspect ratio (width / height).</summary>
        public void Apply(float aspect)
        {
            lastAspect = aspect;
            float viewWidth = 2f * viewCamera.orthographicSize * aspect * pixelsPerUnit;
            Vector2 size = plate.sizeDelta;
            size.x = Mathf.Max(designWidth, viewWidth);
            plate.sizeDelta = size;
        }
    }
}
