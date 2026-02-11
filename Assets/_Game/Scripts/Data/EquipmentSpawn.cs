namespace IOChef.Gameplay
{
    /// <summary>
    /// Defines a piece of equipment placed on the kitchen grid.
    /// </summary>
    [System.Serializable]
    public class EquipmentSpawn
    {
        /// <summary>
        /// Grid X coordinate.
        /// </summary>
        public int gridX;

        /// <summary>
        /// Grid Y coordinate.
        /// </summary>
        public int gridY;

        /// <summary>
        /// Type of equipment to spawn.
        /// </summary>
        public EquipmentType equipmentType;

        /// <summary>
        /// Ingredient type when equipmentType is IngredientSource.
        /// </summary>
        public IngredientType sourceIngredient;
    }
}
