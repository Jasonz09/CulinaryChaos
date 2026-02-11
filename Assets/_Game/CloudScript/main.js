// ═══════════════════════════════════════════════════════════════════
//  IOChef CloudScript — Server-Authoritative Handlers for PlayFab
//  Upload to PlayFab > Automation > CloudScript (Legacy)
//
//  ALL game state mutations happen here. Client NEVER calls
//  UpdateUserData, AddVirtualCurrency, or SubtractVirtualCurrency.
// ═══════════════════════════════════════════════════════════════════

// ─────────────────────── Helpers ────────────────────────

/**
 * Reads a Player Data key, returns parsed JSON or null.
 */
function getPlayerData(playFabId, key) {
    var result = server.GetUserData({ PlayFabId: playFabId, Keys: [key] });
    if (result.Data && result.Data[key]) {
        try { return JSON.parse(result.Data[key].Value); }
        catch (e) { return null; }
    }
    return null;
}

/**
 * Writes a Player Data key with a JSON value.
 */
function setPlayerData(playFabId, key, value) {
    var data = {};
    data[key] = JSON.stringify(value);
    server.UpdateUserData({ PlayFabId: playFabId, Data: data });
}

/**
 * Gets the player's virtual currency balance.
 */
function getCurrencyBalance(playFabId, currencyCode) {
    var inv = server.GetUserInventory({ PlayFabId: playFabId });
    return inv.VirtualCurrency[currencyCode] || 0;
}

/**
 * Gets Title Data key, returns parsed JSON or fallback.
 */
function getTitleData(key, fallback) {
    var result = server.GetTitleData({ Keys: [key] });
    if (result.Data && result.Data[key]) {
        try { return JSON.parse(result.Data[key]); }
        catch (e) { return fallback || null; }
    }
    return fallback || null;
}

/**
 * Gets the player's level progress data.
 */
function getLevelProgress(playFabId) {
    return getPlayerData(playFabId, "LevelProgress") || { MaxUnlockedLevel: 1 };
}

/**
 * Gets full level config from embedded WORLD_CONFIGS.
 * Returns the level object or null if not found.
 */
function getFullLevelConfig(levelId) {
    for (var wKey in WORLD_CONFIGS) {
        if (!WORLD_CONFIGS.hasOwnProperty(wKey)) continue;
        var world = WORLD_CONFIGS[wKey];
        if (!world || !world.levels) continue;
        var config = world.levels[String(levelId)];
        if (config) return config;
    }
    return null;
}

// ═══════════════════════════════════════════════════════════════════
//  WORLD & LEVEL CONFIGS — edit here to change levels server-side
//  After editing, re-upload this file to PlayFab > Automation >
//  CloudScript (Legacy). Changes take effect immediately.
// ═══════════════════════════════════════════════════════════════════

var WORLD_CONFIGS = {
    "1": {
        worldName: "THE KITCHEN",
        levels: {
            "1": {
                name: "Lettuce Salad", time: 120, orderInterval: 50,
                maxOrders: 2, initialOrders: 1,
                star1: 400, star2: 700, star3: 1100,
                unlimitedPlates: true, autoRemovePlates: true,
                requiresSink: false, plateCount: 0, entryCost: 0,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Lettuce Salad", points: 60, time: 120, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] }
                ]
            },
            "2": {
                name: "Two Salads", time: 150, orderInterval: 45,
                maxOrders: 2, initialOrders: 2,
                star1: 500, star2: 900, star3: 1400,
                unlimitedPlates: true, autoRemovePlates: true,
                requiresSink: false, plateCount: 0, entryCost: 25,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Lettuce Salad", points: 60, time: 90, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] },
                    { name: "Tomato Salad", points: 60, time: 90, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Chopped" }] }
                ]
            },
            "3": {
                name: "First Cooking", time: 150, orderInterval: 40,
                maxOrders: 3, initialOrders: 2,
                star1: 600, star2: 1100, star3: 1700,
                unlimitedPlates: true, autoRemovePlates: true,
                requiresSink: false, plateCount: 0, entryCost: 50,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Lettuce Salad", points: 60, time: 80, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] },
                    { name: "Tomato Soup", points: 100, time: 80, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Cooked" }] }
                ]
            },
            "4": {
                name: "Meat Kitchen", time: 180, orderInterval: 40,
                maxOrders: 3, initialOrders: 2,
                star1: 700, star2: 1300, star3: 2000,
                unlimitedPlates: true, autoRemovePlates: true,
                requiresSink: false, plateCount: 0, entryCost: 75,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Lettuce Salad", points: 60, time: 70, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] },
                    { name: "Tomato Soup", points: 100, time: 70, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Cooked" }] },
                    { name: "Cooked Meat", points: 120, time: 75, difficulty: 1,
                      ingredients: [{ type: "Meat", state: "Cooked" }] }
                ]
            },
            "5": {
                name: "Combo Plates", time: 180, orderInterval: 35,
                maxOrders: 3, initialOrders: 3,
                star1: 800, star2: 1500, star3: 2400,
                unlimitedPlates: true, autoRemovePlates: true,
                requiresSink: false, plateCount: 0, entryCost: 100,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Chopped Salad", points: 150, time: 80, difficulty: 2,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Cooked Meat", points: 120, time: 70, difficulty: 1,
                      ingredients: [{ type: "Meat", state: "Cooked" }] },
                    { name: "Tomato Soup", points: 100, time: 65, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Cooked" }] }
                ]
            },
            "6": {
                name: "Dirty Dishes", time: 180, orderInterval: 35,
                maxOrders: 3, initialOrders: 3,
                star1: 900, star2: 1700, star3: 2700,
                unlimitedPlates: false, autoRemovePlates: false,
                requiresSink: true, plateCount: 8, entryCost: 125,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Chopped Salad", points: 150, time: 75, difficulty: 2,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Steak Plate", points: 180, time: 80, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Lettuce", state: "Chopped" }] },
                    { name: "Tomato Soup", points: 100, time: 60, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Cooked" }] }
                ]
            },
            "7": {
                name: "Rush Hour", time: 180, orderInterval: 30,
                maxOrders: 4, initialOrders: 3,
                star1: 1000, star2: 1900, star3: 3000,
                unlimitedPlates: false, autoRemovePlates: false,
                requiresSink: true, plateCount: 8, entryCost: 150,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Steak Plate", points: 180, time: 70, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Lettuce", state: "Chopped" }] },
                    { name: "Steak & Tomato", points: 180, time: 70, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Chopped Salad", points: 150, time: 65, difficulty: 2,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Lettuce Salad", points: 60, time: 50, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] }
                ]
            },
            "8": {
                name: "Plate Crunch", time: 200, orderInterval: 30,
                maxOrders: 4, initialOrders: 3,
                star1: 1100, star2: 2100, star3: 3400,
                unlimitedPlates: false, autoRemovePlates: false,
                requiresSink: true, plateCount: 6, entryCost: 175,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Chopped Salad", points: 150, time: 55, difficulty: 2,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Steak Plate", points: 200, time: 60, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Lettuce", state: "Chopped" }] },
                    { name: "Steak & Tomato", points: 200, time: 60, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Cooked Meat", points: 120, time: 50, difficulty: 1,
                      ingredients: [{ type: "Meat", state: "Cooked" }] }
                ]
            },
            "9": {
                name: "Deluxe Kitchen", time: 200, orderInterval: 25,
                maxOrders: 4, initialOrders: 3,
                star1: 1200, star2: 2400, star3: 3800,
                unlimitedPlates: false, autoRemovePlates: false,
                requiresSink: true, plateCount: 5, entryCost: 200,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Deluxe Salad", points: 250, time: 75, difficulty: 3,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }, { type: "Meat", state: "Cooked" }] },
                    { name: "Steak Plate", points: 200, time: 55, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Lettuce", state: "Chopped" }] },
                    { name: "Chopped Salad", points: 150, time: 50, difficulty: 2,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }] }
                ]
            },
            "10": {
                name: "Grand Kitchen", time: 210, orderInterval: 22,
                maxOrders: 4, initialOrders: 4,
                star1: 1400, star2: 2700, star3: 4200,
                unlimitedPlates: false, autoRemovePlates: false,
                requiresSink: true, plateCount: 5, entryCost: 225,
                freeHeroRewardId: "",
                recipes: [
                    { name: "Deluxe Salad", points: 280, time: 65, difficulty: 3,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }, { type: "Tomato", state: "Chopped" }, { type: "Meat", state: "Cooked" }] },
                    { name: "Steak & Tomato", points: 200, time: 50, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Tomato", state: "Chopped" }] },
                    { name: "Steak Plate", points: 200, time: 50, difficulty: 2,
                      ingredients: [{ type: "Meat", state: "Cooked" }, { type: "Lettuce", state: "Chopped" }] },
                    { name: "Lettuce Salad", points: 60, time: 40, difficulty: 1,
                      ingredients: [{ type: "Lettuce", state: "Chopped" }] },
                    { name: "Tomato Soup", points: 100, time: 40, difficulty: 1,
                      ingredients: [{ type: "Tomato", state: "Cooked" }] }
                ]
            }
        }
    }
};

