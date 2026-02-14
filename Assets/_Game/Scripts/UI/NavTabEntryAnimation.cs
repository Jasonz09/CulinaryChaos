using UnityEngine;

namespace IOChef.UI
{
    /// <summary>
    /// Smooth slide-in animation for navigation tabs on menu load.
    /// Creates a staggered entry effect for a polished UX.
    /// </summary>
    public class NavTabEntryAnimation : MonoBehaviour
    {
        private float delay;
        private float elapsed;
        private bool animating = true;
        private Vector3 startPos;
        private Vector3 targetPos;
        private CanvasGroup canvasGroup;

        private const float AnimDuration = 0.4f;
        private const float SlideDistance = 50f;

        /// <summary>
        /// Sets the delay before this tab animates in.
        /// </summary>
        public void SetDelay(float delaySeconds)
        {
            delay = delaySeconds;
        }

        private void Awake()
        {
            // Add CanvasGroup for fade effect
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                targetPos = rect.anchoredPosition;
                startPos = targetPos + new Vector3(0, SlideDistance, 0);
                rect.anchoredPosition = startPos;
            }
        }

        private void Update()
        {
            if (!animating) return;

            elapsed += Time.unscaledDeltaTime;

            // Wait for delay
            if (elapsed < delay)
            {
                return;
            }

            float t = Mathf.Clamp01((elapsed - delay) / AnimDuration);

            // Ease-out cubic for smooth deceleration
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            var rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchoredPosition = Vector3.Lerp(startPos, targetPos, eased);
            }

            // Fade in
            if (canvasGroup != null)
            {
                canvasGroup.alpha = eased;
            }

            // Complete
            if (t >= 1f)
            {
                animating = false;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }
    }
}
