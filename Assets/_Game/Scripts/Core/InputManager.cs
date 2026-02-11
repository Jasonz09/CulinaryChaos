using System;
using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton that routes input from the active provider to gameplay systems.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static InputManager Instance { get; private set; }

        /// <summary>
        /// Fires when directional input changes.
        /// </summary>
        public event Action<Vector2> OnMovement;

        /// <summary>
        /// Fires when the interact action is triggered.
        /// </summary>
        public event Action OnInteract;

        /// <summary>
        /// Fires when the pause action is triggered.
        /// </summary>
        public event Action OnPause;

        /// <summary>
        /// The currently active input provider handling player input.
        /// </summary>
        private IInputProvider _currentProvider;

        /// <summary>
        /// Initializes singleton and persists across scenes.
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
        }

        /// <summary>
        /// Sets up the default input provider.
        /// </summary>
        private void Start()
        {
            try
            {
#if UNITY_STANDALONE || UNITY_EDITOR
                SetProvider(gameObject.AddComponent<KeyboardInputProvider>());
#else
                SetProvider(gameObject.AddComponent<TouchInputProvider>());
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InputManager] Failed to set input provider: {e.Message}");
            }
        }

        /// <summary>
        /// Switch to a new input provider.
        /// </summary>
        /// <param name="provider">Input provider to use.</param>
        public void SetProvider(IInputProvider provider)
        {
            if (_currentProvider != null)
            {
                _currentProvider.OnMovement -= HandleMovement;
                _currentProvider.OnInteract -= HandleInteract;
                _currentProvider.OnPause -= HandlePause;
            }

            _currentProvider = provider;
            _currentProvider.OnMovement += HandleMovement;
            _currentProvider.OnInteract += HandleInteract;
            _currentProvider.OnPause += HandlePause;
        }

        /// <summary>
        /// Polls the active input provider each frame.
        /// </summary>
        private void Update()
        {
            try
            {
                _currentProvider?.ProcessInput();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[InputManager] Input error: {e.Message}");
            }
        }

        /// <summary>
        /// Forwards movement input to the player controller.
        /// </summary>
        /// <param name="dir">The movement direction vector.</param>
        private void HandleMovement(Vector2 dir) => OnMovement?.Invoke(dir);

        /// <summary>
        /// Forwards interact input to the player controller.
        /// </summary>
        private void HandleInteract() => OnInteract?.Invoke();

        /// <summary>
        /// Forwards pause input to the game manager.
        /// </summary>
        private void HandlePause() => OnPause?.Invoke();
    }
}