// ═══════════════════════════════════════════════════════════════════
//  HERO CATALOG — server-authoritative hero roster for gacha & upgrades
// ═══════════════════════════════════════════════════════════════════

var HERO_CATALOG = [
    { heroId: "hero_basil",  heroName: "Chef Basil", rarity: "Common",    isFreeHero: true,  maxLevel: 10 },
    { heroId: "hero_pepper", heroName: "Pepper",     rarity: "Common",    isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_sizzle", heroName: "Sizzle",     rarity: "Rare",      isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_dash",   heroName: "Dash",       rarity: "Rare",      isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_luna",   heroName: "Luna",       rarity: "Rare",      isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_ginger", heroName: "Ginger",     rarity: "Epic",      isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_miso",   heroName: "Miso",       rarity: "Epic",      isFreeHero: false, maxLevel: 10 },
    { heroId: "hero_noir",   heroName: "Chef Noir",  rarity: "Legendary", isFreeHero: false, maxLevel: 10 }
];

// ═══════════════════════════════════════════════════════════════════
//  CHEST CONFIG — cooldown & rate settings (no pity/guarantee system)
// ═══════════════════════════════════════════════════════════════════

var BRONZE_COOLDOWN_SECONDS = 86400; // 24 hours between free bronze chests

// ═══════════════════════════════════════════════════════════════════
//  BUNDLE CATALOG — server-authoritative bundles for shop
// ═══════════════════════════════════════════════════════════════════

var BUNDLE_CATALOG = [
    {
        bundleId: "starter_bundle", name: "Starter Bundle",
        contents: [{ type: "gems", amount: 100 }, { type: "coins", amount: 500 }, { type: "heroTokens", amount: 20 }],
        gemCost: 80, displayOrder: 1, availability: "always", valueMultiplier: 3
    },
    {
        bundleId: "hero_boost", name: "Hero Boost Pack",
        contents: [{ type: "heroTokens", amount: 50 }, { type: "gems", amount: 200 }],
        gemCost: 150, displayOrder: 2, availability: "always", valueMultiplier: 2
    },
    {
        bundleId: "mega_chest_bundle", name: "Mega Chest Bundle",
        contents: [{ type: "goldChests", amount: 5 }],
        gemCost: 600, displayOrder: 3, availability: "always", valueMultiplier: 2
    },
    {
        bundleId: "coin_vault", name: "Coin Vault",
        contents: [{ type: "coins", amount: 2000 }, { type: "heroTokens", amount: 10 }],
        gemCost: 50, displayOrder: 4, availability: "always", valueMultiplier: 3
    }
];

// ═══════════════════════════════════════════════════════════════════
//  DAILY DEALS POOL — items that can appear as daily deals
// ═══════════════════════════════════════════════════════════════════

var DAILY_DEALS_POOL = [
    { dealId: "free_coins_100",    type: "coins",      amount: 100,  normalGemCost: 5,  dealGemCost: 0, isFree: true,  weight: 10 },
    { dealId: "coins_300",         type: "coins",      amount: 300,  normalGemCost: 8,  dealGemCost: 3, isFree: false, weight: 10 },
    { dealId: "coins_500",         type: "coins",      amount: 500,  normalGemCost: 12, dealGemCost: 5, isFree: false, weight: 8 },
    { dealId: "coins_1000",        type: "coins",      amount: 1000, normalGemCost: 20, dealGemCost: 10, isFree: false, weight: 5 },
    { dealId: "tokens_5",          type: "heroTokens", amount: 5,    normalGemCost: 10, dealGemCost: 4, isFree: false, weight: 8 },
    { dealId: "tokens_15",         type: "heroTokens", amount: 15,   normalGemCost: 25, dealGemCost: 12, isFree: false, weight: 5 },
    { dealId: "tokens_30",         type: "heroTokens", amount: 30,   normalGemCost: 45, dealGemCost: 20, isFree: false, weight: 3 },
    { dealId: "free_tokens_3",     type: "heroTokens", amount: 3,    normalGemCost: 6,  dealGemCost: 0, isFree: true,  weight: 8 },
    { dealId: "gems_10",           type: "gems",       amount: 10,   normalGemCost: 0,  dealGemCost: 0, isFree: true,  weight: 4 },
    { dealId: "bronze_chest",      type: "bronzeChest", amount: 1,   normalGemCost: 5,  dealGemCost: 0, isFree: true,  weight: 6 }
];

// ═══════════════════════════════════════════════════════════════════
//  BATTLE PASS REWARDS — embedded tier reward definitions
// ═══════════════════════════════════════════════════════════════════

var BATTLE_PASS_REWARDS = {};
(function() {
    for (var t = 0; t <= 70; t++) {
        var free = {};
        var prem = {};
        // Free track: coins every tier, gems every 10 tiers
        free.coins = 50 + Math.floor(t / 5) * 25;
        if (t > 0 && t % 10 === 0) free.gems = 5 + Math.floor(t / 10) * 5;
        // Premium track: coins + tokens every tier, gems every 5 tiers
        prem.coins = 75 + Math.floor(t / 5) * 30;
        prem.tokens = 2 + Math.floor(t / 10) * 2;
        if (t > 0 && t % 5 === 0) prem.gems = 10 + Math.floor(t / 10) * 5;
        BATTLE_PASS_REWARDS[String(t)] = {
            freeCoins: free.coins || 0,
            freeGems: free.gems || 0,
            premiumCoins: prem.coins || 0,
            premiumGems: prem.gems || 0,
            premiumTokens: prem.tokens || 0
        };
    }
})();

var BATTLE_PASS_PREMIUM_COST = 500; // gems
// Season end: 30 days from a fixed season start date. Update this each season.
var BATTLE_PASS_SEASON_END = "2026-03-31T00:00:00Z";

/**
 * Server-defined chest costs. Client-sent cost is ignored.
 */
function getChestCost(rarity) {
    var costs = { Bronze: 0, Silver: 50, Gold: 150 };
    return costs[rarity] !== undefined ? costs[rarity] : 0;
}

/**
 * Server-defined ingredient prices. Client-sent price is validated.
 */
function getIngredientPrice(type) {
    var prices = {
        Lettuce: 25, Tomato: 25, Bread: 25, Bun: 25,
        Meat: 50, Cheese: 50, Sausage: 50, Vegetables: 50,
        Dough: 75, Sauce: 75, Pepperoni: 75, Pasta: 75,
        Fish: 75, Rice: 75, Tortilla: 75,
        Seaweed: 100, Broth: 100, Seasoning: 100, Noodles: 100
    };
    return prices[type] !== undefined ? prices[type] : 50;
}

// ─────────────────── Ping ──────────────────────

/**
 * Lightweight connectivity check. Client pings periodically
 * to confirm server is reachable (not just that WiFi is on).
 */
handlers.Ping = function (args, context) {
    return { ok: true, serverTimeUtc: new Date().toISOString() };
};

// ─────────────────── InitNewPlayer ──────────────────────

/**
 * Called once after first login (when NewlyCreated is true).
 * Grants starter currency and ingredient stock.
 */
handlers.InitNewPlayer = function (args, context) {
    var playFabId = currentPlayerId;

    // Check if already initialized
    var existing = getPlayerData(playFabId, "PlayerInitialized");
    if (existing) {
        return { success: false, reason: "Already initialized" };
    }

    // Grant starter currencies
    server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "CO", Amount: 500 });
    server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: 50 });

    // Grant starter ingredient stock (100 of basic ingredients)
    var starterStock = {
        Lettuce: 100, Tomato: 100, Bread: 100, Bun: 100,
        Meat: 100, Cheese: 100
    };
    setPlayerData(playFabId, "IngredientStock", starterStock);

    // Initialize level progress
    setPlayerData(playFabId, "LevelProgress", { MaxUnlockedLevel: 1 });

    // Initialize player level
    setPlayerData(playFabId, "PlayerLevelData", { level: 1, xp: 0 });

    // Mark as initialized
    setPlayerData(playFabId, "PlayerInitialized", { initialized: true });

    return { success: true };
};

