using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Represents an ingredient or plated dish in the kitchen.
    /// </summary>
    public class Ingredient : MonoBehaviour
    {
        /// <summary>
        /// The type of ingredient this object represents.
        /// </summary>
        [Header("Ingredient Data")]
        [SerializeField] private IngredientType ingredientType;

        /// <summary>
        /// Current processing state of the ingredient.
        /// </summary>
        [SerializeField] private IngredientState currentState = IngredientState.Raw;

        /// <summary>
        /// Sprite renderer used to display the ingredient.
        /// </summary>
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        /// <summary>
        /// Sprite shown when the ingredient is in raw state.
        /// </summary>
        [SerializeField] private Sprite rawSprite;

        /// <summary>
        /// Sprite shown when the ingredient is chopped.
        /// </summary>
        [SerializeField] private Sprite choppedSprite;

        /// <summary>
        /// Sprite shown when the ingredient is cooked.
        /// </summary>
        [SerializeField] private Sprite cookedSprite;

        /// <summary>
        /// Sprite shown when the ingredient is burned.
        /// </summary>
        [SerializeField] private Sprite burnedSprite;

        /// <summary>
        /// Sprite shown when the ingredient is plated.
        /// </summary>
        [SerializeField] private Sprite platedSprite;

        /// <summary>
        /// The type of this ingredient.
        /// </summary>
        public IngredientType Type => ingredientType;

        /// <summary>
        /// Current processing state.
        /// </summary>
        public IngredientState CurrentState => currentState;

        /// <summary>
        /// List of ingredients contained in this plated dish.
        /// </summary>
        private List<Ingredient> _platedContents;

        /// <summary>
        /// Contents of this plated dish (null if not plated).
        /// </summary>
        public IReadOnlyList<Ingredient> PlatedContents => _platedContents;

        /// <summary>
        /// Set up ingredient type and initial state.
        /// </summary>
        /// <param name="type">The ingredient type.</param>
        /// <param name="state">The initial processing state.</param>
        public void Initialize(IngredientType type, IngredientState state = IngredientState.Raw)
        {
            ingredientType = type;
            currentState = state;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            UpdateVisual();
        }

        /// <summary>
        /// Change the processing state.
        /// </summary>
        /// <param name="newState">The new processing state.</param>
        public void SetState(IngredientState newState)
        {
            currentState = newState;
            UpdateVisual();
        }

        /// <summary>
        /// Set the contents of a plated dish.
        /// </summary>
        /// <param name="ingredients">List of ingredients to plate.</param>
        public void SetPlatedIngredients(List<Ingredient> ingredients)
        {
            _platedContents = new List<Ingredient>(ingredients);
            UpdateDishLabel();
        }

        /// <summary>
        /// Add an ingredient to an already-plated dish.
        /// </summary>
        /// <param name="ingredient">The ingredient to add.</param>
        public void AddToPlate(Ingredient ingredient)
        {
            if (_platedContents == null) _platedContents = new List<Ingredient>();
            _platedContents.Add(ingredient);
            UpdateDishLabel();
        }

        /// <summary>
        /// Updates the text label showing plated dish contents.
        /// </summary>
        private void UpdateDishLabel()
        {
            if (_platedContents == null || _platedContents.Count == 0) return;
            // Build a readable contents string
            string contents = "";
            foreach (var c in _platedContents)
            {
                if (contents.Length > 0) contents += "+";
                string st = c.CurrentState switch
                {
                    IngredientState.Chopped => "Cut",
                    IngredientState.Cooked => "Ckd",
                    _ => ""
                };
                contents += $"{st}{c.Type.ToString().Substring(0, System.Math.Min(3, c.Type.ToString().Length))}";
            }

            var existing = transform.Find("IngLabel");
            if (existing != null)
            {
                var tm = existing.GetComponent<TextMesh>();
                tm.text = $"DISH({_platedContents.Count})\n{contents}";
                tm.color = new Color(1f, 1f, 0.7f);
            }
        }

        /// <summary>
        /// Cached runtime-generated circle sprite shared across all instances.
        /// </summary>
        private static Sprite _rtCircle;

        /// <summary>
        /// Updates the sprite and color based on current type and state.
        /// </summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;

            if (rawSprite != null)
            {
                spriteRenderer.sprite = currentState switch
                {
                    IngredientState.Raw => rawSprite,
                    IngredientState.Chopped => choppedSprite,
                    IngredientState.Cooked => cookedSprite,
                    IngredientState.Burned => burnedSprite,
                    IngredientState.Plated => platedSprite,
                    _ => rawSprite
                };
                return;
            }

            // Runtime visual: colored circle
            if (_rtCircle == null)
            {
                int sz = 24;
                var t = new Texture2D(sz, sz);
                float r = sz / 2f - 1, cx = sz / 2f, cy = sz / 2f;
                for (int y = 0; y < sz; y++)
                    for (int x = 0; x < sz; x++)
                    {
                        float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d + 0.5f)));
                    }
                t.filterMode = FilterMode.Bilinear;
                t.Apply();
                _rtCircle = Sprite.Create(t, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), (float)sz);
            }

            spriteRenderer.sprite = _rtCircle;
            spriteRenderer.sortingOrder = 20;
            spriteRenderer.color = GetIngredientColor();

            // Add a text label so player knows what ingredient this is
            EnsureLabel();
        }

        /// <summary>
        /// Creates or updates the floating text label for this ingredient.
        /// </summary>
        private void EnsureLabel()
        {
            var existing = transform.Find("IngLabel");
            if (existing != null)
            {
                existing.GetComponent<TextMesh>().text = GetShortName();
                existing.GetComponent<TextMesh>().color = GetLabelColor();
                return;
            }
            var lgo = new GameObject("IngLabel");
            lgo.transform.SetParent(transform);
            lgo.transform.localPosition = new Vector3(0, -0.6f, 0);
            lgo.transform.localScale = Vector3.one;
            var tm = lgo.AddComponent<TextMesh>();
            tm.text = GetShortName();
            tm.fontSize = 36;
            tm.characterSize = 0.08f;
            tm.anchor = TextAnchor.UpperCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = GetLabelColor();
            lgo.GetComponent<MeshRenderer>().sortingOrder = 21;
        }

        /// <summary>
        /// Returns the label color for the current processing state.
        /// </summary>
        /// <returns>The color to use for the ingredient label.</returns>
        private Color GetLabelColor()
        {
            return currentState switch
            {
                IngredientState.Raw => new Color(1, 1, 1, 0.9f),
                IngredientState.Chopped => new Color(0.8f, 1f, 0.8f),
                IngredientState.Cooked => new Color(1f, 0.9f, 0.6f),
                IngredientState.Burned => new Color(1f, 0.3f, 0.2f),
                IngredientState.Plated => new Color(1f, 1f, 0.7f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Returns a short display name for the ingredient and its state.
        /// </summary>
        /// <returns>A short string representing the ingredient name and state.</returns>
        private string GetShortName()
        {
            if (currentState == IngredientState.Plated)
            {
                int count = _platedContents != null ? _platedContents.Count : 0;
                return $"DISH ({count})";
            }
            string state = currentState switch
            {
                IngredientState.Raw => "",
                IngredientState.Chopped => "[CUT] ",
                IngredientState.Cooked => "[DONE] ",
                IngredientState.Burned => "[BURN] ",
                _ => ""
            };
            string name = ingredientType switch
            {
                IngredientType.Lettuce => "Lettuce",
                IngredientType.Tomato => "Tomato",
                IngredientType.Meat => "Meat",
                IngredientType.Fish => "Fish",
                IngredientType.Rice => "Rice",
                _ => ingredientType.ToString()
            };
            return state + name;
        }

        /// <summary>
        /// Returns the visual color for the current ingredient type and state.
        /// </summary>
        /// <returns>The color to apply to the ingredient sprite.</returns>
        private Color GetIngredientColor()
        {
            Color baseC = ingredientType switch
            {
                IngredientType.Lettuce => new Color(0.30f, 0.82f, 0.22f),
                IngredientType.Tomato => new Color(0.92f, 0.22f, 0.15f),
                IngredientType.Meat => new Color(0.72f, 0.30f, 0.22f),
                IngredientType.Bun => new Color(0.85f, 0.70f, 0.35f),
                IngredientType.Cheese => new Color(1f, 0.85f, 0.20f),
                IngredientType.Fish => new Color(0.55f, 0.78f, 0.92f),
                IngredientType.Rice => new Color(0.95f, 0.95f, 0.90f),
                IngredientType.Dough => new Color(0.92f, 0.82f, 0.65f),
                _ => new Color(0.75f, 0.70f, 0.50f)
            };

            return currentState switch
            {
                IngredientState.Raw => baseC,
                IngredientState.Chopped => baseC * 0.85f + new Color(0.15f, 0.15f, 0.15f),
                IngredientState.Cooked => new Color(
                    Mathf.Lerp(baseC.r, 0.55f, 0.4f),
                    Mathf.Lerp(baseC.g, 0.35f, 0.5f),
                    Mathf.Lerp(baseC.b, 0.18f, 0.5f)),
                IngredientState.Burned => new Color(0.20f, 0.15f, 0.10f),
                IngredientState.Plated => Color.white,
                _ => baseC
            };
        }

        /// <summary>
        /// Check if this ingredient's contents match a recipe's requirements.
        /// </summary>
        /// <param name="requirements">The recipe ingredient requirements to match against.</param>
        /// <returns>True if all requirements are satisfied by the plated contents.</returns>
        public bool MatchesRecipeRequirements(List<RecipeIngredient> requirements)
        {
            if (_platedContents == null || _platedContents.Count != requirements.Count)
                return false;

            var remaining = new List<RecipeIngredient>(requirements);
            foreach (var content in _platedContents)
            {
                int idx = remaining.FindIndex(r =>
                    r.ingredientType == content.Type &&
                    r.requiredState == content.CurrentState);

                if (idx < 0) return false;
                remaining.RemoveAt(idx);
            }
            return remaining.Count == 0;
        }
    }
}
