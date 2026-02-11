using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// A single preparation step in a recipe.
    /// </summary>
    [System.Serializable]
    public class RecipeStep
    {
        /// <summary>
        /// Action to perform (Chop, Cook, Mix, etc.).
        /// </summary>
        public string actionType;

        /// <summary>
        /// Ingredients needed for this step.
        /// </summary>
        public List<RecipeIngredient> requiredIngredients;

        /// <summary>
        /// Time in seconds to complete this step.
        /// </summary>
        public float prepTimeSeconds = 2f;
    }
}