// ─────────────────── Shared Chest Roll Logic ────────────────────

/**
 * Pure RNG chest roll — no pity or guarantee system.
 * Drop rates are exactly as displayed to the player:
 *   Bronze: Common 70%, Rare 25%, Epic 4%, Legendary 1%
 *   Silver: Common 50%, Rare 35%, Epic 12%, Legendary 3%
 *   Gold:   Common 25%, Rare 40%, Epic 25%, Legendary 10%
 * Mutates heroProgress in place. Does NOT save — caller saves.
 * Returns { heroId, wasNew, heroRarity, dupTokens }.
 */
function rollChest(playFabId, rarity, heroProgress) {
    // Roll hero rarity based on chest tier — pure probability
    var roll = Math.random() * 100;
    var heroRarity;

    if (rarity === "Gold") {
        if (roll < 25) heroRarity = "Common";
        else if (roll < 65) heroRarity = "Rare";
        else if (roll < 90) heroRarity = "Epic";
        else heroRarity = "Legendary";
    } else if (rarity === "Silver") {
        if (roll < 50) heroRarity = "Common";
        else if (roll < 85) heroRarity = "Rare";
        else if (roll < 97) heroRarity = "Epic";
        else heroRarity = "Legendary";
    } else {
        if (roll < 70) heroRarity = "Common";
        else if (roll < 95) heroRarity = "Rare";
        else if (roll < 99) heroRarity = "Epic";
        else heroRarity = "Legendary";
    }

    // Filter candidates by rarity
    var candidates = HERO_CATALOG.filter(function (h) { return h.rarity === heroRarity && !h.isFreeHero; });
    if (candidates.length === 0) {
        candidates = HERO_CATALOG.filter(function (h) { return !h.isFreeHero; });
    }
    if (candidates.length === 0) {
        return null;
    }

    var chosen = candidates[Math.floor(Math.random() * candidates.length)];

    // Check if player already owns this hero
    var existing = null;
    for (var i = 0; i < heroProgress.entries.length; i++) {
        if (heroProgress.entries[i].heroId === chosen.heroId) {
            existing = heroProgress.entries[i];
            break;
        }
    }

    var wasNew = !existing || !existing.isUnlocked;
    var dupTokens = 0;

    if (wasNew) {
        if (!existing) {
            heroProgress.entries.push({
                heroId: chosen.heroId,
                isUnlocked: true,
                currentLevel: 1,
                currentXP: 0
            });
        } else {
            existing.isUnlocked = true;
        }
    } else {
        dupTokens = 5;
    }

    return { heroId: chosen.heroId, wasNew: wasNew, heroRarity: heroRarity, dupTokens: dupTokens };
}

// ─────────────────── OpenChest ──────────────────────────

/**
 * Server-side gacha roll — pure RNG, no pity/guarantee system.
 * Bronze chests: free but limited to 1 every 24 hours.
 * Silver/Gold chests: cost gems, no cooldown.
 * Args: { rarity: "Bronze"|"Silver"|"Gold" }
 * Returns: { heroId, wasNew, heroRarity, cooldownReset? }
 */
handlers.OpenChest = function (args, context) {
    var playFabId = currentPlayerId;
    var rarity = args.rarity || "Bronze";

    var cost = getChestCost(rarity);
    var gems = getCurrencyBalance(playFabId, "GM");
    if (gems < cost) return { error: "Not enough gems" };

    // Bronze cooldown check (1 free every 24h)
    if (rarity === "Bronze") {
        var cooldownData = getPlayerData(playFabId, "BronzeCooldown");
        if (cooldownData && cooldownData.lastOpenedUtc) {
            var lastOpened = new Date(cooldownData.lastOpenedUtc);
            var now = new Date();
            var elapsed = (now.getTime() - lastOpened.getTime()) / 1000;
            if (elapsed < BRONZE_COOLDOWN_SECONDS) {
                var remaining = Math.ceil(BRONZE_COOLDOWN_SECONDS - elapsed);
                return { error: "Cooldown active", cooldownRemaining: remaining };
            }
        }
    }

    if (cost > 0) {
        server.SubtractUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: cost });
    }

    var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };

    var result = rollChest(playFabId, rarity, heroProgress);
    if (!result) return { error: "No heroes available" };

    setPlayerData(playFabId, "HeroProgress", heroProgress);

    // Update bronze cooldown
    if (rarity === "Bronze") {
        setPlayerData(playFabId, "BronzeCooldown", { lastOpenedUtc: new Date().toISOString() });
    }

    if (result.dupTokens > 0) {
        server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: result.dupTokens });
    }

    return { heroId: result.heroId, wasNew: result.wasNew, heroRarity: result.heroRarity };
};

// ─────────────────── OpenChestMulti ─────────────────────

/**
 * Multi-pull gacha: opens N chests at once. No pity system.
 * Only available for Silver/Gold (paid) chests.
 * Args: { rarity: string, count: int (1-10) }
 * Returns: { results: [{heroId, wasNew, heroRarity}], totalDupTokens }
 */
handlers.OpenChestMulti = function (args, context) {
    var playFabId = currentPlayerId;
    var rarity = args.rarity || "Bronze";
    var count = args.count || 10;
    if (count < 1) count = 1;
    if (count > 10) count = 10;

    // Bronze chests can't be multi-pulled (cooldown-gated)
    if (rarity === "Bronze") return { error: "Bronze chests cannot be multi-pulled" };

    var cost = getChestCost(rarity) * count;
    var gems = getCurrencyBalance(playFabId, "GM");
    if (gems < cost) return { error: "Not enough gems" };

    if (cost > 0) {
        server.SubtractUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: cost });
    }

    var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };

    var results = [];
    var totalDupTokens = 0;

    for (var i = 0; i < count; i++) {
        var r = rollChest(playFabId, rarity, heroProgress);
        if (r) {
            results.push({ heroId: r.heroId, wasNew: r.wasNew, heroRarity: r.heroRarity });
            totalDupTokens += r.dupTokens;
        }
    }

    setPlayerData(playFabId, "HeroProgress", heroProgress);

    if (totalDupTokens > 0) {
        server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: totalDupTokens });
    }

    return { results: results, totalDupTokens: totalDupTokens };
};

// ─────────────── ConvertGemToCoins ──────────────────────

/**
 * Atomic gem-to-coin conversion: 1 gem = 100 coins.
 * Args: { gemAmount: int }
 */
handlers.ConvertGemToCoins = function (args, context) {
    var playFabId = currentPlayerId;
    var gemAmount = args.gemAmount || 0;

    if (gemAmount <= 0) return { error: "Invalid amount" };

    var gems = getCurrencyBalance(playFabId, "GM");
    if (gems < gemAmount) return { error: "Not enough gems" };

    server.SubtractUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "GM",
        Amount: gemAmount
    });

    server.AddUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "CO",
        Amount: gemAmount * 100
    });

    return { success: true, coinsAdded: gemAmount * 100 };
};

// ─────────────── PurchaseIngredient ─────────────────────

/**
 * Server-validated ingredient purchase.
 * Server uses its own price table (ignores client-sent price).
 * Args: { type: string, batchSize: int }
 */
