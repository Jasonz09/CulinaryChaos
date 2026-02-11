using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Attach to a RectTransform to automatically apply safe area insets.
    /// Typically placed on a top-level panel inside each Canvas.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        /// <summary>
        /// Cached RectTransform this fitter adjusts.
        /// </summary>
        private RectTransform _rectTransform;

        /// <summary>
        /// Last applied safe area rect, used to detect changes.
        /// </summary>
        private Rect _lastSafeArea;

        /// <summary>
        /// Caches the RectTransform and applies the initial safe area.
        /// </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        /// <summary>
        /// Reapplies safe area if screen dimensions change.
        /// </summary>
        private void Update()
        {
            if (Screen.safeArea != _lastSafeArea)
                ApplySafeArea();
        }

        /// <summary>
        /// Calculates and applies safe area anchor offsets.
        /// </summary>
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;

            Vector2 anchorMin = new Vector2(safeArea.x / Screen.width, safeArea.y / Screen.height);
            Vector2 anchorMax = new Vector2((safeArea.x + safeArea.width) / Screen.width,
                                           (safeArea.y + safeArea.height) / Screen.height);

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
        }
    }
}
