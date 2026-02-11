using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace IOChef.Core
{
    /// <summary>
    /// Bootstrap scene initializer: creates all DontDestroyOnLoad managers.
    /// Waits for PlayFabManager login success before transitioning to MainMenu.
    /// Shows ConnectionGateUI overlay while connecting.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Manager Prefabs (optional)")]
        [SerializeField] private GameObject playFabManagerPrefab;
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject inputManagerPrefab;
        [SerializeField] private GameObject screenManagerPrefab;
        [SerializeField] private GameObject analyticsManagerPrefab;
        [SerializeField] private GameObject heroManagerPrefab;
        [SerializeField] private GameObject currencyManagerPrefab;
        [SerializeField] private GameObject shopManagerPrefab;
        [SerializeField] private GameObject battlePassManagerPrefab;
        [SerializeField] private GameObject dailyQuestManagerPrefab;
        [SerializeField] private GameObject dailyLoginManagerPrefab;
        [SerializeField] private GameObject ingredientShopManagerPrefab;
        [SerializeField] private GameObject playerLevelManagerPrefab;
        [SerializeField] private GameObject chestManagerPrefab;
        [SerializeField] private GameObject skinManagerPrefab;
        [SerializeField] private GameObject serverLevelLoaderPrefab;
        [SerializeField] private GameObject dailyDealsManagerPrefab;
        [SerializeField] private GameObject bundleManagerPrefab;

        private bool _sceneLoaded;

        private void Awake()
        {
            try
            {
                Debug.Log("[Bootstrap] Starting initialization");
                InitializeAllManagers();
                Debug.Log("[Bootstrap] All managers created. Waiting for server login...");

                // Create connection gate UI
                SafeCreate<UI.ConnectionGateUI>(null, "ConnectionGateUI");

                // Wait for login before proceeding
                if (PlayFabManager.Instance != null)
                {
                    if (PlayFabManager.Instance.IsLoggedIn)
                    {
                        LoadMainMenu();
                    }
                    else
                    {
                        PlayFabManager.Instance.OnLoginSuccess += LoadMainMenu;
                    }
                }
                else
                {
                    Debug.LogError("[Bootstrap] PlayFabManager not found. Cannot proceed.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Bootstrap] CRITICAL ERROR during init: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void LoadMainMenu()
        {
            if (_sceneLoaded) return;

            // Unsubscribe
            if (PlayFabManager.Instance != null)
                PlayFabManager.Instance.OnLoginSuccess -= LoadMainMenu;

            // Sync level progress from server to local cache
            SyncLevelProgress();

            // Fetch server level configs before loading MainMenu
            if (Gameplay.ServerLevelLoader.Instance != null)
            {
                Gameplay.ServerLevelLoader.Instance.OnLoaded += OnLevelDataReady;
                Gameplay.ServerLevelLoader.Instance.FetchAllWorlds();

                // Timeout fallback: load MainMenu after 10s even if fetch hasn't completed
                StartCoroutine(LevelFetchTimeout(10f));
            }
            else
            {
                DoLoadMainMenu();
            }
        }

        /// <summary>
        /// Fetches LevelProgress from server and caches stars/scores in PlayerPrefs
        /// so LevelSelectUI.IsLevelUnlocked works correctly for returning players.
        /// </summary>
        private void SyncLevelProgress()
        {
            if (PlayFabManager.Instance == null || !PlayFabManager.Instance.IsLoggedIn) return;

            PlayFabManager.Instance.GetUserData(
                new List<string> { "LevelProgress" },
                data =>
                {
                    if (data.TryGetValue("LevelProgress", out string json)
                        && !string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            var progress = Newtonsoft.Json.Linq.JObject.Parse(json);

                            // Cache MaxUnlockedLevel
                            int maxUnlocked = int.TryParse(progress["MaxUnlockedLevel"]?.ToString(), out int mu) ? mu : 1;
                            PlayerPrefs.SetInt("MaxUnlockedLevel", maxUnlocked);

                            // Cache per-level stars and best scores
                            foreach (var prop in progress.Properties())
                            {
                                if (prop.Name.StartsWith("Level_") && prop.Name.EndsWith("_Stars"))
                                {
                                    int stars = int.TryParse(prop.Value?.ToString(), out int s) ? s : 0;
                                    PlayerPrefs.SetInt(prop.Name, stars);
                                }
                                else if (prop.Name.StartsWith("Level_") && prop.Name.EndsWith("_BestScore"))
                                {
                                    int score = int.TryParse(prop.Value?.ToString(), out int sc) ? sc : 0;
                                    PlayerPrefs.SetInt(prop.Name, score);
                                }
                            }
                            PlayerPrefs.Save();
                            Debug.Log($"[Bootstrap] Synced level progress: maxUnlocked={maxUnlocked}");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogWarning($"[Bootstrap] LevelProgress parse error: {ex.Message}");
                        }
                    }
                },
                err => Debug.LogWarning($"[Bootstrap] LevelProgress sync failed: {err}"));
        }

        private System.Collections.IEnumerator LevelFetchTimeout(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            if (!_sceneLoaded)
            {
                Debug.LogWarning("[Bootstrap] Level data fetch timed out, loading MainMenu with local fallback");
                DoLoadMainMenu();
            }
        }

        private void OnLevelDataReady()
        {
            if (Gameplay.ServerLevelLoader.Instance != null)
                Gameplay.ServerLevelLoader.Instance.OnLoaded -= OnLevelDataReady;
            DoLoadMainMenu();
        }

        private void DoLoadMainMenu()
        {
            if (_sceneLoaded) return;
            _sceneLoaded = true;
            Debug.Log("[Bootstrap] Server login + level data confirmed. Loading MainMenu...");
            SceneManager.LoadScene("MainMenu");
        }

        private void InitializeAllManagers()
        {
            SafeCreate<PlayFabManager>(playFabManagerPrefab, "PlayFabManager");
            SafeCreate<GameManager>(gameManagerPrefab, "GameManager");
            SafeCreate<AudioManager>(audioManagerPrefab, "AudioManager");
            SafeCreate<InputManager>(inputManagerPrefab, "InputManager");
            SafeCreate<ScreenManager>(screenManagerPrefab, "ScreenManager");
            SafeCreate<AnalyticsManager>(analyticsManagerPrefab, "AnalyticsManager");
            SafeCreate<Heroes.HeroManager>(heroManagerPrefab, "HeroManager");
            SafeCreate<Economy.CurrencyManager>(currencyManagerPrefab, "CurrencyManager");
            SafeCreate<Economy.ShopManager>(shopManagerPrefab, "ShopManager");
            SafeCreate<Economy.BattlePassManager>(battlePassManagerPrefab, "BattlePassManager");
            SafeCreate<Gameplay.DailyQuestManager>(dailyQuestManagerPrefab, "DailyQuestManager");
            SafeCreate<Gameplay.DailyLoginManager>(dailyLoginManagerPrefab, "DailyLoginManager");
            SafeCreate<Economy.IngredientShopManager>(ingredientShopManagerPrefab, "IngredientShopManager");
            SafeCreate<Economy.PlayerLevelManager>(playerLevelManagerPrefab, "PlayerLevelManager");
            SafeCreate<Economy.ChestManager>(chestManagerPrefab, "ChestManager");
            SafeCreate<Economy.SkinManager>(skinManagerPrefab, "SkinManager");
            SafeCreate<Economy.DailyDealsManager>(dailyDealsManagerPrefab, "DailyDealsManager");
            SafeCreate<Economy.BundleManager>(bundleManagerPrefab, "BundleManager");
            SafeCreate<Gameplay.ServerLevelLoader>(serverLevelLoaderPrefab, "ServerLevelLoader");
        }

        private void SafeCreate<T>(GameObject prefab, string fallbackName) where T : MonoBehaviour
        {
            try
            {
                if (FindAnyObjectByType<T>() != null) return;

                if (prefab != null)
                    Instantiate(prefab);
                else
                {
                    var go = new GameObject(fallbackName);
                    go.AddComponent<T>();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Bootstrap] Failed to create {fallbackName}: {ex.Message}");
            }
        }
    }
}