handlers.PurchaseIngredient = function (args, context) {
    var playFabId = currentPlayerId;
    var type = args.type || "";
    var batchSize = args.batchSize || 100;
    var maxStock = 500;

    // Use server-defined price
    var price = getIngredientPrice(type);
    if (price <= 0) return { error: "Invalid ingredient type" };

    // Cap batch size
    if (batchSize <= 0 || batchSize > 100) batchSize = 100;

    // Validate coin balance
    var coins = getCurrencyBalance(playFabId, "CO");
    if (coins < price) return { error: "Not enough coins" };

    // Get current stock
    var stock = getPlayerData(playFabId, "IngredientStock") || {};
    var current = stock[type] || 0;

    if (current >= maxStock) return { error: "Stock is full" };

    var toAdd = Math.min(batchSize, maxStock - current);

    // Deduct coins
    server.SubtractUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "CO",
        Amount: price
    });

    // Update stock
    stock[type] = current + toAdd;
    setPlayerData(playFabId, "IngredientStock", stock);

    return { success: true, newStock: stock[type] };
};

// ─────────────── SyncIngredientStock ────────────────────

/**
 * Deducts consumed ingredients at end of a level.
 * Validates that consumed amounts don't exceed current stock.
 * Args: { consumed: { "Lettuce": 5, "Meat": 3, ... } }
 */
handlers.SyncIngredientStock = function (args, context) {
    var playFabId = currentPlayerId;
    var consumed = args.consumed || {};

    var stock = getPlayerData(playFabId, "IngredientStock") || {};
    var changed = false;

    for (var type in consumed) {
        if (consumed.hasOwnProperty(type)) {
            var amount = consumed[type];
            if (amount <= 0) continue;

            var current = stock[type] || 0;
            // Clamp: never go below 0
            stock[type] = Math.max(0, current - amount);
            changed = true;

            if (amount > current) {
                log.error("SyncIngredientStock: " + type + " consumed " + amount + " but only had " + current);
            }
        }
    }

    if (changed) {
        setPlayerData(playFabId, "IngredientStock", stock);
    }

    return { success: true };
};

// ─────────────── CompleteLevel ──────────────────────────

/**
 * Server-validated level completion. Validates score against Title Data
 * level configs, records best score and stars, unlocks next level,
 * grants coins, player XP, battle pass XP, and hero unlocks.
 *
 * Args: { levelId, score, stars, ordersCompleted, ordersFailed, bestCombo, freeHeroRewardId }
 */
handlers.CompleteLevel = function (args, context) {
    var playFabId = currentPlayerId;
    var levelId = args.levelId || 0;
    var score = args.score || 0;
    var stars = args.stars || 0;
    var ordersCompleted = args.ordersCompleted || 0;
    var ordersFailed = args.ordersFailed || 0;
    var bestCombo = args.bestCombo || 0;
    var freeHeroRewardId = args.freeHeroRewardId || "";

    // Validate level is unlocked
    var progress = getLevelProgress(playFabId);
    var maxUnlocked = progress.MaxUnlockedLevel || 1;
    if (levelId > maxUnlocked) {
        log.error("CompleteLevel: level " + levelId + " not unlocked (max=" + maxUnlocked + ")");
        return { error: "Level not unlocked" };
    }

    // Validate against Title Data level configs
    // Try World_X keys first (full level data), then legacy LevelConfigs key
    var config = getFullLevelConfig(levelId);
    if (!config) {
        var levelConfigs = getTitleData("LevelConfigs", {});
        config = levelConfigs[String(levelId)];
    }

    if (config) {
        // Compute maxScore from recipes if not explicitly set
        if (!config.maxScore && config.recipes && config.time) {
            var maxRecipePoints = 0;
            for (var r = 0; r < config.recipes.length; r++) {
                var rp = config.recipes[r].points || 0;
                if (rp > maxRecipePoints) maxRecipePoints = rp;
            }
            var maxPossibleOrders = Math.ceil(config.time / (config.orderInterval || 30));
            config.maxScore = maxPossibleOrders * maxRecipePoints + 1000;
        }

        // Enforce server-side score cap
        if (config.maxScore && score > config.maxScore) {
            log.error("CompleteLevel: score " + score + " exceeds max " + config.maxScore + " for level " + levelId);
            score = config.maxScore;
        }

        // Re-calculate stars from server thresholds
        if (config.star3 && config.star2 && config.star1) {
            if (score >= config.star3) stars = 3;
            else if (score >= config.star2) stars = 2;
            else if (score >= config.star1) stars = 1;
            else stars = 0;
        }
    } else {
        // No config: apply generous plausibility check
        var maxPlausibleScore = ordersCompleted * 500 + 1000;
        if (score > maxPlausibleScore) {
            log.error("CompleteLevel: suspicious score " + score + " for " + ordersCompleted + " orders on level " + levelId);
            score = maxPlausibleScore;
            stars = Math.min(stars, 2);
        }
    }

    // Clamp stars
    if (stars < 0) stars = 0;
    if (stars > 3) stars = 3;

    // Read existing level progress
    var bestScoreKey = "Level_" + levelId + "_BestScore";
    var starsKey = "Level_" + levelId + "_Stars";
    var heroGrantedKey = "Level_" + levelId + "_HeroGranted";

    var previousBest = progress[bestScoreKey] || 0;
    var newBest = score > previousBest;

    if (newBest) {
        progress[bestScoreKey] = score;
        progress[starsKey] = stars;
    }

    // Update max unlocked level
    if (levelId >= maxUnlocked && stars >= 1) {
        progress.MaxUnlockedLevel = levelId + 1;
    }

    // Grant level completion coins: base 50 + 25 per star
    var coinReward = 50 + stars * 25;
    server.AddUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "CO",
        Amount: coinReward
    });

    // Grant hero tokens: 1 per order completed
    if (ordersCompleted > 0) {
        server.AddUserVirtualCurrency({
            PlayFabId: playFabId,
            VirtualCurrency: "HT",
            Amount: ordersCompleted
        });
    }

    // Grant player XP: 50 base + 25 per star + 10 per order
    var xpReward = 50 + stars * 25 + ordersCompleted * 10;
    var levelData = getPlayerData(playFabId, "PlayerLevelData") || { level: 1, xp: 0 };
    levelData.xp += xpReward;
    var xpNeeded = 100 * levelData.level;
    while (levelData.xp >= xpNeeded) {
        levelData.xp -= xpNeeded;
        levelData.level++;
        xpNeeded = 100 * levelData.level;
    }
    setPlayerData(playFabId, "PlayerLevelData", levelData);

    // Grant battle pass XP: 100 base + 50 per star
    var bpXpReward = 100 + stars * 50;
    var bpData = getPlayerData(playFabId, "BattlePassData");
    if (bpData) {
        bpData.xp = (bpData.xp || 0) + bpXpReward;
        var bpXpPerTier = 1000;
        while (bpData.xp >= bpXpPerTier && (bpData.tier || 0) < 70) {
            bpData.xp -= bpXpPerTier;
            bpData.tier = (bpData.tier || 0) + 1;
        }
        setPlayerData(playFabId, "BattlePassData", bpData);
    }

    // Free hero unlock (first completion only)
    var unlockedHeroId = "";
    if (stars >= 1 && freeHeroRewardId && !progress[heroGrantedKey]) {
        progress[heroGrantedKey] = 1;
        unlockedHeroId = freeHeroRewardId;

        var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };
        var found = false;
        for (var i = 0; i < heroProgress.entries.length; i++) {
            if (heroProgress.entries[i].heroId === freeHeroRewardId) {
                heroProgress.entries[i].isUnlocked = true;
                found = true;
                break;
            }
        }
        if (!found) {
            heroProgress.entries.push({
                heroId: freeHeroRewardId,
                isUnlocked: true,
                currentLevel: 1,
                currentXP: 0
            });
        }
        setPlayerData(playFabId, "HeroProgress", heroProgress);
    }

    setPlayerData(playFabId, "LevelProgress", progress);

    return {
        success: true,
        newBest: newBest,
        bestScore: newBest ? score : previousBest,
        bestStars: newBest ? stars : (progress[starsKey] || 0),
        maxUnlockedLevel: progress.MaxUnlockedLevel,
        coinReward: coinReward,
        xpReward: xpReward,
        bpXpReward: bpXpReward,
        unlockedHeroId: unlockedHeroId,
        playerLevel: levelData.level,
        playerXP: levelData.xp
    };
};

