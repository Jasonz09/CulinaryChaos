using UnityEngine;
using UnityEngine.EventSystems;

namespace IOChef.UI
{
    /// <summary>
    /// Adds a scale bounce effect to buttons on hover, press, and release.
    /// </summary>
    public class ButtonBounceEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Original scale of the button before any bounce effect.
        /// </summary>
        private Vector3 orig;

        /// <summary>
        /// Target scale the button is lerping towards.
        /// </summary>
        private Vector3 target;

        /// <summary>
        /// Whether the effect has been initialized.
        /// </summary>
        private bool ready;

        /// <summary>
        /// Captures the original scale for bounce calculations.
        /// </summary>
        private void Start()  { orig = target = Vector3.one; transform.localScale = Vector3.one; ready = true; }

        /// <summary>
        /// Resets scale to original when re-enabled.
        /// </summary>
        private void OnEnable(){ if (ready) { transform.localScale = orig; target = orig; } }

        /// <summary>
        /// Lerps the transform scale towards the target each frame.
        /// </summary>
        private void Update()  { transform.localScale = Vector3.Lerp(transform.localScale, target, Time.unscaledDeltaTime * 12f); }

        /// <summary>
        /// Scale up on pointer enter.
        /// </summary>
        /// <param name="e">Pointer event data.</param>
        public void OnPointerEnter(PointerEventData e) => target = orig * 1.08f;

        /// <summary>
        /// Restore scale on pointer exit.
        /// </summary>
        /// <param name="e">Pointer event data.</param>
        public void OnPointerExit (PointerEventData e) => target = orig;

        /// <summary>
        /// Scale down on pointer press.
        /// </summary>
        /// <param name="e">Pointer event data.</param>
        public void OnPointerDown (PointerEventData e) => target = orig * 0.92f;

        /// <summary>
        /// Scale up on pointer release.
        /// </summary>
        /// <param name="e">Pointer event data.</param>
        public void OnPointerUp  (PointerEventData e) => target = orig * 1.08f;
    }
}
