using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Cutting board: chops raw ingredients.
    /// Player interacts to start chopping (requires time), then picks up chopped result.
    /// </summary>
    public class CuttingBoard : InteractiveObject
    {
        [Header("Cutting Settings")]
        /// <summary>
        /// Time in seconds required to chop an ingredient.
        /// </summary>
        [SerializeField] private float chopTime = 2f;
        /// <summary>
        /// Prefab used to display the chopping progress bar.
        /// </summary>
        [SerializeField] private GameObject progressBarPrefab;

        /// <summary>
        /// Elapsed time in seconds since chopping started.
        /// </summary>
        private float _chopTimer;
        /// <summary>
        /// Whether an ingredient is currently being chopped.
        /// </summary>
        private bool _isChopping;
        /// <summary>
        /// Instantiated progress bar shown during chopping.
        /// </summary>
        private GameObject _progressBarInstance;

        /// <summary>
        /// Handles player interaction to place a raw ingredient or pick up a chopped one.
        /// </summary>
        /// <param name="player">The player interacting with this cutting board.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying && CanAcceptItem(player.CarriedItem))
            {
                // Only accept raw ingredients
                if (player.CarriedItem.CurrentState == IngredientState.Raw)
                {
                    var item = player.ReleaseItem();
                    PlaceItem(item);
                    StartChopping();
                }
            }
            else if (!player.IsCarrying && CurrentState == ObjectState.Complete && HeldItem != null)
            {
                var item = RemoveItem();
                player.PickupItem(item);
            }
        }

        /// <summary>
        /// Begins chopping the placed ingredient.
        /// </summary>
        private void StartChopping()
        {
            _isChopping = true;
            _chopTimer = 0f;
            CurrentState = ObjectState.Cooking; // "Processing"
        }

        /// <summary>
        /// Advances the chop timer and checks for completion.
        /// </summary>
        private void Update()
        {
            if (!_isChopping) return;

            _chopTimer += Time.deltaTime;
            float progress = _chopTimer / chopTime;

            // TODO: Update visual progress bar

            if (_chopTimer >= chopTime)
            {
                CompleteChopping();
            }
        }

        /// <summary>
        /// Finishes chopping and sets the ingredient to chopped state.
        /// </summary>
        private void CompleteChopping()
        {
            _isChopping = false;
            if (HeldItem != null)
            {
                HeldItem.SetState(IngredientState.Chopped);
            }
            CurrentState = ObjectState.Complete;
        }

        /// <summary>
        /// Determines whether this cutting board can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the board is empty and the ingredient is raw.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return CurrentState == ObjectState.Empty &&
                   HeldItem == null &&
                   item != null &&
                   item.CurrentState == IngredientState.Raw;
        }
    }
}