// ─────────────── AddPlayerXP ────────────────────────────

/**
 * Server-side player XP addition with level-up processing.
 * Args: { amount: int }
 * Returns: { level: int, xp: int }
 */
handlers.AddPlayerXP = function (args, context) {
    var playFabId = currentPlayerId;
    var amount = args.amount || 0;

    if (amount <= 0 || amount > 10000) {
        return { error: "Invalid XP amount" };
    }

    var data = getPlayerData(playFabId, "PlayerLevelData") || { level: 1, xp: 0 };
    data.xp += amount;

    // Process level-ups: XP needed = 100 * level
    var xpNeeded = 100 * data.level;
    while (data.xp >= xpNeeded) {
        data.xp -= xpNeeded;
        data.level++;
        xpNeeded = 100 * data.level;
    }

    setPlayerData(playFabId, "PlayerLevelData", data);

    return { level: data.level, xp: data.xp };
};

// ─────────────── ClaimDailyLogin ────────────────────────

/**
 * Server-time daily login claim. Uses server UTC date to prevent clock manipulation.
 * Args: (none required — server determines everything)
 * Returns: { success, day, streak, reward, isGem }
 */
handlers.ClaimDailyLogin = function (args, context) {
    var playFabId = currentPlayerId;

    // Get server time (UTC)
    var now = new Date();
    var today = now.toISOString().substring(0, 10); // "YYYY-MM-DD"

    var loginData = getPlayerData(playFabId, "DailyLoginData") || {
        lastLogin: "",
        streak: 0,
        day: 0,
        claimedToday: false
    };

    // Already claimed today
    if (loginData.lastLogin === today && loginData.claimedToday) {
        return { success: false, reason: "Already claimed today" };
    }

    // Calculate streak
    if (loginData.lastLogin) {
        var lastDate = new Date(loginData.lastLogin);
        var todayDate = new Date(today);
        var diffDays = Math.round((todayDate - lastDate) / (1000 * 60 * 60 * 24));

        if (diffDays === 1) {
            loginData.streak++;
        } else if (diffDays > 1) {
            loginData.streak = 1;
        }
    } else {
        loginData.streak = 1;
    }

    loginData.day = (loginData.day % 30) + 1;
    loginData.lastLogin = today;
    loginData.claimedToday = true;

    // Calculate reward
    var day = loginData.day;
    var reward;
    var isGem = (day === 21 || day === 30);

    if (day === 1) reward = 50;
    else if (day === 7) reward = 200;
    else if (day === 14) reward = 500;
    else if (day === 21) reward = 50;
    else if (day === 30) reward = 100;
    else reward = 25 + (day * 5);

    // Grant reward
    if (isGem) {
        server.AddUserVirtualCurrency({
            PlayFabId: playFabId,
            VirtualCurrency: "GM",
            Amount: reward
        });
    } else {
        server.AddUserVirtualCurrency({
            PlayFabId: playFabId,
            VirtualCurrency: "CO",
            Amount: reward
        });
    }

    setPlayerData(playFabId, "DailyLoginData", loginData);

    return {
        success: true,
        day: loginData.day,
        streak: loginData.streak,
        reward: reward,
        isGem: isGem
    };
};

// ─────────────── UpgradeHero ────────────────────────────

/**
 * Server-validated hero upgrade using hero tokens.
 * Args: { heroId: string }
 */
handlers.UpgradeHero = function (args, context) {
    var playFabId = currentPlayerId;
    var heroId = args.heroId || "";

    var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };
    var entry = null;
    for (var i = 0; i < heroProgress.entries.length; i++) {
        if (heroProgress.entries[i].heroId === heroId) {
            entry = heroProgress.entries[i];
            break;
        }
    }

    if (!entry || !entry.isUnlocked) {
        return { error: "Hero not unlocked" };
    }

    // Get hero catalog for max level
    var heroes = HERO_CATALOG;
    var heroData = null;
    for (var j = 0; j < heroes.length; j++) {
        if (heroes[j].heroId === heroId) {
            heroData = heroes[j];
            break;
        }
    }

    var maxLevel = heroData ? (heroData.maxLevel || 10) : 10;
    if (entry.currentLevel >= maxLevel) {
        return { error: "Already max level" };
    }

    // Calculate cost: 10 * currentLevel
    var cost = 10 * entry.currentLevel;
    var tokens = getCurrencyBalance(playFabId, "HT");
    if (tokens < cost) {
        return { error: "Not enough hero tokens" };
    }

    // Deduct tokens and upgrade
    server.SubtractUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "HT",
        Amount: cost
    });

    entry.currentLevel++;
    setPlayerData(playFabId, "HeroProgress", heroProgress);

    return { success: true, newLevel: entry.currentLevel };
};

// ─────────────── CheckDailyQuests ───────────────────────

/**
 * Checks if daily quests need regeneration using server UTC date.
 * If it's a new day, generates new quests from the provided pool.
 * Returns the current quest list.
 *
 * Args: { questPool: [{ questId, description, targetCount, creditReward, difficulty }] }
 * Returns: { quests: [...], rerolls: int }
 */
handlers.CheckDailyQuests = function (args, context) {
    var playFabId = currentPlayerId;
    var questPool = args.questPool || [];

    var now = new Date();
    var today = now.toISOString().substring(0, 10);

    var questData = getPlayerData(playFabId, "DailyQuestData") || {
        date: "",
        quests: [],
        rerolls: 0
    };

    // If same day, return existing quests
    if (questData.date === today && questData.quests && questData.quests.length > 0) {
        return { quests: questData.quests, rerolls: questData.rerolls || 0 };
    }

    // New day — generate fresh quests
    var easyPool = questPool.filter(function (q) { return q.difficulty === "easy"; });
    var mediumPool = questPool.filter(function (q) { return q.difficulty === "medium"; });
    var hardPool = questPool.filter(function (q) { return q.difficulty === "hard"; });

    var newQuests = [];

    if (easyPool.length > 0) {
        var e = easyPool[Math.floor(Math.random() * easyPool.length)];
        newQuests.push({
            questId: e.questId,
            description: e.description,
            targetCount: e.targetCount,
            currentCount: 0,
            creditReward: e.creditReward,
            isCompleted: false,
            isClaimed: false
        });
    }
    if (mediumPool.length > 0) {
        var m = mediumPool[Math.floor(Math.random() * mediumPool.length)];
        newQuests.push({
            questId: m.questId,
            description: m.description,
            targetCount: m.targetCount,
            currentCount: 0,
            creditReward: m.creditReward,
            isCompleted: false,
            isClaimed: false
        });
    }
    if (hardPool.length > 0) {
        var h = hardPool[Math.floor(Math.random() * hardPool.length)];
        newQuests.push({
            questId: h.questId,
            description: h.description,
            targetCount: h.targetCount,
            currentCount: 0,
            creditReward: h.creditReward,
            isCompleted: false,
            isClaimed: false
        });
    }

    questData.date = today;
    questData.quests = newQuests;
    questData.rerolls = 0;

    setPlayerData(playFabId, "DailyQuestData", questData);

    return { quests: newQuests, rerolls: 0 };
};

// ─────────────── UpdateQuestProgress ────────────────────

/**
 * Server-side quest progress update. Increments matching quests and
 * marks them as completed when target is reached.
 *
 * Args: { questType: string, amount: int }
 * Returns: { quests: [...] }
 */
