using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// ScriptableObject defining a recipe's ingredients, steps, and scoring.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "IOChef/Recipe")]
    public class RecipeSO : ScriptableObject
    {
        [Header("Recipe Info")]
        /// <summary>
        /// Display name of the recipe.
        /// </summary>
        public string recipeName;

        /// <summary>
        /// Icon sprite for the recipe.
        /// </summary>
        public Sprite recipeIcon;

        /// <summary>
        /// Recipe description text.
        /// </summary>
        [TextArea] public string description;

        [Header("Requirements")]
        /// <summary>
        /// Ingredients needed in the final dish.
        /// </summary>
        public List<RecipeIngredient> finalIngredients;

        /// <summary>
        /// Ordered preparation steps.
        /// </summary>
        public List<RecipeStep> steps;

        [Header("Scoring")]
        /// <summary>
        /// Base points awarded on completion.
        /// </summary>
        public int pointsForCompletion = 100;

        /// <summary>
        /// Bonus points for fast completion.
        /// </summary>
        public int bonusForSpeed = 50;

        /// <summary>
        /// Time limit for this recipe's order.
        /// </summary>
        public float timeLimitSeconds = 60f;

        [Header("Difficulty")]
        /// <summary>
        /// Difficulty tier (1 = Easy, 2 = Medium, 3 = Hard).
        /// </summary>
        public int difficultyTier = 1;

        /// <summary>
        /// Check if a delivered dish matches this recipe.
        /// </summary>
        /// <param name="dish">The plated dish to validate.</param>
        /// <returns>True if the dish satisfies all recipe requirements.</returns>
        public bool MatchesDish(Ingredient dish)
        {
            return dish.MatchesRecipeRequirements(finalIngredients);
        }
    }
}
