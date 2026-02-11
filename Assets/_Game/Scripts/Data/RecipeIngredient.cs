namespace IOChef.Gameplay
{
    /// <summary>
    /// An ingredient requirement within a recipe.
    /// </summary>
    [System.Serializable]
    public class RecipeIngredient
    {
        /// <summary>
        /// Type of ingredient required.
        /// </summary>
        public IngredientType ingredientType;

        /// <summary>
        /// State the ingredient must be in.
        /// </summary>
        public IngredientState requiredState;
    }
}
