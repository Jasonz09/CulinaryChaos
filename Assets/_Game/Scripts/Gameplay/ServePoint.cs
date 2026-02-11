using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Serve point: deliver completed dishes to fulfill orders.
    /// </summary>
    public class ServePoint : InteractiveObject
    {
        [Header("Serve Effects")]
        /// <summary>
        /// Particle or animation effect played when an order is successfully served.
        /// </summary>
        [SerializeField] private GameObject celebrationEffect;
        /// <summary>
        /// Audio clip played when a dish is delivered to this serve point.
        /// </summary>
        [SerializeField] private AudioClip serveSound;

        /// <summary>
        /// Handles player interaction to deliver a plated dish and attempt to match an active order.
        /// </summary>
        /// <param name="player">The player interacting with this serve point.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (!player.IsCarrying) return;

            var item = player.CarriedItem;
            if (item.CurrentState != IngredientState.Plated)
            {
                Debug.Log($"[ServePoint] Rejected: item state is {item.CurrentState}, need Plated");
                return;
            }

            // Try to match against active orders
            var orderQueue = FindAnyObjectByType<OrderQueue>();
            if (orderQueue == null) return;

            bool matched = orderQueue.TryDeliverDish(item);
            if (matched)
            {
                var released = player.ReleaseItem();
                Destroy(released.gameObject);
                Debug.Log("[ServePoint] Order delivered successfully!");
            }
            else
            {
                // Still accept the dish but no points (wrong recipe)
                Debug.Log("[ServePoint] Dish didn't match any active order - trashed");
                var released = player.ReleaseItem();
                Destroy(released.gameObject);
            }
        }

        /// <summary>
        /// Determines whether this serve point can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the item is a plated dish.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return item != null && item.CurrentState == IngredientState.Plated;
        }
    }
}