handlers.UpdateQuestProgress = function (args, context) {
    var playFabId = currentPlayerId;
    var questType = args.questType || "";
    var amount = args.amount || 1;

    if (amount <= 0 || amount > 1000) {
        return { error: "Invalid amount" };
    }

    var questData = getPlayerData(playFabId, "DailyQuestData");
    if (!questData || !questData.quests) {
        return { error: "No active quests" };
    }

    var changed = false;
    for (var i = 0; i < questData.quests.length; i++) {
        var quest = questData.quests[i];
        if (quest.isCompleted) continue;

        if (quest.questId.indexOf(questType) !== -1) {
            quest.currentCount += amount;
            if (quest.currentCount >= quest.targetCount) {
                quest.currentCount = quest.targetCount;
                quest.isCompleted = true;
            }
            changed = true;
        }
    }

    if (changed) {
        setPlayerData(playFabId, "DailyQuestData", questData);
    }

    return { quests: questData.quests };
};

// ─────────────── ClaimQuestReward ───────────────────────

/**
 * Server-validated quest reward claim.
 * Server reads the reward from its own quest data (ignores client-sent reward).
 * Args: { questIndex: int }
 */
handlers.ClaimQuestReward = function (args, context) {
    var playFabId = currentPlayerId;
    var questIndex = args.questIndex || 0;

    var questData = getPlayerData(playFabId, "DailyQuestData");
    if (!questData || !questData.quests || questIndex >= questData.quests.length) {
        return { error: "Quest not found" };
    }

    var quest = questData.quests[questIndex];
    if (!quest.isCompleted) {
        return { error: "Quest not completed" };
    }
    if (quest.isClaimed) {
        return { error: "Quest already claimed" };
    }

    // Use server-stored reward value (NOT client-sent)
    var reward = quest.creditReward || 0;
    if (reward <= 0) {
        return { error: "Invalid reward" };
    }

    quest.isClaimed = true;
    setPlayerData(playFabId, "DailyQuestData", questData);

    // Grant coin reward
    server.AddUserVirtualCurrency({
        PlayFabId: playFabId,
        VirtualCurrency: "CO",
        Amount: reward
    });

    return { success: true, reward: reward };
};

// ─────────────── AddBattlePassXP ────────────────────────

/**
 * Server-side battle pass XP addition with tier-up processing.
 * Premium pass gives 1.5x XP bonus.
 *
 * Args: { amount: int, season: int }
 * Returns: { tier: int, xp: int }
 */
handlers.AddBattlePassXP = function (args, context) {
    var playFabId = currentPlayerId;
    var amount = args.amount || 0;
    var season = args.season || 1;

    // Validate season hasn't ended
    var now = new Date();
    var seasonEnd = new Date(BATTLE_PASS_SEASON_END);
    if (now >= seasonEnd) {
        return { error: "Season has ended" };
    }

    if (amount <= 0 || amount > 50000) {
        return { error: "Invalid XP amount" };
    }

    var bpData = getPlayerData(playFabId, "BattlePassData") || {
        season: season,
        tier: 0,
        xp: 0,
        premium: false,
        claimedFree: [],
        claimedPremium: []
    };

    // Season mismatch — reset
    if (bpData.season !== season) {
        bpData = {
            season: season,
            tier: 0,
            xp: 0,
            premium: false,
            claimedFree: [],
            claimedPremium: []
        };
    }

    // Premium bonus
    var xpToAdd = bpData.premium ? Math.floor(amount * 1.5) : amount;

    bpData.xp += xpToAdd;

    // Process tier-ups: 1000 XP per tier, max 70 tiers
    var xpPerTier = 1000;
    var maxTier = 70;
    while (bpData.xp >= xpPerTier && bpData.tier < maxTier) {
        bpData.xp -= xpPerTier;
        bpData.tier++;
    }

    // Cap XP if at max tier
    if (bpData.tier >= maxTier) {
        bpData.xp = 0;
    }

    setPlayerData(playFabId, "BattlePassData", bpData);

    return { tier: bpData.tier, xp: bpData.xp };
};

// ─────────────── ClaimBPReward ──────────────────────────

/**
 * Server-validated battle pass reward claim.
 * Reads reward definitions from Title Data "BattlePassRewards".
 *
 * Args: { tier: int, season: int, isPremium: bool }
 * Returns: { success, claimedFree, claimedPremium }
 */
handlers.ClaimBPReward = function (args, context) {
    var playFabId = currentPlayerId;
    var tier = args.tier || 0;
    var season = args.season || 1;
    var isPremium = args.isPremium || false;

    // Validate season hasn't ended
    var now = new Date();
    var seasonEnd = new Date(BATTLE_PASS_SEASON_END);
    if (now >= seasonEnd) {
        return { error: "Season has ended", serverTimeUtc: now.toISOString() };
    }

    var bpData = getPlayerData(playFabId, "BattlePassData");
    if (!bpData || bpData.season !== season) {
        return { error: "No battle pass data for this season", serverTimeUtc: now.toISOString() };
    }

    if (tier > bpData.tier) {
        return { error: "Tier not reached", serverTimeUtc: now.toISOString() };
    }

    // Validate tier is within valid range
    if (tier < 0 || tier > 70) {
        return { error: "Invalid tier", serverTimeUtc: now.toISOString() };
    }

    // Initialize claim arrays if missing
    if (!bpData.claimedFree) bpData.claimedFree = [];
    if (!bpData.claimedPremium) bpData.claimedPremium = [];

    var claimedFree = false;
    var claimedPremium = false;

    // Get reward definitions from embedded rewards
    var tierRewards = BATTLE_PASS_REWARDS[String(tier)] || {};

    // Claim free reward if not yet claimed
    if (bpData.claimedFree.indexOf(tier) === -1) {
        bpData.claimedFree.push(tier);
        claimedFree = true;

        // Grant free reward
        if (tierRewards.freeCoins) {
            server.AddUserVirtualCurrency({
                PlayFabId: playFabId,
                VirtualCurrency: "CO",
                Amount: tierRewards.freeCoins
            });
        }
        if (tierRewards.freeGems) {
            server.AddUserVirtualCurrency({
                PlayFabId: playFabId,
                VirtualCurrency: "GM",
                Amount: tierRewards.freeGems
            });
        }
    }

    // Claim premium reward if pass is premium and not yet claimed
    if (isPremium && bpData.premium && bpData.claimedPremium.indexOf(tier) === -1) {
        bpData.claimedPremium.push(tier);
        claimedPremium = true;

        // Grant premium reward
        if (tierRewards.premiumCoins) {
            server.AddUserVirtualCurrency({
                PlayFabId: playFabId,
                VirtualCurrency: "CO",
                Amount: tierRewards.premiumCoins
            });
        }
        if (tierRewards.premiumGems) {
            server.AddUserVirtualCurrency({
                PlayFabId: playFabId,
                VirtualCurrency: "GM",
                Amount: tierRewards.premiumGems
            });
        }
        if (tierRewards.premiumTokens) {
            server.AddUserVirtualCurrency({
                PlayFabId: playFabId,
                VirtualCurrency: "HT",
                Amount: tierRewards.premiumTokens
            });
        }
    }

    if (!claimedFree && !claimedPremium) {
        return { error: "Already claimed", serverTimeUtc: now.toISOString() };
    }

    // Track last claim time for client-side display
    bpData.lastClaimUtc = now.toISOString();
    setPlayerData(playFabId, "BattlePassData", bpData);

    return {
        success: true,
        claimedFree: claimedFree,
        claimedPremium: claimedPremium,
        lastClaimUtc: bpData.lastClaimUtc,
        serverTimeUtc: now.toISOString()
    };
};

// ─────────────── StartLevel ──────────────────────────

/**
 * Server-validated level start. Checks level is unlocked, validates
 * entry cost from server config, deducts coins if needed.
 * Args: { levelId: int }
 * Returns: { success, entryCost }
 */
handlers.StartLevel = function (args, context) {
    var playFabId = currentPlayerId;
    var levelId = args.levelId || 0;

    if (levelId <= 0) return { error: "Invalid level ID" };

    // Check level is unlocked
    var progress = getLevelProgress(playFabId);
    var maxUnlocked = progress.MaxUnlockedLevel || 1;
    if (levelId > maxUnlocked) {
        return { error: "Level not unlocked" };
    }

    // Get entry cost from server config (never trust client)
    var config = getFullLevelConfig(levelId);
    var entryCost = 0;
    if (config) {
        entryCost = config.entryCost || 0;
    }

    // Deduct entry cost if any
    if (entryCost > 0) {
        var coins = getCurrencyBalance(playFabId, "CO");
        if (coins < entryCost) {
            return { error: "Not enough coins", needed: entryCost, have: coins };
        }
        server.SubtractUserVirtualCurrency({
            PlayFabId: playFabId,
            VirtualCurrency: "CO",
            Amount: entryCost
        });
    }

    return { success: true, entryCost: entryCost };
};

