using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace IOChef.UI
{
    /// <summary>
    /// Attached to a nav tab button. Shows an orange underline and scales up
    /// the label text on hover / press. Hides both when not interacting.
    /// </summary>
    public class NavTabClickFlash : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private GameObject underline;
        private Image underlineImage;
        private RectTransform labelRT;
        private TextMeshProUGUI labelTMP;
        private Color baseColor = new Color(1f, 0.55f, 0.1f, 1f);

        private float baseFontSize;
        private float hoverFontSize;

        private bool isHovered;
        private bool isPressed;

        // Smooth animation
        private float currentScale = 1f;
        private float targetScale = 1f;
        private float currentAlpha = 0f;
        private float targetAlpha = 0f;
        private float animSpeed = 10f;

        public void Init(GameObject underlineObj, RectTransform label)
        {
            underline = underlineObj;
            underlineImage = underline.GetComponent<Image>();
            underlineImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            underline.SetActive(true); // keep active, control via alpha

            labelRT = label;
            labelTMP = label.GetComponent<TextMeshProUGUI>();
            baseFontSize = labelTMP.fontSize;
            hoverFontSize = baseFontSize + 3f; // grow by 3pt on hover
        }

        // Legacy — keep for compatibility, now a no-op
        public void Init(GameObject underlineObj)
        {
            underline = underlineObj;
            underlineImage = underline.GetComponent<Image>();
            underlineImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);
            underline.SetActive(true);
        }

        public void Flash() { } // no-op, kept for compatibility

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            if (!isPressed) SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            if (!isHovered) SetActive(false);
        }

        private void SetActive(bool active)
        {
            targetAlpha = active ? 1f : 0f;
            targetScale = active ? 1f : 0f; // 1 = hover size, 0 = base size
        }

        private void Update()
        {
            // Smooth alpha
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.unscaledDeltaTime * animSpeed);
            if (underlineImage != null)
                underlineImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, currentAlpha);

            // Smooth font size
            currentScale = Mathf.MoveTowards(currentScale, targetScale, Time.unscaledDeltaTime * animSpeed);
            if (labelTMP != null)
                labelTMP.fontSize = Mathf.Lerp(baseFontSize, hoverFontSize, currentScale);
        }
    }
}
