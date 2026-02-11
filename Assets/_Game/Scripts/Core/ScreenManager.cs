using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton manager that tracks screen dimensions, safe area, and notch detection.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance of the ScreenManager.
        /// </summary>
        public static ScreenManager Instance { get; private set; }

        /// <summary>
        /// The device safe area in pixel coordinates.
        /// </summary>
        public Rect SafeArea { get; private set; }
        /// <summary>
        /// The current screen resolution in pixels.
        /// </summary>
        public Vector2 ScreenSize { get; private set; }
        /// <summary>
        /// Whether the device has a display notch or cutout.
        /// </summary>
        public bool HasNotch { get; private set; }
        /// <summary>
        /// The screen width-to-height aspect ratio.
        /// </summary>
        public float AspectRatio { get; private set; }

        /// <summary>
        /// Initializes singleton and captures initial screen info.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            UpdateScreenInfo();
        }

        /// <summary>
        /// Updates cached screen dimensions and orientation.
        /// </summary>
        private void UpdateScreenInfo()
        {
            SafeArea = Screen.safeArea;
            ScreenSize = new Vector2(Screen.width, Screen.height);
            AspectRatio = (float)Screen.width / Screen.height;

            // Detect notch by comparing safe area to full screen
            HasNotch = SafeArea.y > 0 || SafeArea.height < Screen.height;
        }

        /// <summary>
        /// Returns safe area as normalized anchors (0..1) for use with RectTransform.
        /// </summary>
        /// <returns>A tuple of anchorMin and anchorMax vectors in normalized coordinates.</returns>
        public (Vector2 anchorMin, Vector2 anchorMax) GetSafeAreaAnchors()
        {
            var anchorMin = new Vector2(SafeArea.x / Screen.width, SafeArea.y / Screen.height);
            var anchorMax = new Vector2((SafeArea.x + SafeArea.width) / Screen.width,
                                       (SafeArea.y + SafeArea.height) / Screen.height);
            return (anchorMin, anchorMax);
        }
    }
}