// ─────────────── GetWorldConfigs ──────────────────────

/**
 * Returns the embedded WORLD_CONFIGS object to the client.
 * Client uses this to build level data at runtime.
 * Args: (none)
 */
handlers.GetWorldConfigs = function (args, context) {
    return WORLD_CONFIGS;
};

// ─────────────── Cosmetic Purchases ──────────────────────

/**
 * Validates and completes cosmetic purchase.
 * Deducts currency server-side and records in UserData.
 * Args: { cosmeticId, price, currencyType }
 */
handlers.PurchaseCosmetic = function (args, context) {
    var playFabId = context.playerId;
    if (!args.cosmeticId || !args.price || !args.currencyType) {
        return { success: false, error: "Missing parameters" };
    }

    var currData = getPlayerData(playFabId, "CosmeticData") || { owned: [], equipped: {} };
    if (typeof currData === "string") currData = JSON.parse(currData);

    // Check already owned
    if (currData.owned.indexOf(args.cosmeticId) !== -1) {
        return { success: false, error: "Already owned" };
    }

    // Validate currency and deduct
    if (args.currencyType === "credits") {
        var coins = getPlayerData(playFabId, "Coins") || 0;
        if (coins < args.price) {
            return { success: false, error: "Insufficient coins" };
        }
        setPlayerData(playFabId, "Coins", coins - args.price);
    } else if (args.currencyType === "gems") {
        var gems = getPlayerData(playFabId, "Gems") || 0;
        if (gems < args.price) {
            return { success: false, error: "Insufficient gems" };
        }
        setPlayerData(playFabId, "Gems", gems - args.price);
    } else {
        return { success: false, error: "Invalid currency type" };
    }

    // Add cosmetic
    currData.owned.push(args.cosmeticId);
    setPlayerData(playFabId, "CosmeticData", currData);

    return { success: true };
};

/**
 * Validates and equips a cosmetic.
 * Args: { cosmeticId, cosmeticType }
 */
handlers.EquipCosmetic = function (args, context) {
    var playFabId = context.playerId;
    if (!args.cosmeticId || args.cosmeticType === undefined) {
        return { success: false, error: "Missing parameters" };
    }

    var currData = getPlayerData(playFabId, "CosmeticData") || { owned: [], equipped: {} };
    if (typeof currData === "string") currData = JSON.parse(currData);

    // Check ownership
    if (currData.owned.indexOf(args.cosmeticId) === -1) {
        return { success: false, error: "Cosmetic not owned" };
    }

    currData.equipped[args.cosmeticType] = args.cosmeticId;
    setPlayerData(playFabId, "CosmeticData", currData);

    return { success: true };
};

// ─────────────── Skin Purchases ──────────────────────

/**
 * Validates and completes skin purchase.
 * Deducts currency server-side and records in UserData.
 * Args: { skinId, price, currencyType }
 */
handlers.PurchaseSkin = function (args, context) {
    var playFabId = context.playerId;
    if (!args.skinId || !args.price || !args.currencyType) {
        return { success: false, error: "Missing parameters" };
    }

    var skinData = getPlayerData(playFabId, "SkinData") || { owned: [], equipped: {} };
    if (typeof skinData === "string") skinData = JSON.parse(skinData);

    // Check already owned
    if (skinData.owned.indexOf(args.skinId) !== -1) {
        return { success: false, error: "Already owned" };
    }

    // Validate currency and deduct
    if (args.currencyType === "coins") {
        var coins = getPlayerData(playFabId, "Coins") || 0;
        if (coins < args.price) {
            return { success: false, error: "Insufficient coins" };
        }
        setPlayerData(playFabId, "Coins", coins - args.price);
    } else if (args.currencyType === "gems") {
        var gems = getPlayerData(playFabId, "Gems") || 0;
        if (gems < args.price) {
            return { success: false, error: "Insufficient gems" };
        }
        setPlayerData(playFabId, "Gems", gems - args.price);
    } else {
        return { success: false, error: "Invalid currency type" };
    }

    // Add skin
    skinData.owned.push(args.skinId);
    setPlayerData(playFabId, "SkinData", skinData);

    return { success: true };
};

/**
 * Validates and equips a skin.
 * Args: { skinId, skinType }
 */
handlers.EquipSkin = function (args, context) {
    var playFabId = context.playerId;
    if (!args.skinId || args.skinType === undefined) {
        return { success: false, error: "Missing parameters" };
    }

    var skinData = getPlayerData(playFabId, "SkinData") || { owned: [], equipped: {} };
    if (typeof skinData === "string") skinData = JSON.parse(skinData);

    // Check ownership
    if (skinData.owned.indexOf(args.skinId) === -1) {
        return { success: false, error: "Skin not owned" };
    }

    skinData.equipped[args.skinType] = args.skinId;
    setPlayerData(playFabId, "SkinData", skinData);

    return { success: true };
};

// ─────────────── GetHeroCatalog ────────────────────────

/**
 * Returns the server-authoritative hero catalog to the client.
 * No args required.
 */
handlers.GetHeroCatalog = function (args, context) {
    return { heroes: HERO_CATALOG };
};

// ─────────────── GetShopData ──────────────────────────

/**
 * Returns all shop data in one call: bundles, daily deals, cooldown info.
 * Args: (none)
 */
handlers.GetShopData = function (args, context) {
    var playFabId = currentPlayerId;

    // Bundles
    var purchased = getPlayerData(playFabId, "PurchasedBundles") || [];
    var availableBundles = BUNDLE_CATALOG.filter(function(b) {
        return purchased.indexOf(b.bundleId) === -1;
    });

    // Daily deals
    var now = new Date();
    var today = now.toISOString().substring(0, 10);

    // Deterministic seed from date
    var seed = 0;
    for (var c = 0; c < today.length; c++) {
        seed = ((seed << 5) - seed) + today.charCodeAt(c);
        seed = seed & seed;
    }
    if (seed < 0) seed = -seed;

    // Select 4 deals: 1 free + 3 paid via seeded shuffle
    var freeDeals = DAILY_DEALS_POOL.filter(function(d) { return d.isFree; });
    var paidDeals = DAILY_DEALS_POOL.filter(function(d) { return !d.isFree; });

    var todayFree = freeDeals[seed % freeDeals.length];
    var shuffled = paidDeals.slice().sort(function(a, b) {
        var ha = (seed * 31 + a.dealId.length) % 1000;
        var hb = (seed * 31 + b.dealId.length) % 1000;
        return ha - hb;
    });
    var todayPaid = shuffled.slice(0, 3);
    var todayDeals = [todayFree].concat(todayPaid);

    var dealsPurchased = getPlayerData(playFabId, "DailyDealsPurchased") || { date: "", bought: [] };
    if (dealsPurchased.date !== today) {
        dealsPurchased = { date: today, bought: [] };
        setPlayerData(playFabId, "DailyDealsPurchased", dealsPurchased);
    }

    // Seconds until midnight UTC
    var tomorrow = new Date(now);
    tomorrow.setUTCDate(tomorrow.getUTCDate() + 1);
    tomorrow.setUTCHours(0, 0, 0, 0);
    var secondsUntilReset = Math.floor((tomorrow - now) / 1000);

    // Bronze cooldown info
    var cooldownData = getPlayerData(playFabId, "BronzeCooldown");
    var bronzeCooldownRemaining = 0;
    if (cooldownData && cooldownData.lastOpenedUtc) {
        var lastOpened = new Date(cooldownData.lastOpenedUtc);
        var now2 = new Date();
        var elapsed = (now2.getTime() - lastOpened.getTime()) / 1000;
        bronzeCooldownRemaining = Math.max(0, Math.ceil(BRONZE_COOLDOWN_SECONDS - elapsed));
    }

    return {
        bundles: availableBundles,
        dailyDeals: todayDeals,
        purchasedDeals: dealsPurchased.bought,
        secondsUntilReset: secondsUntilReset,
        bronzeCooldownRemaining: bronzeCooldownRemaining,
        serverTime: now.toISOString()
    };
};

