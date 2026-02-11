using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Grid-based spatial system for the kitchen layout.
    /// </summary>
    public class KitchenGrid : MonoBehaviour
    {
        /// <summary>
        /// Width of the kitchen grid in cells.
        /// </summary>
        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 8;

        /// <summary>
        /// Height of the kitchen grid in cells.
        /// </summary>
        [SerializeField] private int gridHeight = 6;

        /// <summary>
        /// World-space size of each grid cell.
        /// </summary>
        [SerializeField] private float cellSize = 1f;

        /// <summary>
        /// World-space origin position of the grid.
        /// </summary>
        [SerializeField] private Vector2 gridOrigin = Vector2.zero;

        /// <summary>
        /// Two-dimensional array storing all grid cells.
        /// </summary>
        private GridCell[,] _cells;

        /// <summary>
        /// Grid width in cells.
        /// </summary>
        public int Width => gridWidth;

        /// <summary>
        /// Grid height in cells.
        /// </summary>
        public int Height => gridHeight;

        /// <summary>
        /// World-space size of each cell.
        /// </summary>
        public float CellSize => cellSize;

        /// <summary>
        /// Initializes the grid on startup.
        /// </summary>
        private void Awake()
        {
            InitializeGrid();
        }

        /// <summary>
        /// Creates the grid cell array based on current dimensions.
        /// </summary>
        private void InitializeGrid()
        {
            _cells = new GridCell[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    _cells[x, y] = new GridCell(x, y);
                }
            }
        }

        /// <summary>
        /// Resize and reinitialize the grid.
        /// </summary>
        /// <param name="width">New grid width in cells.</param>
        /// <param name="height">New grid height in cells.</param>
        public void Reinitialize(int width, int height)
        {
            gridWidth = width;
            gridHeight = height;
            InitializeGrid();
        }

        /// <summary>
        /// Convert grid coordinates to world position.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>World-space center of the cell.</returns>
        public Vector2 GridToWorld(int x, int y)
        {
            return gridOrigin + new Vector2(x * cellSize + cellSize * 0.5f,
                                            y * cellSize + cellSize * 0.5f);
        }

        /// <summary>
        /// Convert world position to grid coordinates.
        /// </summary>
        /// <param name="worldPos">World-space position.</param>
        /// <returns>Clamped grid coordinates.</returns>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            Vector2 local = worldPos - gridOrigin;
            int x = Mathf.FloorToInt(local.x / cellSize);
            int y = Mathf.FloorToInt(local.y / cellSize);
            return new Vector2Int(
                Mathf.Clamp(x, 0, gridWidth - 1),
                Mathf.Clamp(y, 0, gridHeight - 1)
            );
        }

        /// <summary>
        /// Check if coordinates are within grid bounds.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>True if in bounds.</returns>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
        }

        /// <summary>
        /// Check if a cell is in bounds and walkable.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>True if movement is possible.</returns>
        public bool CanMoveTo(int x, int y)
        {
            return IsInBounds(x, y) && _cells[x, y].IsWalkable;
        }

        /// <summary>
        /// Get the cell at given coordinates.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>The GridCell, or null if out of bounds.</returns>
        public GridCell GetCell(int x, int y)
        {
            if (!IsInBounds(x, y)) return null;
            return _cells[x, y];
        }

        /// <summary>
        /// Set the walkability of a cell.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <param name="walkable">Whether the cell is walkable.</param>
        public void SetWalkable(int x, int y, bool walkable)
        {
            if (IsInBounds(x, y))
                _cells[x, y].IsWalkable = walkable;
        }

        /// <summary>
        /// Set or clear the occupant of a cell.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <param name="occupant">The interactive object to place, or null to clear.</param>
        public void SetOccupant(int x, int y, InteractiveObject occupant)
        {
            if (IsInBounds(x, y))
            {
                _cells[x, y].Occupant = occupant;
                if (occupant != null)
                    _cells[x, y].IsWalkable = false;
            }
        }

        /// <summary>
        /// Get orthogonal neighbor cells.
        /// </summary>
        /// <param name="x">Grid x coordinate.</param>
        /// <param name="y">Grid y coordinate.</param>
        /// <returns>List of adjacent GridCells.</returns>
        public List<GridCell> GetNeighbors(int x, int y)
        {
            var neighbors = new List<GridCell>();
            if (IsInBounds(x - 1, y)) neighbors.Add(_cells[x - 1, y]);
            if (IsInBounds(x + 1, y)) neighbors.Add(_cells[x + 1, y]);
            if (IsInBounds(x, y - 1)) neighbors.Add(_cells[x, y - 1]);
            if (IsInBounds(x, y + 1)) neighbors.Add(_cells[x, y + 1]);
            return neighbors;
        }

        /// <summary>
        /// Find the nearest InteractiveObject of a given type from grid position.
        /// </summary>
        /// <param name="fromX">Starting grid x coordinate.</param>
        /// <param name="fromY">Starting grid y coordinate.</param>
        /// <returns>The nearest matching object, or null if none found.</returns>
        public InteractiveObject FindNearestObject<T>(int fromX, int fromY) where T : InteractiveObject
        {
            InteractiveObject nearest = null;
            float nearestDist = float.MaxValue;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (_cells[x, y].Occupant is T)
                    {
                        float dist = Mathf.Abs(x - fromX) + Mathf.Abs(y - fromY);
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            nearest = _cells[x, y].Occupant;
                        }
                    }
                }
            }
            return nearest;
        }

        /// <summary>
        /// Draws grid cell outlines in the Unity editor for debugging.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_cells == null)
            {
                // Draw preview grid in editor
                Gizmos.color = Color.gray;
                for (int x = 0; x <= gridWidth; x++)
                    Gizmos.DrawLine(
                        (Vector3)(gridOrigin + new Vector2(x * cellSize, 0)),
                        (Vector3)(gridOrigin + new Vector2(x * cellSize, gridHeight * cellSize)));

                for (int y = 0; y <= gridHeight; y++)
                    Gizmos.DrawLine(
                        (Vector3)(gridOrigin + new Vector2(0, y * cellSize)),
                        (Vector3)(gridOrigin + new Vector2(gridWidth * cellSize, y * cellSize)));
                return;
            }

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector3 center = (Vector3)GridToWorld(x, y);
                    Gizmos.color = _cells[x, y].IsWalkable ? new Color(0, 1, 0, 0.2f)
                                                           : new Color(1, 0, 0, 0.2f);
                    Gizmos.DrawCube(center, Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
