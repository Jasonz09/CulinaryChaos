namespace IOChef.Gameplay
{
    /// <summary>
    /// A single cell in the kitchen grid.
    /// </summary>
    [System.Serializable]
    public class GridCell
    {
        /// <summary>
        /// Grid X coordinate.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Grid Y coordinate.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Whether the player can walk through this cell.
        /// </summary>
        public bool IsWalkable { get; set; } = true;

        /// <summary>
        /// Interactive object occupying this cell, if any.
        /// </summary>
        public InteractiveObject Occupant { get; set; }

        /// <summary>
        /// Create a new grid cell at the given coordinates.
        /// </summary>
        /// <param name="x">Grid X coordinate.</param>
        /// <param name="y">Grid Y coordinate.</param>
        public GridCell(int x, int y)
        {
            X = x;
            Y = y;
            IsWalkable = true;
        }
    }
}