// ─────────────── PurchaseBundle ──────────────────────────

/**
 * Validates and completes a bundle purchase.
 * Args: { bundleId: string }
 */
handlers.PurchaseBundle = function (args, context) {
    var playFabId = currentPlayerId;
    var bundleId = args.bundleId || "";

    var bundle = null;
    for (var i = 0; i < BUNDLE_CATALOG.length; i++) {
        if (BUNDLE_CATALOG[i].bundleId === bundleId) { bundle = BUNDLE_CATALOG[i]; break; }
    }
    if (!bundle) return { error: "Bundle not found" };

    // Check not already purchased
    var purchased = getPlayerData(playFabId, "PurchasedBundles") || [];
    if (purchased.indexOf(bundleId) !== -1) return { error: "Already purchased" };

    // Validate gem cost
    var gems = getCurrencyBalance(playFabId, "GM");
    if (gems < bundle.gemCost) return { error: "Not enough gems" };

    // Deduct gems
    if (bundle.gemCost > 0) {
        server.SubtractUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: bundle.gemCost });
    }

    // Grant contents
    for (var j = 0; j < bundle.contents.length; j++) {
        var item = bundle.contents[j];
        if (item.type === "coins") {
            server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "CO", Amount: item.amount });
        } else if (item.type === "gems") {
            server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: item.amount });
        } else if (item.type === "heroTokens") {
            server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: item.amount });
        } else if (item.type === "goldChests") {
            // Grant N gold chest rolls
            var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };
            var totalDup = 0;
            for (var k = 0; k < item.amount; k++) {
                var r = rollChest(playFabId, "Gold", heroProgress);
                if (r && r.dupTokens > 0) totalDup += r.dupTokens;
            }
            setPlayerData(playFabId, "HeroProgress", heroProgress);
            if (totalDup > 0) {
                server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: totalDup });
            }
        }
    }

    // Record purchase
    purchased.push(bundleId);
    setPlayerData(playFabId, "PurchasedBundles", purchased);

    return { success: true, bundleId: bundleId };
};

// ─────────────── PurchaseDailyDeal ──────────────────────

/**
 * Purchases a daily deal. Server validates it exists today and hasn't been bought.
 * Args: { dealId: string }
 */
handlers.PurchaseDailyDeal = function (args, context) {
    var playFabId = currentPlayerId;
    var dealId = args.dealId || "";

    // Regenerate today's deals to validate
    var now = new Date();
    var today = now.toISOString().substring(0, 10);

    var seed = 0;
    for (var c = 0; c < today.length; c++) {
        seed = ((seed << 5) - seed) + today.charCodeAt(c);
        seed = seed & seed;
    }
    if (seed < 0) seed = -seed;

    var freeDeals = DAILY_DEALS_POOL.filter(function(d) { return d.isFree; });
    var paidDeals = DAILY_DEALS_POOL.filter(function(d) { return !d.isFree; });
    var todayFree = freeDeals[seed % freeDeals.length];
    var shuffled = paidDeals.slice().sort(function(a, b) {
        var ha = (seed * 31 + a.dealId.length) % 1000;
        var hb = (seed * 31 + b.dealId.length) % 1000;
        return ha - hb;
    });
    var todayDeals = [todayFree].concat(shuffled.slice(0, 3));

    // Find the requested deal in today's list
    var deal = null;
    for (var i = 0; i < todayDeals.length; i++) {
        if (todayDeals[i].dealId === dealId) { deal = todayDeals[i]; break; }
    }
    if (!deal) return { error: "Deal not available today" };

    // Check not already purchased
    var purchased = getPlayerData(playFabId, "DailyDealsPurchased") || { date: "", bought: [] };
    if (purchased.date !== today) purchased = { date: today, bought: [] };
    if (purchased.bought.indexOf(dealId) !== -1) return { error: "Already purchased today" };

    // Deduct gem cost
    if (deal.dealGemCost > 0) {
        var gems = getCurrencyBalance(playFabId, "GM");
        if (gems < deal.dealGemCost) return { error: "Not enough gems" };
        server.SubtractUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: deal.dealGemCost });
    }

    // Grant reward
    if (deal.type === "coins") {
        server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "CO", Amount: deal.amount });
    } else if (deal.type === "gems") {
        server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: deal.amount });
    } else if (deal.type === "heroTokens") {
        server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: deal.amount });
    } else if (deal.type === "bronzeChest") {
        var heroProgress = getPlayerData(playFabId, "HeroProgress") || { entries: [] };
        var result = rollChest(playFabId, "Bronze", heroProgress);
        setPlayerData(playFabId, "HeroProgress", heroProgress);
        if (result && result.dupTokens > 0) {
            server.AddUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "HT", Amount: result.dupTokens });
        }
    }

    // Record purchase
    purchased.bought.push(dealId);
    setPlayerData(playFabId, "DailyDealsPurchased", purchased);

    return { success: true, dealId: dealId };
};

// ─────────────── PurchaseBattlePass ─────────────────────

/**
 * Upgrades battle pass to premium. Server-defined cost.
 * Args: { season: int }
 */
handlers.PurchaseBattlePass = function (args, context) {
    var playFabId = currentPlayerId;
    var season = args.season || 1;

    // Validate season hasn't ended
    var now = new Date();
    var seasonEnd = new Date(BATTLE_PASS_SEASON_END);
    if (now >= seasonEnd) return { error: "Season has ended" };

    var bpData = getPlayerData(playFabId, "BattlePassData") || {
        season: season, tier: 0, xp: 0, premium: false, claimedFree: [], claimedPremium: []
    };

    if (bpData.premium) return { error: "Already premium" };

    var gems = getCurrencyBalance(playFabId, "GM");
    if (gems < BATTLE_PASS_PREMIUM_COST) return { error: "Not enough gems" };

    server.SubtractUserVirtualCurrency({ PlayFabId: playFabId, VirtualCurrency: "GM", Amount: BATTLE_PASS_PREMIUM_COST });

    bpData.premium = true;
    setPlayerData(playFabId, "BattlePassData", bpData);

    return { success: true, premium: true, bpData: bpData };
};

// ─────────────── GetBattlePassConfig ────────────────────

/**
 * Returns battle pass reward definitions and player progress.
 * Args: { season: int }
 */
handlers.GetBattlePassConfig = function (args, context) {
    var playFabId = currentPlayerId;
    var season = args.season || 1;

    var bpData = getPlayerData(playFabId, "BattlePassData");
    if (!bpData || bpData.season !== season) {
        // Initialize default data and persist it so ClaimBPReward can find it
        bpData = { season: season, tier: 0, xp: 0, premium: false, claimedFree: [], claimedPremium: [] };
        setPlayerData(playFabId, "BattlePassData", bpData);
    }

    return {
        rewards: BATTLE_PASS_REWARDS,
        playerData: bpData,
        premiumCost: BATTLE_PASS_PREMIUM_COST,
        maxTier: 70,
        xpPerTier: 1000,
        seasonEnd: BATTLE_PASS_SEASON_END,
        serverTimeUtc: new Date().toISOString()
    };
};

// ─────────────── GetPityCounters ────────────────────────

/**
 * Returns bronze chest cooldown info for UI display.
 * Args: (none)
 */
handlers.GetPityCounters = function (args, context) {
    var cooldownData = getPlayerData(currentPlayerId, "BronzeCooldown");
    var cooldownRemaining = 0;
    if (cooldownData && cooldownData.lastOpenedUtc) {
        var lastOpened = new Date(cooldownData.lastOpenedUtc);
        var now = new Date();
        var elapsed = (now.getTime() - lastOpened.getTime()) / 1000;
        cooldownRemaining = Math.max(0, Math.ceil(BRONZE_COOLDOWN_SECONDS - elapsed));
    }
    return { bronzeCooldownRemaining: cooldownRemaining };
};
