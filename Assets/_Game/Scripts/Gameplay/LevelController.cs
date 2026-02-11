using UnityEngine;
using System.Collections.Generic;
using IOChef.Core;
using Newtonsoft.Json.Linq;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Level controller: orchestrates a kitchen level based on CurrentLevelId.
    /// Loads level data first, destroys scene-placed stations, then builds the kitchen
    /// from the level's equipment layout before initializing gameplay systems.
    /// Server-authoritative: all rewards and progress recording go through CloudScript.
    /// </summary>
    public class LevelController : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private LevelDatabase levelDatabase;

        [Header("References")]
        [SerializeField] private KitchenGrid kitchenGrid;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private OrderQueue orderQueue;
        [SerializeField] private ScoreCalculator scoreCalculator;
        [SerializeField] private PlayerController player;

        private LevelResults _results;
        public LevelResults Results => _results;

        private int _levelId = 1;
        private LevelDataSO _currentLevel;

        /// <summary>Number of order-undo charges remaining (from UndoOrder hero ability).</summary>
        private int _undosRemaining;

        /// <summary>Extra order failures allowed before game-over (from ExtraLives hero ability).</summary>
        private int _extraLives;

        public static LevelController Instance { get; private set; }
        public static LevelResults LastRunResults { get; private set; }
        public static bool LastRunWasNewBest { get; private set; }

        public bool UnlimitedPlates => _currentLevel != null && _currentLevel.unlimitedPlates;
        public bool AutoRemovePlates => _currentLevel == null || _currentLevel.autoRemovePlates;
        public bool RequiresSink => _currentLevel != null && _currentLevel.requiresSink;
        public int PlateCount => _currentLevel != null ? _currentLevel.plateCount : 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (kitchenGrid == null) kitchenGrid = FindAnyObjectByType<KitchenGrid>();
            if (gameTimer == null) gameTimer = FindAnyObjectByType<GameTimer>();
            if (orderQueue == null) orderQueue = FindAnyObjectByType<OrderQueue>();
            if (scoreCalculator == null) scoreCalculator = FindAnyObjectByType<ScoreCalculator>();
            if (player == null) player = FindAnyObjectByType<PlayerController>();

            _levelId = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentLevelId : 1;
            _currentLevel = LoadLevel(_levelId);

            if (_currentLevel == null)
            {
                Debug.LogError($"[LevelController] No level data found for level {_levelId}");
                return;
            }

            DestroyScenePlacedStations();
            BuildKitchen();
            InitializeGameplay();
        }

        private void DestroyScenePlacedStations()
        {
            var sceneStations = FindObjectsByType<InteractiveObject>(FindObjectsSortMode.None);
            foreach (var station in sceneStations)
            {
                Debug.Log($"[LevelController] Destroying scene-placed station: {station.gameObject.name}");
                Destroy(station.gameObject);
            }
        }

        private void BuildKitchen()
        {
            if (kitchenGrid != null)
                kitchenGrid.Reinitialize(_currentLevel.gridWidth, _currentLevel.gridHeight);

            var existing = FindAnyObjectByType<KitchenVisualSetup>();
            if (existing != null)
                Destroy(existing.gameObject);

            var visGO = new GameObject("KitchenVisualSetup");
            var vis = visGO.AddComponent<KitchenVisualSetup>();
            vis.BuildKitchen(_currentLevel);
        }

        private void InitializeGameplay()
        {
            Core.GameManager.Instance?.SetCurrentWorld(_currentLevel.worldId);

            Debug.Log($"[LevelController] Level {_levelId} '{_currentLevel.levelName}': " +
                      $"grid={_currentLevel.gridWidth}x{_currentLevel.gridHeight}, " +
                      $"{_currentLevel.timeLimitSeconds}s, {_currentLevel.availableRecipes.Count} recipes, " +
                      $"interval={_currentLevel.orderSpawnInterval}s, maxOrders={_currentLevel.maxActiveOrders}, " +
                      $"plates={(_currentLevel.unlimitedPlates ? "unlimited" : _currentLevel.plateCount.ToString())}, " +
                      $"sink={_currentLevel.requiresSink}");

            if (gameTimer != null)
            {
                gameTimer.Initialize(_currentLevel.timeLimitSeconds);
                gameTimer.OnTimerExpired += OnTimerExpired;
            }

            if (orderQueue != null)
            {
                orderQueue.InitializeDefault(
                    _currentLevel.availableRecipes,
                    _currentLevel.orderSpawnInterval,
                    _currentLevel.maxActiveOrders);
                orderQueue.OnOrderCompleted += OnOrderCompleted;
                orderQueue.OnOrderExpired += OnOrderExpired;
            }

            if (scoreCalculator != null)
                scoreCalculator.Initialize();

            // Initialize hero ability charges
            _undosRemaining = 0;
            _extraLives = 0;
            if (Heroes.HeroManager.Instance != null)
            {
                var abilityType = Heroes.HeroManager.Instance.GetActiveSpecialAbilityType();
                float abilityValue = Heroes.HeroManager.Instance.GetActiveSpecialAbilityValue();
                if (abilityType == Heroes.SpecialAbilityType.UndoOrder)
                    _undosRemaining = Mathf.RoundToInt(abilityValue);
                else if (abilityType == Heroes.SpecialAbilityType.ExtraLives)
                    _extraLives = Mathf.RoundToInt(abilityValue);
            }

            Core.GameManager.Instance?.SetGameState(Core.GameState.Playing);
            if (gameTimer != null)
                gameTimer.StartTimer();

            for (int i = 0; i < _currentLevel.initialOrders; i++)
                Invoke(nameof(SpawnFirstOrder), 2f + i * 0.8f);
        }

        private LevelDataSO LoadLevel(int levelId)
        {
            // 1. Server configs (authoritative)
            if (ServerLevelLoader.Instance != null && ServerLevelLoader.Instance.IsLoaded)
            {
                var level = ServerLevelLoader.Instance.GetLevel(levelId);
                if (level != null) return level;
            }

            // 2. Local database (editor / fallback)
            if (levelDatabase != null)
            {
                var level = levelDatabase.GetLevel(levelId);
                if (level != null) return level;
            }

            // 3. Hardcoded factory (dev fallback)
            Debug.Log($"[LevelController] No server/database entry for level {levelId}, using DefaultLevelFactory");
            return DefaultLevelFactory.Create(levelId);
        }

        // ═══════════════════════════════════════════════════════
        //  EVENTS
        // ═══════════════════════════════════════════════════════

        private void SpawnFirstOrder()
        {
            if (orderQueue != null) orderQueue.ForceSpawnOrder();
        }

        /// <summary>
        /// Handles a completed order by updating the score tracker.
        /// No direct currency grants — all rewards are handled by the
        /// CompleteLevel CloudScript at end of level.
        /// </summary>
        private void OnOrderCompleted(Order order)
        {
            Debug.Log($"[LevelController] Order completed: {order.recipe.recipeName}");
            if (scoreCalculator != null) scoreCalculator.OrderCompleted(order);
        }

        private void OnOrderExpired(Order order)
        {
            Debug.Log($"[LevelController] Order expired: {order.recipe.recipeName}");

            // UndoOrder ability: cancel the failure entirely
            if (_undosRemaining > 0)
            {
                _undosRemaining--;
                Debug.Log($"[LevelController] UndoOrder used! {_undosRemaining} remaining");
                return; // Skip penalty
            }

            if (scoreCalculator != null) scoreCalculator.OrderFailed();

            // ExtraLives ability: absorb excess failures
            if (_extraLives > 0 && scoreCalculator != null && scoreCalculator.OrdersFailed > 3)
            {
                _extraLives--;
                Debug.Log($"[LevelController] ExtraLives absorbed failure! {_extraLives} remaining");
            }
        }

        private void OnTimerExpired() => EndLevel();

        /// <summary>
        /// Calculates final results and delegates all reward granting + progress
        /// recording to the CompleteLevel CloudScript handler. Server validates
        /// score against Title Data level configs and grants coins, XP, BP XP,
        /// hero unlocks, and updates best score/stars/max unlocked.
        /// </summary>
        private void EndLevel()
        {
            int score = scoreCalculator != null ? scoreCalculator.CurrentScore : 0;
            int stars = _currentLevel.GetStarRating(score);

            _results = new LevelResults
            {
                finalScore = score,
                ordersCompleted = scoreCalculator?.OrdersCompleted ?? 0,
                ordersFailed = scoreCalculator?.OrdersFailed ?? 0,
                bestCombo = scoreCalculator?.BestCombo ?? 0,
                starRating = stars
            };

            // Flush ingredient consumption to server
            if (Economy.IngredientShopManager.Instance != null)
                Economy.IngredientShopManager.Instance.FlushToServer();

            // Send results to server — server handles ALL reward granting and progress saving
            PlayFabManager.Instance?.ExecuteCloudScript("CompleteLevel",
                new
                {
                    levelId = _levelId,
                    score,
                    stars,
                    ordersCompleted = _results.ordersCompleted,
                    ordersFailed = _results.ordersFailed,
                    bestCombo = _results.bestCombo,
                    freeHeroRewardId = _currentLevel.freeHeroRewardId ?? ""
                },
                resultJson =>
                {
                    try
                    {
                        var result = JObject.Parse(resultJson);
                        bool newBest = result["newBest"]?.Value<bool>() ?? false;
                        LastRunWasNewBest = newBest;

                        // Update local cache from server response
                        if (newBest)
                        {
                            PlayerPrefs.SetInt($"Level_{_levelId}_BestScore", result["bestScore"]?.Value<int>() ?? score);
                            PlayerPrefs.SetInt($"Level_{_levelId}_Stars", result["bestStars"]?.Value<int>() ?? stars);
                        }
                        int maxUnlocked = result["maxUnlockedLevel"]?.Value<int>() ?? 0;
                        if (maxUnlocked > 0)
                            PlayerPrefs.SetInt("MaxUnlockedLevel", maxUnlocked);
                        PlayerPrefs.Save();

                        // Refresh currency balances from server
                        PlayFabManager.Instance?.RefreshCurrencies();

                        // Notify hero manager if a hero was unlocked
                        string unlockedHero = result["unlockedHeroId"]?.Value<string>();
                        if (!string.IsNullOrEmpty(unlockedHero))
                            Heroes.HeroManager.Instance?.UnlockHero(unlockedHero);

                        Debug.Log($"[LevelController] Server CompleteLevel: newBest={newBest}, coins={result["coinReward"]}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[LevelController] CompleteLevel parse error: {ex.Message}");
                    }
                },
                err => Debug.LogWarning($"[LevelController] Server CompleteLevel failed: {err}"));

            LastRunResults = _results;
            Core.GameManager.Instance?.LoadResults();
        }

        private void OnDestroy()
        {
            Instance = null;
            if (gameTimer != null) gameTimer.OnTimerExpired -= OnTimerExpired;
            if (orderQueue != null)
            {
                orderQueue.OnOrderCompleted -= OnOrderCompleted;
                orderQueue.OnOrderExpired -= OnOrderExpired;
            }
        }
    }
}
