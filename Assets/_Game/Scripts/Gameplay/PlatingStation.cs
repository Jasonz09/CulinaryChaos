using UnityEngine;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Plating station: combine cooked/chopped ingredients into a finished dish.
    /// </summary>
    public class PlatingStation : InteractiveObject
    {
        [Header("Plating")]
        /// <summary>
        /// Maximum number of ingredients that can be placed on a single plate.
        /// </summary>
        [SerializeField] private int maxIngredients = 4;

        /// <summary>
        /// Ingredients currently placed on the plate.
        /// </summary>
        private System.Collections.Generic.List<Ingredient> _platedIngredients = new();

        /// <summary>
        /// Gets the number of ingredients currently on the plate.
        /// </summary>
        public int PlatedCount => _platedIngredients.Count;

        /// <summary>
        /// Handles player interaction to add an ingredient to the plate or pick up the plated dish.
        /// </summary>
        /// <param name="player">The player interacting with this plating station.</param>
        public override void OnPlayerInteract(PlayerController player)
        {
            if (player.IsCarrying)
            {
                var item = player.CarriedItem;
                if (item.CurrentState == IngredientState.Cooked ||
                    item.CurrentState == IngredientState.Chopped)
                {
                    if (_platedIngredients.Count < maxIngredients)
                    {
                        var released = player.ReleaseItem();
                        _platedIngredients.Add(released);
                        released.gameObject.SetActive(false);
                        CurrentState = ObjectState.HasIngredient;
                        UpdatePlateVisual();
                    }
                }
            }
            else if (!player.IsCarrying && _platedIngredients.Count > 0)
            {
                // Pick up the plated dish as a combined ingredient
                var dish = CreateDish();
                player.PickupItem(dish);
                _platedIngredients.Clear();
                CurrentState = ObjectState.Empty;
                UpdatePlateVisual();
            }
        }

        /// <summary>
        /// Updates the visual counter showing plated ingredient count.
        /// </summary>
        private void UpdatePlateVisual()
        {
            var existing = transform.Find("PlateCount");
            if (_platedIngredients.Count == 0)
            {
                if (existing != null) existing.gameObject.SetActive(false);
                return;
            }

            TextMesh tm;
            if (existing != null)
            {
                existing.gameObject.SetActive(true);
                tm = existing.GetComponent<TextMesh>();
            }
            else
            {
                var lgo = new GameObject("PlateCount");
                lgo.transform.SetParent(transform);
                lgo.transform.localPosition = new Vector3(0, 0.6f, 0);
                lgo.transform.localScale = Vector3.one;
                tm = lgo.AddComponent<TextMesh>();
                tm.fontSize = 36;
                tm.characterSize = 0.12f;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.alignment = TextAlignment.Center;
                lgo.GetComponent<MeshRenderer>().sortingOrder = 22;
            }
            tm.text = $"\u25CF {_platedIngredients.Count}";
            tm.color = new Color(1f, 0.9f, 0.3f);
        }

        /// <summary>
        /// Creates a plated dish ingredient from the current plate contents.
        /// </summary>
        /// <returns>A new ingredient representing the plated dish.</returns>
        private Ingredient CreateDish()
        {
            var dishGO = new GameObject("PlatedDish");
            dishGO.AddComponent<SpriteRenderer>();
            var dish = dishGO.AddComponent<Ingredient>();
            dish.Initialize(IngredientType.PlatedDish, IngredientState.Plated);
            dish.SetPlatedIngredients(_platedIngredients);
            return dish;
        }

        /// <summary>
        /// Returns a copy of the list of ingredients currently on the plate.
        /// </summary>
        /// <returns>A new list containing the plated ingredients.</returns>
        public System.Collections.Generic.List<Ingredient> GetPlatedIngredients()
        {
            return new System.Collections.Generic.List<Ingredient>(_platedIngredients);
        }

        /// <summary>
        /// Determines whether this plating station can accept the given ingredient.
        /// </summary>
        /// <param name="item">The ingredient to check.</param>
        /// <returns>True if the plate is not full and the ingredient is cooked or chopped.</returns>
        public override bool CanAcceptItem(Ingredient item)
        {
            return _platedIngredients.Count < maxIngredients &&
                   item != null &&
                   (item.CurrentState == IngredientState.Cooked || item.CurrentState == IngredientState.Chopped);
        }
    }
}
