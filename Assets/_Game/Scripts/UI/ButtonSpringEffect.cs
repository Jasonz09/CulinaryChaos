using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IOChef.UI
{
    /// <summary>
    /// Modern spring-physics button effect with smooth ease-out scaling
    /// and subtle opacity pulse. Feels premium and responsive.
    /// </summary>
    public class ButtonSpringEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private Vector3 restScale;
        private Vector3 velocity;
        private Vector3 targetScale;
        private CanvasGroup canvasGroup;

        // Spring constants — tuned for snappy modern feel
        private const float SpringStiffness = 300f;
        private const float SpringDamping = 18f;
        private const float HoverScale = 1.04f;
        private const float PressScale = 0.94f;

        private void Awake()
        {
            restScale = Vector3.one;
            targetScale = restScale;
            velocity = Vector3.zero;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            transform.localScale = restScale;
            targetScale = restScale;
            velocity = Vector3.zero;
        }

        private void Update()
        {
            // Critically-damped spring for buttery smooth motion
            float dt = Time.unscaledDeltaTime;
            Vector3 displacement = transform.localScale - targetScale;
            Vector3 springForce = -SpringStiffness * displacement - SpringDamping * velocity;
            velocity += springForce * dt;
            transform.localScale += velocity * dt;

            // Subtle opacity feedback on CanvasGroup
            if (canvasGroup != null)
            {
                float scaleRatio = transform.localScale.x / restScale.x;
                canvasGroup.alpha = Mathf.Lerp(0.88f, 1f, Mathf.InverseLerp(PressScale, HoverScale, scaleRatio));
            }
        }

        public void OnPointerEnter(PointerEventData e)
        {
            targetScale = restScale * HoverScale;
        }

        public void OnPointerExit(PointerEventData e)
        {
            targetScale = restScale;
        }

        public void OnPointerDown(PointerEventData e)
        {
            targetScale = restScale * PressScale;
            velocity = Vector3.zero; // snap feel on press
        }

        public void OnPointerUp(PointerEventData e)
        {
            targetScale = restScale * HoverScale;
        }
    }
}
