using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Sink: wash dirty dishes/plates to reuse.
    /// </summary>
    public class Sink : InteractiveObject
    {
        [Header("Wash Settings")]
        /// <summary>
        /// Time in seconds required to wash a dirty item.
        /// </summary>
        [SerializeField] private float washTime = 1.5f;

        /// <summary>
        /// Elapsed time in seconds since washing started.
        /// </summary>
        private float _washTimer;
        /// <summary>
        /// Whether an item is currently being washed.
        /// </summary>
        private bool _isWashing;

        /// <summary>
        /// Handles player interaction to place a burned item for washing or pick up a clean one.
        /// </summary>
        /// <param name="player">The player interacting with this sink.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying && CanAcceptItem(player.CarriedItem))
            {
                var item = player.ReleaseItem();
                PlaceItem(item);
                StartWashing();
            }
            else if (!player.IsCarrying && CurrentState == ObjectState.Complete && HeldItem != null)
            {
                var item = RemoveItem();
                player.PickupItem(item);
            }
        }

        /// <summary>
        /// Begins washing the dirty item.
        /// </summary>
        private void StartWashing()
        {
            _isWashing = true;
            _washTimer = 0f;
            CurrentState = ObjectState.Cooking; // "Processing"
        }

        /// <summary>
        /// Advances the wash timer and checks for completion.
        /// </summary>
        private void Update()
        {
            if (!_isWashing) return;

            _washTimer += Time.deltaTime;
            if (_washTimer >= washTime)
            {
                CompleteWashing();
            }
        }

        /// <summary>
        /// Finishes washing and resets the item to a clean state.
        /// </summary>
        private void CompleteWashing()
        {
            _isWashing = false;
            if (HeldItem != null)
                HeldItem.SetState(IngredientState.Raw); // Clean plate, ready to reuse
            CurrentState = ObjectState.Complete;
        }

        /// <summary>
        /// Determines whether this sink can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the sink is empty and the ingredient is burned.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return CurrentState == ObjectState.Empty &&
                   HeldItem == null &&
                   item != null &&
                   item.CurrentState == IngredientState.Burned; // Only accept dirty/burned items
        }
    }
}
