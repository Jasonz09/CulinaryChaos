using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Ingredient source: spawns raw ingredients for the player to pick up.
    /// Deducts 1 from the player's ingredient stock on each pickup.
    /// </summary>
    public class IngredientSource : InteractiveObject
    {
        [Header("Source Settings")]
        /// <summary>
        /// Type of ingredient this source spawns.
        /// </summary>
        [SerializeField] private IngredientType sourceType;
        /// <summary>
        /// Prefab instantiated when a player picks up an ingredient from this source.
        /// </summary>
        [SerializeField] private GameObject ingredientPrefab;

        /// <summary>
        /// Gets the type of ingredient this source provides.
        /// </summary>
        public IngredientType SourceType => sourceType;

        /// <summary>
        /// Sets the type of ingredient this source provides.
        /// </summary>
        /// <param name="type">The ingredient type to assign.</param>
        public void SetSourceType(IngredientType type) { sourceType = type; }

        /// <summary>
        /// Handles player interaction to spawn and pick up a new raw ingredient.
        /// Deducts 1 unit from the ingredient stock via IngredientShopManager.
        /// </summary>
        /// <param name="player">The player interacting with this source.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying) return;

            // FreeIngredient ability: chance to skip stock consumption
            bool freePickup = false;
            if (Heroes.HeroManager.Instance != null
                && Heroes.HeroManager.Instance.GetActiveSpecialAbilityType() == Heroes.SpecialAbilityType.FreeIngredient)
            {
                float chance = Heroes.HeroManager.Instance.GetActiveSpecialAbilityValue();
                if (Random.value < chance)
                    freePickup = true;
            }

            // Deduct 1 from stock (unless free pickup)
            if (!freePickup
                && Economy.IngredientShopManager.Instance != null
                && !Economy.IngredientShopManager.Instance.ConsumeIngredient(sourceType))
            {
                Debug.Log($"[IngredientSource] Out of stock: {sourceType}");
                return;
            }

            // Spawn a new ingredient for the player
            Ingredient newIngredient;
            if (ingredientPrefab != null)
            {
                var go = Instantiate(ingredientPrefab, transform.position, Quaternion.identity);
                newIngredient = go.GetComponent<Ingredient>();
                newIngredient.Initialize(sourceType, IngredientState.Raw);
            }
            else
            {
                var go = new GameObject($"Ingredient_{sourceType}");
                go.AddComponent<SpriteRenderer>();
                newIngredient = go.AddComponent<Ingredient>();
                newIngredient.Initialize(sourceType, IngredientState.Raw);
            }

            player.PickupItem(newIngredient);
        }

        /// <summary>
        /// Determines whether this source can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>Always returns false; sources do not accept items.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return false; // Sources don't accept items
        }
    }
}
