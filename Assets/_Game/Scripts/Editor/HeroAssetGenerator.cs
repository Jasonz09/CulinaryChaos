using UnityEditor;
using UnityEngine;
using IOChef.Heroes;

namespace IOChef.Editor
{
    public static class HeroAssetGenerator
    {
        [MenuItem("IOChef/Generate Hero Assets")]
        public static void GenerateHeroes()
        {
            string folder = "Assets/_Game/Data/Heroes";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                    AssetDatabase.CreateFolder("Assets/_Game", "Data");
                AssetDatabase.CreateFolder("Assets/_Game/Data", "Heroes");
            }

            CreateHero(folder, "hero_basil", "Chef Basil",
                "Your trusty starter chef. Reliable, dependable, and always ready to cook.",
                HeroRarity.Common, true, "Starter", SpecialAbilityType.None,
                "All-Rounder", "No special ability, but solid all-around stats.",
                1.00f, 0.95f, 1.00f, 1.08f, 1.00f, 1.05f,
                1.00f, 1.05f, 1.00f, 1.05f, 0f, 3f,
                0f, 0f);

            CreateHero(folder, "hero_pepper", "Pepper",
                "A fiery chef who knows the value of a good tip. Earns more points per order.",
                HeroRarity.Common, false, "Chest", SpecialAbilityType.ScoreBoost,
                "Big Tipper", "Bonus score on every completed order.",
                1.00f, 0.96f, 1.00f, 1.05f, 1.03f, 1.18f,
                1.00f, 1.04f, 1.00f, 1.04f, 0f, 2f,
                0.03f, 0.18f);

            CreateHero(folder, "hero_sizzle", "Sizzle",
                "A cool-headed chef who never lets the heat get to the food.",
                HeroRarity.Rare, false, "Chest", SpecialAbilityType.IgnoreBurn,
                "Burn Guard", "Food takes extra seconds before burning after cooking.",
                0.97f, 0.90f, 1.05f, 1.25f, 1.00f, 1.05f,
                1.00f, 1.04f, 1.00f, 1.04f, 0f, 2f,
                3f, 8f);

            CreateHero(folder, "hero_dash", "Dash",
                "A lightning-fast chef who always seems to find extra time.",
                HeroRarity.Rare, false, "Chest", SpecialAbilityType.DoubleTime,
                "Time Warp", "Extra bonus seconds added to the level timer.",
                0.98f, 0.93f, 1.00f, 1.06f, 1.00f, 1.04f,
                1.02f, 1.10f, 1.00f, 1.04f, 5f, 15f,
                5f, 15f);

            CreateHero(folder, "hero_luna", "Luna",
                "A graceful chef who never loses her rhythm, even when orders go wrong.",
                HeroRarity.Rare, false, "Chest", SpecialAbilityType.ComboKeep,
                "Combo Shield", "Failed orders don't break your combo streak.",
                0.98f, 0.93f, 1.00f, 1.06f, 1.02f, 1.10f,
                1.00f, 1.05f, 1.00f, 1.05f, 0f, 3f,
                1f, 3f);

            CreateHero(folder, "hero_ginger", "Ginger",
                "A resourceful chef who sometimes manages to cook without using any ingredients.",
                HeroRarity.Epic, false, "Chest", SpecialAbilityType.FreeIngredient,
                "Lucky Pantry", "Chance to pick up ingredients without using stock.",
                0.96f, 0.88f, 1.00f, 1.08f, 1.00f, 1.06f,
                1.00f, 1.05f, 1.02f, 1.08f, 0f, 3f,
                0.08f, 0.25f);

            CreateHero(folder, "hero_miso", "Miso",
                "A zen master chef who can wave away mistakes as if they never happened.",
                HeroRarity.Epic, false, "Chest", SpecialAbilityType.UndoOrder,
                "Order Zen", "Can dismiss expired orders without penalty.",
                0.97f, 0.90f, 1.02f, 1.12f, 1.01f, 1.08f,
                1.01f, 1.06f, 1.01f, 1.06f, 0f, 4f,
                1f, 3f);

            CreateHero(folder, "hero_noir", "Chef Noir",
                "A legendary chef who thrives under pressure. Extra chances to recover from mistakes.",
                HeroRarity.Legendary, false, "Chest", SpecialAbilityType.ExtraLives,
                "Iron Will", "Extra order failures allowed before affecting your rating.",
                0.96f, 0.88f, 1.00f, 1.10f, 1.02f, 1.12f,
                1.01f, 1.06f, 1.01f, 1.06f, 0f, 5f,
                1f, 3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[HeroAssetGenerator] 8 hero assets created in " + folder);
        }

        private static void CreateHero(string folder, string id, string name, string desc,
            HeroRarity rarity, bool isFree, string acquisition, SpecialAbilityType ability,
            string abilityName, string abilityDesc,
            float baseCook, float maxCook, float baseBurn, float maxBurn,
            float baseScore, float maxScore, float baseSpeed, float maxSpeed,
            float baseRadius, float maxRadius, float baseBonus, float maxBonus,
            float baseAbilityVal, float maxAbilityVal)
        {
            string path = $"{folder}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<HeroDataSO>(path);
            var hero = existing != null ? existing : ScriptableObject.CreateInstance<HeroDataSO>();

            hero.heroId = id;
            hero.heroName = name;
            hero.description = desc;
            hero.rarity = rarity;
            hero.isFreeHero = isFree;
            hero.acquisitionMethod = acquisition;
            hero.specialAbilityType = ability;
            hero.abilityName = abilityName;
            hero.abilityDescription = abilityDesc;
            hero.maxLevel = 10;

            hero.baseCookTimeMultiplier = baseCook;
            hero.maxCookTimeMultiplier = maxCook;
            hero.baseBurnTimeMultiplier = baseBurn;
            hero.maxBurnTimeMultiplier = maxBurn;
            hero.baseScoreMultiplier = baseScore;
            hero.maxScoreMultiplier = maxScore;
            hero.baseMovementSpeedMultiplier = baseSpeed;
            hero.maxMovementSpeedMultiplier = maxSpeed;
            hero.baseInteractionRadiusMultiplier = baseRadius;
            hero.maxInteractionRadiusMultiplier = maxRadius;
            hero.baseBonusTimeSeconds = baseBonus;
            hero.maxBonusTimeSeconds = maxBonus;
            hero.baseSpecialAbilityValue = baseAbilityVal;
            hero.maxSpecialAbilityValue = maxAbilityVal;

            if (existing == null)
            {
                AssetDatabase.CreateAsset(hero, path);
            }
            else
            {
                EditorUtility.SetDirty(hero);
            }
        }
    }
}
