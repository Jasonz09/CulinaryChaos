using UnityEngine;
using UnityEngine.SceneManagement;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton that manages game state, scene loading, and pause/resume.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance.
        /// </summary>
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// Current game state.
        /// </summary>
        public GameState CurrentGameState { get; private set; }

        /// <summary>
        /// ID of the currently loaded level.
        /// </summary>
        public int CurrentLevelId { get; private set; }

        /// <summary>
        /// ID of the currently selected world.
        /// </summary>
        public int CurrentWorldId { get; private set; } = 1;

        /// <summary>
        /// Set the active world ID.
        /// </summary>
        /// <param name="worldId">World to make current.</param>
        public void SetCurrentWorld(int worldId) { CurrentWorldId = worldId; }

        /// <summary>
        /// Initializes the singleton instance and persists across scenes.
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
        /// Transition to a new game state, adjusting time scale as needed.
        /// </summary>
        /// <param name="newState">Target game state.</param>
        public void SetGameState(GameState newState)
        {
            CurrentGameState = newState;

            switch (newState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
            }
        }

        /// <summary>
        /// Load a gameplay level by ID.
        /// </summary>
        /// <param name="levelId">Level to load.</param>
        public void LoadLevel(int levelId)
        {
            CurrentLevelId = levelId;
            SetGameState(GameState.Playing);
            SceneManager.LoadScene("Kitchen_Level_1");
        }

        /// <summary>
        /// Return to the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            SetGameState(GameState.MainMenu);
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// Load the level select / world map scene.
        /// </summary>
        public void LoadLevelSelect()
        {
            SetGameState(GameState.LevelSelect);
            SceneManager.LoadScene("LevelSelect");
        }

        /// <summary>
        /// Load the results screen scene.
        /// </summary>
        public void LoadResults()
        {
            SetGameState(GameState.Results);
            SceneManager.LoadScene("Results");
        }

        /// <summary>
        /// Pause gameplay if currently playing.
        /// </summary>
        public void PauseGame()
        {
            if (CurrentGameState == GameState.Playing)
                SetGameState(GameState.Paused);
        }

        /// <summary>
        /// Resume gameplay if currently paused.
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentGameState == GameState.Paused)
                SetGameState(GameState.Playing);
        }

        /// <summary>
        /// End the current level and freeze time.
        /// </summary>
        public void GameOver()
        {
            SetGameState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// Force-reload the entire game back to Bootstrap for fresh server login.
        /// Called when connection timeout expires (30s without server).
        /// </summary>
        public void ForceReloadGame()
        {
            Time.timeScale = 1f;
            CurrentGameState = GameState.MainMenu;
            Debug.Log("[GameManager] Force reloading to Bootstrap");
            SceneManager.LoadScene("Bootstrap");
        }
    }
}
