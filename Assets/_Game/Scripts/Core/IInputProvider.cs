using System;
using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Abstraction for input sources (keyboard, touch, etc.).
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>
        /// Fires when directional input changes.
        /// </summary>
        event Action<Vector2> OnMovement;

        /// <summary>
        /// Fires when the interact action is triggered.
        /// </summary>
        event Action OnInteract;

        /// <summary>
        /// Fires when the pause action is triggered.
        /// </summary>
        event Action OnPause;

        /// <summary>
        /// Poll and dispatch input events for the current frame.
        /// </summary>
        void ProcessInput();
    }
}
