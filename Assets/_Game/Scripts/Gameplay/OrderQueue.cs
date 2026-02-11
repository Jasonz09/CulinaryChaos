using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Spawns, tracks, and expires customer orders during gameplay.
    /// </summary>
    public class OrderQueue : MonoBehaviour
    {
        /// <summary>
        /// Seconds between automatic order spawns.
        /// </summary>
        [Header("Settings")]
        [SerializeField] private float orderSpawnInterval = 60f;

        /// <summary>
        /// Maximum number of orders that can be active simultaneously.
        /// </summary>
        [SerializeField] private int maxActiveOrders = 4;

        /// <summary>
        /// Pool of recipes from which new orders are randomly selected.
        /// </summary>
        [Header("Available Recipes")]
        [SerializeField] private List<RecipeSO> availableRecipes;

        /// <summary>
        /// List of currently active customer orders.
        /// </summary>
        private List<Order> _activeOrders = new();

        /// <summary>
        /// Timer tracking seconds until the next order spawns.
        /// </summary>
        private float _spawnTimer;

        /// <summary>
        /// Read-only list of current active orders.
        /// </summary>
        public IReadOnlyList<Order> ActiveOrders => _activeOrders;

        /// <summary>
        /// Fires when a new order is created.
        /// </summary>
        public event System.Action<Order> OnOrderSpawned;

        /// <summary>
        /// Fires when an order is successfully delivered.
        /// </summary>
        public event System.Action<Order> OnOrderCompleted;

        /// <summary>
        /// Fires when an order's time runs out.
        /// </summary>
        public event System.Action<Order> OnOrderExpired;

        /// <summary>
        /// Configure the queue from level data.
        /// </summary>
        /// <param name="levelData">Level configuration.</param>
        public void Initialize(LevelDataSO levelData)
        {
            availableRecipes = new List<RecipeSO>(levelData.availableRecipes);
            orderSpawnInterval = levelData.orderSpawnInterval;
            maxActiveOrders = levelData.maxActiveOrders;
            _activeOrders.Clear();
            _spawnTimer = 0f;
        }

        /// <summary>
        /// Configure the queue with explicit parameters.
        /// </summary>
        /// <param name="recipes">Available recipe pool.</param>
        /// <param name="interval">Seconds between order spawns.</param>
        /// <param name="maxOrders">Maximum concurrent active orders.</param>
        public void InitializeDefault(List<RecipeSO> recipes, float interval, int maxOrders)
        {
            availableRecipes = new List<RecipeSO>(recipes);
            orderSpawnInterval = interval;
            maxActiveOrders = maxOrders;
            _activeOrders.Clear();
            _spawnTimer = 0f;
        }

        /// <summary>
        /// Ticks spawn timer and expires overdue orders each frame.
        /// </summary>
        private void Update()
        {
            if (Core.GameManager.Instance != null &&
                Core.GameManager.Instance.CurrentGameState != Core.GameState.Playing)
                return;

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= orderSpawnInterval && _activeOrders.Count < maxActiveOrders)
            {
                SpawnOrder();
                _spawnTimer = 0f;
            }

            // Update order timers
            for (int i = _activeOrders.Count - 1; i >= 0; i--)
            {
                _activeOrders[i].remainingTime -= Time.deltaTime;
                if (_activeOrders[i].remainingTime <= 0f)
                {
                    var expired = _activeOrders[i];
                    _activeOrders.RemoveAt(i);
                    OnOrderExpired?.Invoke(expired);
                }
            }
        }

        /// <summary>
        /// Creates a new random order from the available recipe pool.
        /// </summary>
        private void SpawnOrder()
        {
            if (availableRecipes.Count == 0) return;

            var recipe = availableRecipes[Random.Range(0, availableRecipes.Count)];
            var order = new Order
            {
                recipe = recipe,
                pointsReward = recipe.pointsForCompletion,
                timeLimit = recipe.timeLimitSeconds,
                remainingTime = recipe.timeLimitSeconds
            };

            _activeOrders.Add(order);
            OnOrderSpawned?.Invoke(order);
        }

        /// <summary>
        /// Force spawn an order immediately (for tutorial/events).
        /// </summary>
        public void ForceSpawnOrder()
        {
            if (_activeOrders.Count < maxActiveOrders)
                SpawnOrder();
        }

        /// <summary>
        /// Try to match a delivered dish against active orders.
        /// </summary>
        /// <param name="dish">The plated dish to deliver.</param>
        /// <returns>True if a matching order was found and completed.</returns>
        public bool TryDeliverDish(Ingredient dish)
        {
            Debug.Log($"[OrderQueue] TryDeliverDish: type={dish.Type} state={dish.CurrentState} platedCount={dish.PlatedContents?.Count ?? 0}");
            if (dish.PlatedContents != null)
            {
                foreach (var c in dish.PlatedContents)
                    Debug.Log($"  -> plate has: {c.Type} {c.CurrentState}");
            }

            for (int i = 0; i < _activeOrders.Count; i++)
            {
                Debug.Log($"  checking order: {_activeOrders[i].recipe.recipeName}");
                if (_activeOrders[i].recipe.MatchesDish(dish))
                {
                    var completed = _activeOrders[i];
                    _activeOrders.RemoveAt(i);
                    OnOrderCompleted?.Invoke(completed);

                    // Immediately spawn replacement if below max
                    if (_activeOrders.Count < maxActiveOrders)
                        SpawnOrder();

                    return true;
                }
            }
            Debug.Log("[OrderQueue] No matching order found!");
            return false;
        }
    }
}
