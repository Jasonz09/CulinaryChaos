using UnityEngine;
using UnityEngine.UI;

namespace IOChef.UI
{
    /// <summary>
    /// Simple sine-wave alpha pulsing effect on an Image component.
    /// </summary>
    public class AlphaPulse : MonoBehaviour
    {
        public float speed = 2f;
        public float minAlpha = 0.05f;
        public float maxAlpha = 0.25f;

        private Image _image;
        private Color _baseColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_image != null) _baseColor = _image.color;
        }

        private void Update()
        {
            if (_image == null) return;
            float t = (Mathf.Sin(Time.unscaledTime * speed) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            _image.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
        }
    }
}
