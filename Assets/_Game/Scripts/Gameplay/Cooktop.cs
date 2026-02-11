using UnityEngine;
using IOChef.Core;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Cooktop: cooks ingredients over time. Can burn if left too long.
    /// </summary>
    public class Cooktop : InteractiveObject
    {
        [Header("Cook Settings")]
        /// <summary>
        /// Time in seconds required to fully cook an ingredient.
        /// </summary>
        [SerializeField] private float cookTime = 3f;
        /// <summary>
        /// Additional time in seconds after cooking before the ingredient burns.
        /// </summary>
        [SerializeField] private float burnTime = 5f;
        /// <summary>
        /// Visual flame effect displayed while cooking is active.
        /// </summary>
        [SerializeField] private GameObject flameEffect;

        /// <summary>
        /// Elapsed time in seconds since cooking started.
        /// </summary>
        private float _cookTimer;
        /// <summary>
        /// Whether an ingredient is currently being cooked.
        /// </summary>
        private bool _isCooking;
        /// <summary>
        /// Whether the ingredient on this cooktop has burned.
        /// </summary>
        private bool _isBurning;

        /// <summary>
        /// Gets the normalized cooking progress from 0 to 1.
        /// </summary>
        public float CookProgress => _isCooking ? Mathf.Clamp01(_cookTimer / cookTime) : 0f;

        /// <summary>
        /// Gets whether the ingredient on this cooktop has burned.
        /// </summary>
        public bool IsBurning => _isBurning;

        /// <summary>
        /// Handles player interaction to place an ingredient for cooking or pick up a cooked item.
        /// </summary>
        /// <param name="player">The player interacting with this cooktop.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying && CanAcceptItem(player.CarriedItem))
            {
                var item = player.ReleaseItem();
                PlaceItem(item);
                StartCooking();
            }
            else if (!player.IsCarrying && HeldItem != null &&
                    (CurrentState == ObjectState.Complete || CurrentState == ObjectState.HasIngredient))
            {
                StopCooking();
                var item = RemoveItem();
                player.PickupItem(item);
            }
        }

        /// <summary>
        /// Begins cooking the placed ingredient.
        /// </summary>
        private void StartCooking()
        {
            _isCooking = true;
            _isBurning = false;
            _cookTimer = 0f;
            CurrentState = ObjectState.Cooking;

            if (flameEffect != null)
                flameEffect.SetActive(true);
        }

        /// <summary>
        /// Stops the cooking process and resets state.
        /// </summary>
        private void StopCooking()
        {
            _isCooking = false;
            _isBurning = false;

            if (flameEffect != null)
                flameEffect.SetActive(false);
        }

        /// <summary>
        /// Advances the cook timer and handles cooked/burned transitions.
        /// </summary>
        private void Update()
        {
            if (!_isCooking || HeldItem == null) return;

            // Apply hero cook time modifier
            float cookMultiplier = 1f;
            float burnMultiplier = 1f;
            if (Heroes.HeroManager.Instance != null)
            {
                var mods = Heroes.HeroManager.Instance.GetActiveModifiers();
                cookMultiplier = mods.cookTimeMultiplier;
                burnMultiplier = mods.burnTimeMultiplier;
            }

            _cookTimer += Time.deltaTime / cookMultiplier;

            if (!_isBurning && _cookTimer >= cookTime)
            {
                // Cooking complete
                HeldItem.SetState(IngredientState.Cooked);
                CurrentState = ObjectState.Complete;

                if (AudioManager.Instance != null)
                {
                    // Play ding sound
                }
            }

            // IgnoreBurn ability: adds extra grace seconds before burn timer starts
            float burnGrace = 0f;
            if (Heroes.HeroManager.Instance != null
                && Heroes.HeroManager.Instance.GetActiveSpecialAbilityType() == Heroes.SpecialAbilityType.IgnoreBurn)
            {
                burnGrace = Heroes.HeroManager.Instance.GetActiveSpecialAbilityValue();
            }

            float effectiveBurnTime = burnTime * burnMultiplier + burnGrace;
            if (_cookTimer >= cookTime + effectiveBurnTime)
            {
                // Burned!
                _isBurning = true;
                HeldItem.SetState(IngredientState.Burned);
                CurrentState = ObjectState.Dirty;
                _isCooking = false;

                if (AudioManager.Instance != null)
                {
                    // Play burn sound
                }
            }
        }

        /// <summary>
        /// Determines whether this cooktop can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the cooktop is empty and the ingredient is raw or chopped.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return CurrentState == ObjectState.Empty &&
                   HeldItem == null &&
                   item != null &&
                   (item.CurrentState == IngredientState.Raw || item.CurrentState == IngredientState.Chopped);
        }
    }
}
