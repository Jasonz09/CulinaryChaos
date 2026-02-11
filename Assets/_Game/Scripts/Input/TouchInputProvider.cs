using System;
using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Touch-based input provider that uses a virtual joystick on the left side of the screen.
    /// </summary>
    public class TouchInputProvider : MonoBehaviour, IInputProvider
    {
        /// <summary>
        /// Raised each frame with the current movement direction from the virtual joystick.
        /// </summary>
        public event Action<Vector2> OnMovement;
        /// <summary>
        /// Raised when the player taps the right side of the screen to interact.
        /// </summary>
        public event Action OnInteract;
        /// <summary>
        /// Raised when the player taps the pause region of the screen.
        /// </summary>
        public event Action OnPause;

        /// <summary>
        /// Screen position where the current joystick touch began.
        /// </summary>
        private Vector2 _joystickOrigin;
        /// <summary>
        /// Whether the player is currently dragging the virtual joystick.
        /// </summary>
        private bool _isDragging;
        /// <summary>
        /// Touch finger ID currently driving the virtual joystick, or -1 if none.
        /// </summary>
        private int _joystickFingerId = -1;

        [Header("Settings")]
        /// <summary>
        /// Minimum drag distance in pixels before joystick input is registered.
        /// </summary>
        [SerializeField] private float joystickDeadzone = 20f;
        /// <summary>
        /// Maximum drag radius in pixels used to normalize joystick output.
        /// </summary>
        [SerializeField] private float joystickMaxRadius = 100f;

        /// <summary>
        /// Reads current touch state and raises input events accordingly.
        /// </summary>
        public void ProcessInput()
        {
            if (Input.touchCount == 0)
            {
                if (_isDragging)
                {
                    _isDragging = false;
                    _joystickFingerId = -1;
                    OnMovement?.Invoke(Vector2.zero);
                }
                return;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                float screenMidX = Screen.width * 0.5f;

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        if (touch.position.x < Screen.width * 0.4f && _joystickFingerId == -1)
                        {
                            _joystickOrigin = touch.position;
                            _joystickFingerId = touch.fingerId;
                            _isDragging = true;
                        }
                        else if (touch.position.x > Screen.width * 0.6f)
                        {
                            OnInteract?.Invoke();
                        }
                        else if (touch.position.y > Screen.height * 0.9f && touch.position.x > Screen.width * 0.8f)
                        {
                            OnPause?.Invoke();
                        }
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (touch.fingerId == _joystickFingerId)
                        {
                            Vector2 delta = touch.position - _joystickOrigin;
                            if (delta.magnitude > joystickDeadzone)
                            {
                                Vector2 clamped = Vector2.ClampMagnitude(delta, joystickMaxRadius);
                                OnMovement?.Invoke(clamped / joystickMaxRadius);
                            }
                            else
                            {
                                OnMovement?.Invoke(Vector2.zero);
                            }
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touch.fingerId == _joystickFingerId)
                        {
                            _isDragging = false;
                            _joystickFingerId = -1;
                            OnMovement?.Invoke(Vector2.zero);
                        }
                        break;
                }
            }
        }
    }
}
