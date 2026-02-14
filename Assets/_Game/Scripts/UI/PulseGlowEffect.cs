using UnityEngine;
using UnityEngine.UI;

namespace IOChef.UI
{
    /// <summary>
    /// Subtle pulsing glow effect for active navigation tabs.
    /// Creates a breathing animation that draws attention without being distracting.
    /// </summary>
    public class PulseGlowEffect : MonoBehaviour
    {
        private Image image;
        private Color baseColor;
        private float time;

        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float minAlpha = 0.15f;
        [SerializeField] private float maxAlpha = 0.4f;

        private void Awake()
        {
            image = GetComponent<Image>();
            if (image != null)
            {
                baseColor = image.color;
            }
        }

        private void Update()
        {
            if (image == null) return;

            time += Time.unscaledDeltaTime * pulseSpeed;

            // Smooth sine wave for breathing effect
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, 
                (Mathf.Sin(time) + 1f) * 0.5f);

            Color newColor = baseColor;
            newColor.a = alpha;
            image.color = newColor;
        }
    }
}
