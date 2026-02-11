using UnityEngine;
using System.Collections.Generic;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Creates runtime LevelDataSO instances for levels 1-10 (World 1).
    /// Used as a fallback when no ScriptableObject assets exist yet.
    /// Once real .asset files are created in the Unity editor, this class can be removed.
    /// </summary>
    public static class DefaultLevelFactory
    {
        /// <summary>
        /// The number of worlds defined by the factory.
        /// </summary>
        public static int WorldCount => 1;

        /// <summary>
        /// Creates a single LevelDataSO for the given level identifier.
        /// </summary>
        /// <param name="levelId">The level identifier (1-10).</param>
        /// <returns>A populated LevelDataSO instance, or null for unknown identifiers.</returns>
        public static LevelDataSO Create(int levelId)
        {
            return levelId switch
            {
                1  => Level1(),
                2  => Level2(),
                3  => Level3(),
                4  => Level4(),
                5  => Level5(),
                6  => Level6(),
                7  => Level7(),
                8  => Level8(),
                9  => Level9(),
                10 => Level10(),
                _  => null,
            };
        }

        /// <summary>
        /// Creates all levels for the specified world.
        /// </summary>
        /// <param name="worldId">The world identifier.</param>
        /// <returns>An array of LevelDataSO instances, or an empty array for unknown worlds.</returns>
        public static LevelDataSO[] CreateWorld(int worldId)
        {
            return worldId switch
            {
                1 => new[] { Level1(), Level2(), Level3(), Level4(), Level5(),
                             Level6(), Level7(), Level8(), Level9(), Level10() },
                _ => System.Array.Empty<LevelDataSO>(),
            };
        }

        /// <summary>
        /// Returns the display name for the specified world.
        /// </summary>
        /// <param name="worldId">The world identifier.</param>
        /// <returns>The human-readable world name.</returns>
        public static string GetWorldName(int worldId)
        {
            return worldId switch
            {
                1 => "THE KITCHEN",
                _ => $"WORLD {worldId}",
            };
        }

        // ── Level 1: Tutorial — just chopped lettuce salad ──
        /// <summary>
        /// Creates level 1 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 1.</returns>
        private static LevelDataSO Level1()
        {
            return Build(1, 1, 1, "Lettuce Salad", 120, 50, 2, 1, 400, 700, 1100,
                true, true, false, 0,
                R("Lettuce Salad", 60, 120, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)));
        }

        // ── Level 2: Add tomato salad ──
        /// <summary>
        /// Creates level 2 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 2.</returns>
        private static LevelDataSO Level2()
        {
            return Build(2, 1, 2, "Two Salads", 150, 45, 2, 2, 500, 900, 1400,
                true, true, false, 0,
                R("Lettuce Salad", 60, 90, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Tomato Salad", 60, 90, 1,
                    I(IngredientType.Tomato, IngredientState.Chopped)));
        }

        // ── Level 3: Introduce cooking (tomato soup) ──
        /// <summary>
        /// Creates level 3 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 3.</returns>
        private static LevelDataSO Level3()
        {
            return Build(3, 1, 3, "First Cooking", 150, 40, 3, 2, 600, 1100, 1700,
                true, true, false, 0,
                R("Lettuce Salad", 60, 80, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Tomato Soup", 100, 80, 1,
                    I(IngredientType.Tomato, IngredientState.Cooked)));
        }

        // ── Level 4: Add meat cooking ──
        /// <summary>
        /// Creates level 4 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 4.</returns>
        private static LevelDataSO Level4()
        {
            return Build(4, 1, 4, "Meat Kitchen", 180, 40, 3, 2, 700, 1300, 2000,
                true, true, false, 0,
                R("Lettuce Salad", 60, 70, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Tomato Soup", 100, 70, 1,
                    I(IngredientType.Tomato, IngredientState.Cooked)),
                R("Cooked Meat", 120, 75, 1,
                    I(IngredientType.Meat, IngredientState.Cooked)));
        }

        // ── Level 5: First 2-ingredient recipe ──
        /// <summary>
        /// Creates level 5 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 5.</returns>
        private static LevelDataSO Level5()
        {
            return Build(5, 1, 5, "Combo Plates", 180, 35, 3, 3, 800, 1500, 2400,
                true, true, false, 0,
                R("Chopped Salad", 150, 80, 2,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Cooked Meat", 120, 70, 1,
                    I(IngredientType.Meat, IngredientState.Cooked)),
                R("Tomato Soup", 100, 65, 1,
                    I(IngredientType.Tomato, IngredientState.Cooked)));
        }

        // ── Level 6: Dirty dishes introduced — sink required, limited plates ──
        /// <summary>
        /// Creates level 6 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 6.</returns>
        private static LevelDataSO Level6()
        {
            return Build(6, 1, 6, "Dirty Dishes", 180, 35, 3, 3, 900, 1700, 2700,
                false, false, true, 8,
                R("Chopped Salad", 150, 75, 2,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Steak Plate", 180, 80, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Tomato Soup", 100, 60, 1,
                    I(IngredientType.Tomato, IngredientState.Cooked)));
        }

        // ── Level 7: More combos, tighter timing ──
        /// <summary>
        /// Creates level 7 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 7.</returns>
        private static LevelDataSO Level7()
        {
            return Build(7, 1, 7, "Rush Hour", 180, 30, 4, 3, 1000, 1900, 3000,
                false, false, true, 8,
                R("Steak Plate", 180, 70, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Steak & Tomato", 180, 70, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Chopped Salad", 150, 65, 2,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Lettuce Salad", 60, 50, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)));
        }

        // ── Level 8: All recipes, fewer plates ──
        /// <summary>
        /// Creates level 8 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 8.</returns>
        private static LevelDataSO Level8()
        {
            return Build(8, 1, 8, "Plate Crunch", 200, 30, 4, 3, 1100, 2100, 3400,
                false, false, true, 6,
                R("Chopped Salad", 150, 55, 2,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Steak Plate", 200, 60, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Steak & Tomato", 200, 60, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Cooked Meat", 120, 50, 1,
                    I(IngredientType.Meat, IngredientState.Cooked)));
        }

        // ── Level 9: 3-ingredient recipe introduced ──
        /// <summary>
        /// Creates level 9 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 9.</returns>
        private static LevelDataSO Level9()
        {
            return Build(9, 1, 9, "Deluxe Kitchen", 200, 25, 4, 3, 1200, 2400, 3800,
                false, false, true, 5,
                R("Deluxe Salad", 250, 75, 3,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped),
                    I(IngredientType.Meat, IngredientState.Cooked)),
                R("Steak Plate", 200, 55, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Chopped Salad", 150, 50, 2,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped)));
        }

        // ── Level 10: All recipes, tight timers, max chaos ──
        /// <summary>
        /// Creates level 10 configuration.
        /// </summary>
        /// <returns>A LevelDataSO for level 10.</returns>
        private static LevelDataSO Level10()
        {
            return Build(10, 1, 10, "Grand Kitchen", 210, 22, 4, 4, 1400, 2700, 4200,
                false, false, true, 5,
                R("Deluxe Salad", 280, 65, 3,
                    I(IngredientType.Lettuce, IngredientState.Chopped),
                    I(IngredientType.Tomato, IngredientState.Chopped),
                    I(IngredientType.Meat, IngredientState.Cooked)),
                R("Steak & Tomato", 200, 50, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Tomato, IngredientState.Chopped)),
                R("Steak Plate", 200, 50, 2,
                    I(IngredientType.Meat, IngredientState.Cooked),
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Lettuce Salad", 60, 40, 1,
                    I(IngredientType.Lettuce, IngredientState.Chopped)),
                R("Tomato Soup", 100, 40, 1,
                    I(IngredientType.Tomato, IngredientState.Cooked)));
        }

        // ═══════════════════════════════════════════════════════
        //  Helpers
        // ═══════════════════════════════════════════════════════

        /// <summary>
        /// Returns the coin entry cost for the given level.
        /// Level 1 is free, then scales by 25 per level.
        /// </summary>
        /// <param name="levelId">The level identifier.</param>
        /// <returns>Entry cost in coins.</returns>
        public static int GetEntryCost(int levelId)
        {
            if (levelId <= 1) return 0;
            return (levelId - 1) * 25;
        }

        /// <summary>
        /// Constructs a LevelDataSO with the given parameters.
        /// </summary>
        /// <param name="id">Unique level identifier.</param>
        /// <param name="worldId">World identifier this level belongs to.</param>
        /// <param name="levelNumber">Display number within the world.</param>
        /// <param name="name">Display name of the level.</param>
        /// <param name="time">Time limit in seconds.</param>
        /// <param name="interval">Order spawn interval in seconds.</param>
        /// <param name="maxOrders">Maximum simultaneous active orders.</param>
        /// <param name="initialOrders">Number of orders at level start.</param>
        /// <param name="s1">Score threshold for 1 star.</param>
        /// <param name="s2">Score threshold for 2 stars.</param>
        /// <param name="s3">Score threshold for 3 stars.</param>
        /// <param name="unlimitedPlates">Whether plates are unlimited.</param>
        /// <param name="autoRemove">Whether served plates are auto-removed.</param>
        /// <param name="requiresSink">Whether the level requires a sink station.</param>
        /// <param name="plateCount">Number of plates if not unlimited.</param>
        /// <param name="recipes">Available recipes for this level.</param>
        /// <returns>A fully configured LevelDataSO instance.</returns>
        private static LevelDataSO Build(int id, int worldId, int levelNumber, string name,
            float time, float interval, int maxOrders, int initialOrders,
            int s1, int s2, int s3,
            bool unlimitedPlates, bool autoRemove, bool requiresSink, int plateCount,
            params RecipeSO[] recipes)
        {
            var level = ScriptableObject.CreateInstance<LevelDataSO>();
            level.levelId = id;
            level.worldId = worldId;
            level.levelNumber = levelNumber;
            level.levelName = name;
            level.timeLimitSeconds = time;
            level.orderSpawnInterval = interval;
            level.maxActiveOrders = maxOrders;
            level.initialOrders = initialOrders;
            level.threshold1Star = s1;
            level.threshold2Star = s2;
            level.threshold3Star = s3;
            level.unlimitedPlates = unlimitedPlates;
            level.autoRemovePlates = autoRemove;
            level.requiresSink = requiresSink;
            level.plateCount = plateCount;
            level.entryCost = GetEntryCost(id);
            level.availableRecipes = new List<RecipeSO>(recipes);

            // Generate equipment layout from recipe requirements
            KitchenLayoutGenerator.Generate(level);

            return level;
        }

        /// <summary>
        /// Creates a recipe ScriptableObject with the given parameters.
        /// </summary>
        /// <param name="name">Display name of the recipe.</param>
        /// <param name="pts">Points awarded for completing the recipe.</param>
        /// <param name="time">Time limit for the recipe order in seconds.</param>
        /// <param name="diff">Difficulty tier of the recipe.</param>
        /// <param name="ings">Required ingredients for the recipe.</param>
        /// <returns>A configured RecipeSO instance.</returns>
        private static RecipeSO R(string name, int pts, float time, int diff, params RecipeIngredient[] ings)
        {
            var r = ScriptableObject.CreateInstance<RecipeSO>();
            r.recipeName = name;
            r.pointsForCompletion = pts;
            r.timeLimitSeconds = time;
            r.difficultyTier = diff;
            r.finalIngredients = new List<RecipeIngredient>(ings);
            return r;
        }

        /// <summary>
        /// Creates a recipe ingredient requirement.
        /// </summary>
        /// <param name="t">The ingredient type.</param>
        /// <param name="s">The required ingredient state.</param>
        /// <returns>A configured RecipeIngredient instance.</returns>
        private static RecipeIngredient I(IngredientType t, IngredientState s)
        {
            return new RecipeIngredient { ingredientType = t, requiredState = s };
        }
    }
}
