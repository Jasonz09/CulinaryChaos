using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IOChef.UI
{
    /// <summary>
    /// Pause menu overlay during gameplay.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        /// <summary>
        /// Root panel GameObject for the pause menu overlay.
        /// </summary>
        [Header("Panel")]
        [SerializeField] private GameObject pausePanel;

        /// <summary>
        /// Button to resume gameplay from the pause menu.
        /// </summary>
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;

        /// <summary>
        /// Button to retry the current level.
        /// </summary>
        [SerializeField] private Button retryButton;

        /// <summary>
        /// Button to open the settings panel.
        /// </summary>
        [SerializeField] private Button settingsButton;

        /// <summary>
        /// Button to quit to the main menu.
        /// </summary>
        [SerializeField] private Button quitButton;

        /// <summary>
        /// Panel GameObject containing the settings controls.
        /// </summary>
        [Header("Settings")]
        [SerializeField] private GameObject settingsPanel;

        /// <summary>
        /// Slider controlling the music volume.
        /// </summary>
        [SerializeField] private Slider musicSlider;

        /// <summary>
        /// Slider controlling the sound effects volume.
        /// </summary>
        [SerializeField] private Slider sfxSlider;

        /// <summary>
        /// Wires up button click listeners and hides the pause panel.
        /// </summary>
        private void Start()
        {
            resumeButton?.onClick.AddListener(OnResumeClicked);
            retryButton?.onClick.AddListener(OnRetryClicked);
            settingsButton?.onClick.AddListener(() => settingsPanel?.SetActive(true));
            quitButton?.onClick.AddListener(OnQuitClicked);

            pausePanel?.SetActive(false);
        }

        /// <summary>
        /// Subscribes to the pause input event.
        /// </summary>
        private void OnEnable()
        {
            if (Core.InputManager.Instance != null)
                Core.InputManager.Instance.OnPause += TogglePause;
        }

        /// <summary>
        /// Unsubscribes from the pause input event.
        /// </summary>
        private void OnDisable()
        {
            if (Core.InputManager.Instance != null)
                Core.InputManager.Instance.OnPause -= TogglePause;
        }

        /// <summary>
        /// Toggles the pause menu visibility and game time scale.
        /// </summary>
        private void TogglePause()
        {
            if (pausePanel == null) return;

            if (pausePanel.activeSelf)
                OnResumeClicked();
            else
                ShowPause();
        }

        /// <summary>
        /// Shows the pause panel and freezes game time.
        /// </summary>
        private void ShowPause()
        {
            pausePanel?.SetActive(true);
            Core.GameManager.Instance?.PauseGame();
        }

        /// <summary>
        /// Resumes gameplay and hides the pause menu.
        /// </summary>
        private void OnResumeClicked()
        {
            pausePanel?.SetActive(false);
            settingsPanel?.SetActive(false);
            Core.GameManager.Instance?.ResumeGame();
        }

        /// <summary>
        /// Reloads the current scene to restart the level.
        /// </summary>
        private void OnRetryClicked()
        {
            Core.GameManager.Instance?.ResumeGame();
            int levelId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            Core.GameManager.Instance?.LoadLevel(levelId);
        }

        /// <summary>
        /// Returns to the main menu scene.
        /// </summary>
        private void OnQuitClicked()
        {
            Core.GameManager.Instance?.ResumeGame();
            Core.GameManager.Instance?.LoadMainMenu();
        }
    }
}
