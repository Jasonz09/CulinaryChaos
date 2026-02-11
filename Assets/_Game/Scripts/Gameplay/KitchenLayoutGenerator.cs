using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Analyzes a level's recipes to determine which kitchen equipment is needed,
    /// then generates an adaptive equipment layout based on complexity tier.
    /// </summary>
    public static class KitchenLayoutGenerator
    {
        /// <summary>
        /// Populates level.equipment, level.gridWidth, and level.gridHeight
        /// based on the level's available recipes and dish management settings.
        /// </summary>
        /// <param name="level">The level data to populate with layout information.</param>
        public static void Generate(LevelDataSO level)
        {
            // Analyze recipes to determine needed equipment
            var neededIngredients = new HashSet<IngredientType>();
            bool needsCuttingBoard = false;
            bool needsCooktop = false;
            bool needsSink = level.requiresSink;

            foreach (var recipe in level.availableRecipes)
            {
                if (recipe.finalIngredients == null) continue;
                foreach (var ing in recipe.finalIngredients)
                {
                    neededIngredients.Add(ing.ingredientType);
                    if (ing.requiredState == IngredientState.Chopped)
                        needsCuttingBoard = true;
                    if (ing.requiredState == IngredientState.Cooked)
                        needsCooktop = true;
                }
            }

            // Count total stations to determine tier
            int totalStations = neededIngredients.Count
                + (needsCuttingBoard ? 1 : 0)
                + (needsCooktop ? 1 : 0)
                + 3 // PlatingStation + ServePoint + TrashBin (always)
                + (needsSink ? 1 : 0);

            LayoutTier tier;
            if (totalStations <= 6)
                tier = LayoutTier.Small;
            else if (totalStations <= 8)
                tier = LayoutTier.Medium;
            else
                tier = LayoutTier.Full;

            // Set grid dimensions
            var (gw, gh) = GetGridSize(tier);
            level.gridWidth = gw;
            level.gridHeight = gh;

            // Build equipment list
            level.equipment = BuildLayout(tier, gw, gh, neededIngredients,
                needsCuttingBoard, needsCooktop, needsSink);

            Debug.Log($"[KitchenLayoutGenerator] Level '{level.levelName}': " +
                      $"tier={tier}, grid={gw}x{gh}, stations={totalStations}, " +
                      $"equipment={level.equipment.Count} spawns");
        }

        /// <summary>
        /// Returns grid dimensions for the given layout tier.
        /// </summary>
        /// <param name="tier">The layout tier determining kitchen size.</param>
        /// <returns>A tuple containing the grid width and height.</returns>
        private static (int width, int height) GetGridSize(LayoutTier tier) => tier switch
        {
            LayoutTier.Small  => (6, 5),
            LayoutTier.Medium => (8, 6),
            LayoutTier.Full   => (10, 7),
            _ => (10, 7)
        };

        /// <summary>
        /// Builds the full equipment spawn list for the given configuration.
        /// </summary>
        /// <param name="tier">The layout tier determining kitchen complexity.</param>
        /// <param name="gw">The grid width.</param>
        /// <param name="gh">The grid height.</param>
        /// <param name="sources">The set of ingredient types that need source stations.</param>
        /// <param name="needsCutting">Whether a cutting board is required.</param>
        /// <param name="needsCooking">Whether a cooktop is required.</param>
        /// <param name="needsSink">Whether a sink is required.</param>
        /// <returns>A list of equipment spawns defining the kitchen layout.</returns>
        private static List<EquipmentSpawn> BuildLayout(LayoutTier tier, int gw, int gh,
            HashSet<IngredientType> sources,
            bool needsCutting, bool needsCooking, bool needsSink)
        {
            var spawns = new List<EquipmentSpawn>();

            int topY = gh - 1;
            int botY = tier == LayoutTier.Full ? 1 : 0;

            // === TOP ROW: sources -> processing -> plating -> counters ===
            int slot = 0;

            // Ingredient sources (sorted for deterministic order)
            var sortedSources = new List<IngredientType>(sources);
            sortedSources.Sort();
            foreach (var src in sortedSources)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = slot, gridY = topY,
                    equipmentType = EquipmentType.IngredientSource,
                    sourceIngredient = src
                });
                slot++;
            }

            // CuttingBoard
            if (needsCutting)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = slot, gridY = topY,
                    equipmentType = EquipmentType.CuttingBoard
                });
                slot++;
            }

            // Cooktop
            if (needsCooking)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = slot, gridY = topY,
                    equipmentType = EquipmentType.Cooktop
                });
                slot++;
            }

            // PlatingStation
            spawns.Add(new EquipmentSpawn
            {
                gridX = slot, gridY = topY,
                equipmentType = EquipmentType.PlatingStation
            });
            slot++;

            // Pad remaining top row with Counters
            for (int x = slot; x < gw; x++)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = x, gridY = topY,
                    equipmentType = EquipmentType.Counter
                });
            }

            // === BOTTOM ROW: trash -> sink? -> counters -> serve ===

            // TrashBin always leftmost
            spawns.Add(new EquipmentSpawn
            {
                gridX = 0, gridY = botY,
                equipmentType = EquipmentType.TrashBin
            });

            // Sink second from left (if needed)
            int botSlotStart = 1;
            if (needsSink)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = 1, gridY = botY,
                    equipmentType = EquipmentType.Sink
                });
                botSlotStart = 2;
            }

            // ServePoint always rightmost
            spawns.Add(new EquipmentSpawn
            {
                gridX = gw - 1, gridY = botY,
                equipmentType = EquipmentType.ServePoint
            });

            // Pad middle of bottom row with Counters
            for (int x = botSlotStart; x < gw - 1; x++)
            {
                spawns.Add(new EquipmentSpawn
                {
                    gridX = x, gridY = botY,
                    equipmentType = EquipmentType.Counter
                });
            }

            // === CENTER ISLANDS ===
            AddIslands(spawns, tier, gw, gh);

            return spawns;
        }

        /// <summary>
        /// Adds center counter islands based on layout tier.
        /// </summary>
        /// <param name="spawns">The list of equipment spawns to append to.</param>
        /// <param name="tier">The layout tier determining island configuration.</param>
        /// <param name="gw">The grid width.</param>
        /// <param name="gh">The grid height.</param>
        private static void AddIslands(List<EquipmentSpawn> spawns, LayoutTier tier, int gw, int gh)
        {
            switch (tier)
            {
                case LayoutTier.Small:
                    // Single row of 4 counters at y=2
                    for (int x = 1; x <= 4; x++)
                        spawns.Add(new EquipmentSpawn
                        {
                            gridX = x, gridY = 2,
                            equipmentType = EquipmentType.Counter
                        });
                    break;

                case LayoutTier.Medium:
                    // Two 2x2 islands
                    Add2x2Island(spawns, 1, 2);
                    Add2x2Island(spawns, 5, 2);
                    break;

                case LayoutTier.Full:
                    // Three 2x2 islands
                    Add2x2Island(spawns, 1, 3);
                    Add2x2Island(spawns, 5, 3);
                    Add2x2Island(spawns, 8, 3);
                    break;
            }
        }

        /// <summary>
        /// Adds a 2x2 counter island at the specified grid position.
        /// </summary>
        /// <param name="spawns">The list of equipment spawns to append to.</param>
        /// <param name="startX">The starting X grid coordinate of the island.</param>
        /// <param name="startY">The starting Y grid coordinate of the island.</param>
        private static void Add2x2Island(List<EquipmentSpawn> spawns, int startX, int startY)
        {
            for (int dx = 0; dx < 2; dx++)
                for (int dy = 0; dy < 2; dy++)
                    spawns.Add(new EquipmentSpawn
                    {
                        gridX = startX + dx,
                        gridY = startY + dy,
                        equipmentType = EquipmentType.Counter
                    });
        }
    }
}
