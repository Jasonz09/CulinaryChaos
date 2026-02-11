using System;
using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Keyboard-based input provider using WASD/arrow keys, Space/E, and Escape/P.
    /// </summary>
    public class KeyboardInputProvider : MonoBehaviour, IInputProvider
    {
        /// <summary>
        /// Raised each frame with the current movement direction from keyboard input.
        /// </summary>
        public event Action<Vector2> OnMovement;
        /// <summary>
        /// Raised when the player presses Space or E to interact.
        /// </summary>
        public event Action OnInteract;
        /// <summary>
        /// Raised when the player presses Escape or P to pause.
        /// </summary>
        public event Action OnPause;

        /// <summary>
        /// Reads current keyboard state and raises input events accordingly.
        /// </summary>
        public void ProcessInput()
        {
            float h = 0f, v = 0f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v = 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v = -1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1f;

            Vector2 movement = new Vector2(h, v).normalized;
            OnMovement?.Invoke(movement);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
                OnInteract?.Invoke();

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
                OnPause?.Invoke();
        }
    }
}
