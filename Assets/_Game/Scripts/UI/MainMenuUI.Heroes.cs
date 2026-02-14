using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IOChef.Heroes;
using IOChef.Economy;

namespace IOChef.UI
{
    public partial class MainMenuUI
    {
        // ─── Heroes panel dynamic refs ───
        private TextMeshProUGUI heroesTokensLabel;
        private TextMeshProUGUI heroesGemsLabel;
        private RectTransform heroesScrollContent;

        // ─── Sort/Filter State ───
        private HeroRarity? _heroFilterRarity;
        private bool _heroSortByLevel;
        private RectTransform _heroFilterBar;

        // ─── Hero detail panel refs ───
        private GameObject heroDetailPanel;
        private Image heroDetailArt;
        private TextMeshProUGUI heroDetailName;
        private TextMeshProUGUI heroDetailRarity;
        private TextMeshProUGUI heroDetailAbilityName;
        private TextMeshProUGUI heroDetailAbilityDesc;
        private RectTransform heroDetailStatsContainer;
        private RectTransform heroDetailButtonArea;

        // ─── Rarity Colors ───
        private static readonly Color COL_RARITY_COMMON    = new(0.60f, 0.60f, 0.60f);
        private static readonly Color COL_RARITY_RARE      = new(0.20f, 0.50f, 0.90f);
        private static readonly Color COL_RARITY_EPIC      = new(0.65f, 0.25f, 0.85f);
        private static readonly Color COL_RARITY_LEGENDARY = new(1.00f, 0.84f, 0.22f);

        private static readonly Color COL_RARITY_COMMON_DARK    = new(0.35f, 0.35f, 0.35f);
        private static readonly Color COL_RARITY_RARE_DARK      = new(0.12f, 0.30f, 0.60f);
        private static readonly Color COL_RARITY_EPIC_DARK      = new(0.40f, 0.15f, 0.55f);
        private static readonly Color COL_RARITY_LEGENDARY_DARK = new(0.70f, 0.55f, 0.10f);

        private Color GetRarityColor(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => COL_RARITY_COMMON,
                HeroRarity.Rare => COL_RARITY_RARE,
                HeroRarity.Epic => COL_RARITY_EPIC,
                HeroRarity.Legendary => COL_RARITY_LEGENDARY,
                _ => COL_RARITY_COMMON,
            };
        }

        private Color GetRarityColorDark(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => COL_RARITY_COMMON_DARK,
                HeroRarity.Rare => COL_RARITY_RARE_DARK,
                HeroRarity.Epic => COL_RARITY_EPIC_DARK,
                HeroRarity.Legendary => COL_RARITY_LEGENDARY_DARK,
                _ => COL_RARITY_COMMON_DARK,
            };
        }

        private string GetRarityStars(HeroRarity rarity)
        {
            return rarity switch
            {
                HeroRarity.Common => "*",
                HeroRarity.Rare => "**",
                HeroRarity.Epic => "***",
                HeroRarity.Legendary => "****",
                _ => "*",
            };
        }

        private void AddRarityGlow(RectTransform card, HeroRarity rarity)
        {
            if (rarity != HeroRarity.Epic && rarity != HeroRarity.Legendary) return;

            Color glowColor;
            float speed, minA, maxA;

            if (rarity == HeroRarity.Legendary)
            {
                glowColor = new Color(1f, 0.84f, 0.22f, 0.15f);
                speed = 3f;
                minA = 0.05f;
                maxA = 0.30f;
            }
            else // Epic
            {
                glowColor = new Color(0.65f, 0.25f, 0.85f, 0.10f);
                speed = 2f;
                minA = 0.03f;
                maxA = 0.20f;
            }

            var glowGO = new GameObject("RarityGlow", typeof(RectTransform), typeof(Image));
            glowGO.transform.SetParent(card, false);
            var glowImg = glowGO.GetComponent<Image>();
            glowImg.color = glowColor;
            glowImg.raycastTarget = false;
            var glowRT = glowGO.GetComponent<RectTransform>();
            Stretch(glowRT);

            // Ignore layout so it doesn't affect the card's layout
            var glowLE = glowGO.AddComponent<LayoutElement>();
            glowLE.ignoreLayout = true;

            var pulse = glowGO.AddComponent<AlphaPulse>();
            pulse.speed = speed;
            pulse.minAlpha = minA;
            pulse.maxAlpha = maxA;
        }

        // ═══════════════════════════════════════════════════════
        //  BUILD HEROES PANEL
        // ═══════════════════════════════════════════════════════

        private GameObject BuildHeroesPanel(RectTransform p)
        {
            var panel = MakePanel(p, "HeroesPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "HeroBox", new Color(0.95f, 0.88f, 0.75f));
            box.anchorMin = new Vector2(0.03f, 0.02f);
            box.anchorMax = new Vector2(0.97f, 0.98f);
            box.offsetMin = box.offsetMax = Vector2.zero;

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 8;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.padding = new RectOffset(16, 16, 16, 16);

            // Title
            AddLayoutText(box, "HTitle", "HEROES", 38, COL_BTN_TEXT, FontStyles.Bold, 50);

            // Currency bar
            var currBar = MakePanel(box, "HCurrBar", new Color(0.22f, 0.13f, 0.04f, 0.12f));
            AddLE(currBar.gameObject, 42);
            var chl = currBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            chl.spacing = 30;
            chl.childAlignment = TextAnchor.MiddleCenter;
            chl.childForceExpandWidth = true;
            chl.childForceExpandHeight = false;
            chl.childControlWidth = true;
            chl.childControlHeight = true;
            chl.padding = new RectOffset(16, 16, 4, 4);

            heroesTokensLabel = MakeCurrencyLabel(currBar, "Tokens", "tokens", "0", 22, COL_BTN_TEXT, 36);
            heroesGemsLabel = MakeCurrencyLabel(currBar, "Gems", "gems", "0", 22, new Color(0.20f, 0.55f, 0.85f), 36);

            // Scrollable hero list
            var scrollGO = new GameObject("HScroll", typeof(RectTransform),
                typeof(ScrollRect), typeof(Image));
            scrollGO.transform.SetParent(box, false);
            var scrollLE = scrollGO.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1;
            scrollGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.05f);

            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport", typeof(RectTransform),
                typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGO.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>(); Stretch(vpRT);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            heroesScrollContent = content.GetComponent<RectTransform>();
            heroesScrollContent.anchorMin = new Vector2(0, 1);
            heroesScrollContent.anchorMax = new Vector2(1, 1);
            heroesScrollContent.pivot = new Vector2(0.5f, 1);
            heroesScrollContent.sizeDelta = new Vector2(0, 0);

            var cvl = content.AddComponent<VerticalLayoutGroup>();
            cvl.spacing = 10;
            cvl.childAlignment = TextAnchor.UpperCenter;
            cvl.childForceExpandWidth = true;
            cvl.childForceExpandHeight = false;
            cvl.childControlWidth = true;
            cvl.childControlHeight = true;
            cvl.padding = new RectOffset(8, 8, 8, 8);

            content.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRT;
            scrollRect.content = heroesScrollContent;

            // Close
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () => panel.gameObject.SetActive(false));

            // Build hero detail overlay (hidden)
            heroDetailPanel = BuildHeroDetailPanel(p);

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        // ═══════════════════════════════════════════════════════
        //  HERO DETAIL PANEL
        // ═══════════════════════════════════════════════════════

        private GameObject BuildHeroDetailPanel(RectTransform p)
        {
            var panel = MakePanel(p, "HeroDetailPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "DetailBox", new Color(0.95f, 0.88f, 0.75f));
            box.anchorMin = new Vector2(0.05f, 0.03f);
            box.anchorMax = new Vector2(0.95f, 0.97f);
            box.offsetMin = box.offsetMax = Vector2.zero;

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 10;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.padding = new RectOffset(24, 24, 24, 24);

            // Hero art area
            var artHolder = MakePanel(box, "ArtHolder", new Color(0.18f, 0.12f, 0.08f));
            AddLE(artHolder.gameObject, 380);
            var artGO = new GameObject("HeroArt", typeof(RectTransform), typeof(Image));
            artGO.transform.SetParent(artHolder, false);
            heroDetailArt = artGO.GetComponent<Image>();
            heroDetailArt.preserveAspect = true;
            heroDetailArt.color = Color.white;
            var artRT = artGO.GetComponent<RectTransform>(); Stretch(artRT);
            artRT.offsetMin = new Vector2(8, 8);
            artRT.offsetMax = new Vector2(-8, -8);

            // Name + rarity
            var nameRT = MakeText(box, "DetailName", "Hero Name", 34,
                COL_BTN_TEXT, FontStyles.Bold);
            AddLE(nameRT.gameObject, 44);
            heroDetailName = nameRT.GetComponent<TextMeshProUGUI>();

            var rarRT = MakeText(box, "DetailRarity", "Common *", 22,
                COL_RARITY_COMMON, FontStyles.Bold);
            AddLE(rarRT.gameObject, 30);
            heroDetailRarity = rarRT.GetComponent<TextMeshProUGUI>();

            // Ability
            var abilNameRT = MakeText(box, "AbilName", "Ability Name", 24,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Bold);
            AddLE(abilNameRT.gameObject, 32);
            heroDetailAbilityName = abilNameRT.GetComponent<TextMeshProUGUI>();

            var abilDescRT = MakeText(box, "AbilDesc", "Ability description", 18,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic);
            AddLE(abilDescRT.gameObject, 56);
            heroDetailAbilityDesc = abilDescRT.GetComponent<TextMeshProUGUI>();
            heroDetailAbilityDesc.textWrappingMode = TextWrappingModes.Normal;

            // Stats container
            AddLayoutText(box, "StatsH", "STATS", 22, COL_BTN_TEXT, FontStyles.Bold, 30);

            var statsScroll = new GameObject("StatsArea", typeof(RectTransform));
            statsScroll.transform.SetParent(box, false);
            AddLE(statsScroll, 250);
            heroDetailStatsContainer = statsScroll.GetComponent<RectTransform>();
            var statsVL = statsScroll.AddComponent<VerticalLayoutGroup>();
            statsVL.spacing = 6;
            statsVL.childAlignment = TextAnchor.UpperCenter;
            statsVL.childForceExpandWidth = true;
            statsVL.childForceExpandHeight = false;
            statsVL.childControlWidth = true;
            statsVL.childControlHeight = true;
            statsVL.padding = new RectOffset(4, 4, 4, 4);

            // Button area (dynamic)
            var btnArea = MakePanel(box, "DetailButtons", Color.clear);
            AddLE(btnArea.gameObject, 130);
            heroDetailButtonArea = btnArea;
            var btnVL = btnArea.gameObject.AddComponent<VerticalLayoutGroup>();
            btnVL.spacing = 8;
            btnVL.childAlignment = TextAnchor.MiddleCenter;
            btnVL.childForceExpandWidth = true;
            btnVL.childForceExpandHeight = false;
            btnVL.childControlHeight = true;
            btnVL.childControlWidth = true;

            // Close
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 24, 54,
                () => heroDetailPanel.SetActive(false));

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private void ShowHeroDetail(HeroDataSO hero)
        {
            if (heroDetailPanel == null || hero == null) return;

            bool unlocked = HeroManager.Instance != null && HeroManager.Instance.IsHeroUnlocked(hero.heroId);
            int level = HeroManager.Instance?.GetHeroLevel(hero.heroId) ?? 1;
            bool isSelected = HeroManager.Instance?.SelectedHero == hero;
            int upgradeCost = HeroManager.Instance?.GetUpgradeCost(hero.heroId) ?? 0;

            // Art
            if (heroDetailArt != null)
            {
                heroDetailArt.sprite = hero.heroArt;
                heroDetailArt.color = unlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f);
            }

            // Name
            if (heroDetailName != null)
            {
                string heroName = !string.IsNullOrEmpty(hero.heroName) ? hero.heroName : hero.heroId;
                heroDetailName.text = unlocked ? heroName : $"{heroName} (LOCKED)";
                heroDetailName.color = unlocked ? COL_BTN_TEXT : COL_DISABLED;
                heroDetailName.overflowMode = TextOverflowModes.Ellipsis;
                heroDetailName.textWrappingMode = TextWrappingModes.NoWrap;
            }

            // Rarity
            if (heroDetailRarity != null)
            {
                heroDetailRarity.text = $"{hero.rarity} {GetRarityStars(hero.rarity)}";
                heroDetailRarity.color = GetRarityColor(hero.rarity);
            }

            // Ability
            if (heroDetailAbilityName != null)
                heroDetailAbilityName.text = !string.IsNullOrEmpty(hero.abilityName) ? hero.abilityName : "—";
            if (heroDetailAbilityDesc != null)
                heroDetailAbilityDesc.text = !string.IsNullOrEmpty(hero.abilityDescription)
                    ? hero.abilityDescription : "No ability description.";

            // Stats
            PopulateHeroStats(hero, level, unlocked);

            // Buttons
            PopulateHeroDetailButtons(hero, unlocked, isSelected, upgradeCost);

            heroDetailPanel.SetActive(true);
        }

        private void PopulateHeroStats(HeroDataSO hero, int level, bool unlocked)
        {
            if (heroDetailStatsContainer == null) return;
            for (int i = heroDetailStatsContainer.childCount - 1; i >= 0; i--)
                Destroy(heroDetailStatsContainer.GetChild(i).gameObject);

            if (!unlocked)
            {
                AddLayoutText(heroDetailStatsContainer, "Locked", "Unlock to view stats", 18,
                    COL_DISABLED, FontStyles.Italic, 32);
                return;
            }

            var mods = hero.ToModifiers(level);
            var maxMods = hero.ToModifiers(hero.maxLevel);

            AddStatBar("Cook Speed", 1f / mods.cookTimeMultiplier, 1f / maxMods.cookTimeMultiplier);
            AddStatBar("Burn Resist", mods.burnTimeMultiplier, maxMods.burnTimeMultiplier);
            AddStatBar("Score Bonus", mods.scoreMultiplier, maxMods.scoreMultiplier);
            AddStatBar("Move Speed", mods.movementSpeedMultiplier, maxMods.movementSpeedMultiplier);
            AddStatBar("Interact Range", mods.interactionRadiusMultiplier, maxMods.interactionRadiusMultiplier);
            AddStatBar("Bonus Time", mods.bonusTimeSeconds, hero.maxBonusTimeSeconds);
        }

        private void AddStatBar(string label, float current, float max)
        {
            if (heroDetailStatsContainer == null) return;

            var row = MakePanel(heroDetailStatsContainer, $"Stat_{label}", Color.clear);
            AddLE(row.gameObject, 34);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 8;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.padding = new RectOffset(4, 4, 2, 2);

            // Label
            var lblRT = MakeText(row, "Lbl", label, 16, COL_BTN_TEXT, FontStyles.Normal);
            var lblLE = lblRT.gameObject.AddComponent<LayoutElement>();
            lblLE.preferredWidth = 140;
            lblLE.preferredHeight = 28;
            lblRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Bar background
            var barBg = MakePanel(row, "BarBg", new Color(0.3f, 0.25f, 0.2f, 0.4f));
            var barBgLE = barBg.gameObject.AddComponent<LayoutElement>();
            barBgLE.preferredWidth = 320;
            barBgLE.preferredHeight = 22;
            barBgLE.flexibleWidth = 1;

            float fillPct = max > 0 ? Mathf.Clamp01(current / max) : 0;
            var fill = MakePanel(barBg, "Fill", COL_PLAY);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(fillPct, 1);
            fill.offsetMin = fill.offsetMax = Vector2.zero;

            // Delta text
            float remainPct = (max - current);
            string deltaStr = remainPct > 0.01f ? $"+{remainPct:F2}" : "MAX";
            Color deltaCol = remainPct > 0.01f ? new Color(0.2f, 0.7f, 0.2f) : COL_RARITY_LEGENDARY;
            var deltaRT = MakeText(row, "Delta", deltaStr, 14, deltaCol, FontStyles.Bold);
            var deltaLE = deltaRT.gameObject.AddComponent<LayoutElement>();
            deltaLE.preferredWidth = 60;
            deltaLE.preferredHeight = 28;
        }

        private void PopulateHeroDetailButtons(HeroDataSO hero, bool unlocked, bool isSelected, int upgradeCost)
        {
            if (heroDetailButtonArea == null) return;
            for (int i = heroDetailButtonArea.childCount - 1; i >= 0; i--)
                Destroy(heroDetailButtonArea.GetChild(i).gameObject);

            if (!unlocked)
            {
                AddLayoutText(heroDetailButtonArea, "LockedMsg",
                    "Open chests to unlock this hero!", 20, COL_DISABLED, FontStyles.Italic, 50);
                return;
            }

            if (!isSelected)
            {
                string capturedId = hero.heroId;
                MakeChunkyButton(heroDetailButtonArea, "SELECT HERO", COL_PLAY, COL_PLAY_SHADOW,
                    Color.white, 22, 58,
                    () =>
                    {
                        HeroManager.Instance?.SelectHero(capturedId);
                        heroDetailPanel.SetActive(false);
                        RefreshHeroesPanel();
                    });
            }
            else
            {
                AddLayoutText(heroDetailButtonArea, "SelectedMsg",
                    "CURRENTLY SELECTED", 20, COL_PLAY, FontStyles.Bold, 40);
            }

            if (upgradeCost > 0)
            {
                string capturedId = hero.heroId;
                MakeChunkyButton(heroDetailButtonArea, $"UPGRADE ({upgradeCost} TOKENS)",
                    COL_BTN, COL_BTN_SHADOW, COL_BTN_TEXT, 20, 54,
                    () =>
                    {
                        HeroManager.Instance?.UpgradeHero(capturedId, success =>
                        {
                            if (success)
                            {
                                ShowHeroDetail(hero);
                                RefreshHeroesPanel();
                            }
                        });
                    });
            }
            else
            {
                AddLayoutText(heroDetailButtonArea, "MaxLvMsg",
                    "MAX LEVEL", 18, COL_RARITY_LEGENDARY, FontStyles.Bold, 32);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH HEROES
        // ═══════════════════════════════════════════════════════

        private void RefreshHeroesPanel()
        {
            if (CurrencyManager.Instance != null)
            {
                if (heroesTokensLabel != null)
                    heroesTokensLabel.text = $"{CurrencyManager.Instance.HeroTokens}";
                if (heroesGemsLabel != null)
                    heroesGemsLabel.text = $"{CurrencyManager.Instance.Gems}";
            }

            if (heroesScrollContent == null) return;

            for (int i = heroesScrollContent.childCount - 1; i >= 0; i--)
                Destroy(heroesScrollContent.GetChild(i).gameObject);

            if (HeroManager.Instance == null)
            {
                AddLayoutText(heroesScrollContent, "NoHeroes",
                    "No heroes available yet!", 20, COL_BTN_TEXT, FontStyles.Italic, 50);
                return;
            }

            var allHeroes = HeroManager.Instance.GetAllHeroes();
            if (allHeroes.Count == 0)
            {
                AddLayoutText(heroesScrollContent, "NoHeroes",
                    "No heroes available yet!", 20, COL_BTN_TEXT, FontStyles.Italic, 50);
                return;
            }

            // Filter bar
            BuildHeroFilterBar();

            // Selected hero spotlight at top
            var selected = HeroManager.Instance.SelectedHero;
            if (selected != null)
            {
                BuildHeroSpotlight(selected);
            }

            // Filter
            var filtered = new System.Collections.Generic.List<HeroDataSO>();
            foreach (var hero in allHeroes)
            {
                if (_heroFilterRarity.HasValue && hero.rarity != _heroFilterRarity.Value) continue;
                filtered.Add(hero);
            }

            // Sort
            if (_heroSortByLevel)
            {
                filtered.Sort((a, b) =>
                {
                    int lvA = HeroManager.Instance?.GetHeroLevel(a.heroId) ?? 0;
                    int lvB = HeroManager.Instance?.GetHeroLevel(b.heroId) ?? 0;
                    return lvB.CompareTo(lvA); // descending
                });
            }
            else
            {
                filtered.Sort((a, b) =>
                {
                    int cmp = ((int)b.rarity).CompareTo((int)a.rarity); // higher rarity first
                    if (cmp != 0) return cmp;
                    return string.Compare(a.heroName, b.heroName, System.StringComparison.Ordinal);
                });
            }

            foreach (var hero in filtered)
            {
                BuildHeroCard(hero);
            }
        }

        private void BuildHeroFilterBar()
        {
            if (heroesScrollContent == null) return;

            var filterRow = MakePanel(heroesScrollContent, "FilterBar", new Color(0.22f, 0.13f, 0.04f, 0.12f));
            AddLE(filterRow.gameObject, 42);
            filterRow.transform.SetAsFirstSibling();
            _heroFilterBar = filterRow;

            var hl = filterRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 6;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.padding = new RectOffset(8, 8, 4, 4);

            // Filter buttons
            BuildFilterButton(filterRow, "ALL", null);
            BuildFilterButton(filterRow, "C", HeroRarity.Common);
            BuildFilterButton(filterRow, "R", HeroRarity.Rare);
            BuildFilterButton(filterRow, "E", HeroRarity.Epic);
            BuildFilterButton(filterRow, "L", HeroRarity.Legendary);

            // Separator
            var sep = MakePanel(filterRow, "Sep", new Color(0, 0, 0, 0.15f));
            var sepLE = sep.gameObject.AddComponent<LayoutElement>();
            sepLE.preferredWidth = 2;
            sepLE.preferredHeight = 30;

            // Sort toggle
            string sortLabel = _heroSortByLevel ? "BY LEVEL" : "BY RARITY";
            Color sortColor = new Color(0.55f, 0.28f, 0.85f);
            Color sortSh = new Color(0.35f, 0.15f, 0.60f);
            MakeChunkyButton(filterRow, sortLabel, sortColor, sortSh, Color.white, 12, 34,
                () =>
                {
                    _heroSortByLevel = !_heroSortByLevel;
                    RefreshHeroesPanel();
                });
        }

        private void BuildFilterButton(RectTransform parent, string label, HeroRarity? rarity)
        {
            bool isActive = _heroFilterRarity == rarity;
            Color bg = isActive ? COL_PLAY : new Color(0.5f, 0.5f, 0.5f, 0.4f);
            Color sh = isActive ? COL_PLAY_SHADOW : new Color(0.35f, 0.35f, 0.35f);
            Color txt = isActive ? Color.white : new Color(0.85f, 0.83f, 0.80f);

            HeroRarity? capturedRarity = rarity;
            MakeChunkyButton(parent, label, bg, sh, txt, 14, 34,
                () =>
                {
                    _heroFilterRarity = capturedRarity;
                    RefreshHeroesPanel();
                });
        }

        private void BuildHeroSpotlight(HeroDataSO hero)
        {
            int level = HeroManager.Instance?.GetHeroLevel(hero.heroId) ?? 1;
            Color rarColor = GetRarityColor(hero.rarity);

            var spotlight = MakePanel(heroesScrollContent, "Spotlight", new Color(0.28f, 0.22f, 0.15f, 0.7f));
            AddLE(spotlight.gameObject, 160);
            var shl = spotlight.gameObject.AddComponent<HorizontalLayoutGroup>();
            shl.spacing = 12;
            shl.childAlignment = TextAnchor.MiddleCenter;
            shl.childForceExpandWidth = false;
            shl.childForceExpandHeight = false;
            shl.childControlWidth = true;
            shl.childControlHeight = true;
            shl.padding = new RectOffset(12, 12, 8, 8);

            // Portrait
            var portraitHolder = MakePanel(spotlight, "Portrait", new Color(0, 0, 0, 0.3f));
            var portraitLE = portraitHolder.gameObject.AddComponent<LayoutElement>();
            portraitLE.preferredWidth = 100;
            portraitLE.preferredHeight = 100;
            if (hero.heroPortrait != null)
            {
                var pImg = new GameObject("Img", typeof(RectTransform), typeof(Image));
                pImg.transform.SetParent(portraitHolder, false);
                var img = pImg.GetComponent<Image>();
                img.sprite = hero.heroPortrait;
                img.preserveAspect = true;
                img.raycastTarget = false;
                Stretch(pImg.GetComponent<RectTransform>());
            }

            // Info
            var infoGO = new GameObject("Info", typeof(RectTransform));
            infoGO.transform.SetParent(spotlight, false);
            var infoLE = infoGO.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;
            infoLE.preferredHeight = 140;

            var infoVL = infoGO.AddComponent<VerticalLayoutGroup>();
            infoVL.spacing = 4;
            infoVL.childAlignment = TextAnchor.MiddleLeft;
            infoVL.childForceExpandWidth = true;
            infoVL.childForceExpandHeight = false;
            infoVL.childControlWidth = true;
            infoVL.childControlHeight = true;

            var infoRT = infoGO.GetComponent<RectTransform>();
            AddLayoutText(infoRT, "Active", "ACTIVE HERO", 14, COL_PLAY, FontStyles.Bold, 20);

            string heroName = !string.IsNullOrEmpty(hero.heroName) ? hero.heroName : hero.heroId;
            var nameRT = MakeText(infoRT, "SName", heroName, 28, Color.white, FontStyles.Bold);
            AddLE(nameRT.gameObject, 34);
            var sNameTMP = nameRT.GetComponent<TextMeshProUGUI>();
            sNameTMP.alignment = TextAlignmentOptions.Left;
            sNameTMP.overflowMode = TextOverflowModes.Ellipsis;
            sNameTMP.textWrappingMode = TextWrappingModes.NoWrap;

            var rarStr = $"{hero.rarity} {GetRarityStars(hero.rarity)} | Lv.{level}";
            var rarTextRT = MakeText(infoRT, "SRar", rarStr, 18, rarColor, FontStyles.Bold);
            AddLE(rarTextRT.gameObject, 24);
            rarTextRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            if (!string.IsNullOrEmpty(hero.abilityName))
            {
                var abRT = MakeText(infoRT, "SAbil", hero.abilityName, 16,
                    new Color(1, 1, 1, 0.7f), FontStyles.Italic);
                AddLE(abRT.gameObject, 22);
                abRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            }
        }

        private void BuildHeroCard(HeroDataSO hero)
        {
            bool unlocked = HeroManager.Instance != null && HeroManager.Instance.IsHeroUnlocked(hero.heroId);
            int level = HeroManager.Instance?.GetHeroLevel(hero.heroId) ?? 1;
            bool isSelected = HeroManager.Instance?.SelectedHero == hero;
            int upgradeCost = HeroManager.Instance?.GetUpgradeCost(hero.heroId) ?? 0;
            Color rarColor = GetRarityColor(hero.rarity);
            Color rarColorDark = GetRarityColorDark(hero.rarity);

            Color cardBg = unlocked
                ? (isSelected ? new Color(0.85f, 0.95f, 0.85f) : new Color(1, 1, 1, 0.45f))
                : new Color(0.55f, 0.52f, 0.48f, 0.5f);

            var card = MakePanel(heroesScrollContent, $"Hero_{hero.heroId}", cardBg);
            AddLE(card.gameObject, 130);

            // Main layout
            var cardHL = card.gameObject.AddComponent<HorizontalLayoutGroup>();
            cardHL.spacing = 0;
            cardHL.childAlignment = TextAnchor.MiddleCenter;
            cardHL.childForceExpandWidth = false;
            cardHL.childForceExpandHeight = false;
            cardHL.childControlWidth = true;
            cardHL.childControlHeight = true;
            cardHL.padding = new RectOffset(0, 8, 4, 4);

            // Rarity border strip (left)
            var borderStrip = MakePanel(card, "RarityBorder", rarColor);
            var borderLE = borderStrip.gameObject.AddComponent<LayoutElement>();
            borderLE.preferredWidth = 8;
            borderLE.preferredHeight = 120;

            // Portrait
            var portraitBg = MakePanel(card, "PortBg", new Color(0, 0, 0, 0.2f));
            var portLE = portraitBg.gameObject.AddComponent<LayoutElement>();
            portLE.preferredWidth = 80;
            portLE.preferredHeight = 80;
            if (hero.heroPortrait != null)
            {
                var pImg = new GameObject("Img", typeof(RectTransform), typeof(Image));
                pImg.transform.SetParent(portraitBg, false);
                var img = pImg.GetComponent<Image>();
                img.sprite = hero.heroPortrait;
                img.preserveAspect = true;
                img.color = unlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f);
                img.raycastTarget = false;
                Stretch(pImg.GetComponent<RectTransform>());
            }

            // Info column
            var infoGO = new GameObject("Info", typeof(RectTransform));
            infoGO.transform.SetParent(card, false);
            var infoLE = infoGO.AddComponent<LayoutElement>();
            infoLE.preferredWidth = 280;
            infoLE.flexibleWidth = 1;
            infoLE.preferredHeight = 110;

            var infoVL = infoGO.AddComponent<VerticalLayoutGroup>();
            infoVL.spacing = 3;
            infoVL.childAlignment = TextAnchor.MiddleLeft;
            infoVL.childForceExpandWidth = true;
            infoVL.childForceExpandHeight = false;
            infoVL.childControlWidth = true;
            infoVL.childControlHeight = true;
            infoVL.padding = new RectOffset(8, 4, 4, 4);

            var infoRT = infoGO.GetComponent<RectTransform>();

            // Name (rarity-colored)
            string heroName = !string.IsNullOrEmpty(hero.heroName) ? hero.heroName : hero.heroId;
            string nameStr = unlocked ? heroName : $"{heroName} (LOCKED)";
            Color nameCol = unlocked ? rarColor : COL_DISABLED;
            var nameRT = MakeText(infoRT, "Name", nameStr, 20, nameCol, FontStyles.Bold);
            AddLE(nameRT.gameObject, 26);
            var nameTMP = nameRT.GetComponent<TextMeshProUGUI>();
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;
            nameTMP.textWrappingMode = TextWrappingModes.NoWrap;

            // Rarity + stars
            var rarLbl = $"{hero.rarity} {GetRarityStars(hero.rarity)}";
            var rarLblRT = MakeText(infoRT, "Rar", rarLbl, 14, rarColor, FontStyles.Normal);
            AddLE(rarLblRT.gameObject, 20);
            rarLblRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Level bar (mini progress bar)
            if (unlocked)
            {
                var lvlRow = MakePanel(infoRT, "LvlRow", Color.clear);
                AddLE(lvlRow.gameObject, 22);
                var lvlHL = lvlRow.gameObject.AddComponent<HorizontalLayoutGroup>();
                lvlHL.spacing = 6;
                lvlHL.childAlignment = TextAnchor.MiddleLeft;
                lvlHL.childForceExpandWidth = false;
                lvlHL.childForceExpandHeight = false;
                lvlHL.childControlWidth = true;
                lvlHL.childControlHeight = true;

                var lvlTxt = MakeText(lvlRow, "Lv", $"Lv.{level}", 14, COL_BTN_TEXT, FontStyles.Bold);
                var lvlTxtLE = lvlTxt.gameObject.AddComponent<LayoutElement>();
                lvlTxtLE.preferredWidth = 50;
                lvlTxtLE.preferredHeight = 18;
                lvlTxt.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

                var barBg = MakePanel(lvlRow, "BarBg", new Color(0.3f, 0.25f, 0.2f, 0.4f));
                var barBgLE = barBg.gameObject.AddComponent<LayoutElement>();
                barBgLE.preferredWidth = 140;
                barBgLE.preferredHeight = 12;
                barBgLE.flexibleWidth = 1;

                float pct = hero.maxLevel > 1 ? (float)(level - 1) / (hero.maxLevel - 1) : 1f;
                var fill = MakePanel(barBg, "Fill", rarColor);
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = new Vector2(Mathf.Clamp01(pct), 1);
                fill.offsetMin = fill.offsetMax = Vector2.zero;
            }

            // Detail line
            string detailStr = unlocked
                ? (isSelected ? "SELECTED" : "Tap for details")
                : "Open chests to unlock";
            Color detailCol = isSelected ? COL_PLAY : new Color(0.42f, 0.32f, 0.18f);
            var detRT = MakeText(infoRT, "Detail", detailStr, 14, detailCol, FontStyles.Italic);
            AddLE(detRT.gameObject, 20);
            detRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            // Action buttons
            var btnGO = new GameObject("Btns", typeof(RectTransform));
            btnGO.transform.SetParent(card, false);
            var btnLE = btnGO.AddComponent<LayoutElement>();
            btnLE.preferredWidth = 110;
            btnLE.preferredHeight = 110;

            var btnVL = btnGO.AddComponent<VerticalLayoutGroup>();
            btnVL.spacing = 4;
            btnVL.childAlignment = TextAnchor.MiddleCenter;
            btnVL.childForceExpandWidth = true;
            btnVL.childForceExpandHeight = false;
            btnVL.childControlWidth = true;
            btnVL.childControlHeight = true;
            btnVL.padding = new RectOffset(2, 2, 2, 2);

            var btnRT = btnGO.GetComponent<RectTransform>();

            if (unlocked)
            {
                if (!isSelected)
                {
                    string capturedId = hero.heroId;
                    MakeChunkyButton(btnRT, "SELECT", COL_PLAY, COL_PLAY_SHADOW,
                        Color.white, 14, 38,
                        () =>
                        {
                            HeroManager.Instance?.SelectHero(capturedId);
                            RefreshHeroesPanel();
                        });
                }

                if (upgradeCost > 0)
                {
                    string capturedId = hero.heroId;
                    MakeChunkyButton(btnRT, $"{upgradeCost}T UP", COL_BTN,
                        COL_BTN_SHADOW, COL_BTN_TEXT, 12, 36,
                        () =>
                        {
                            HeroManager.Instance?.UpgradeHero(capturedId, success =>
                            {
                                if (success) RefreshHeroesPanel();
                            });
                        });
                }
            }

            // Rarity glow for Epic/Legendary
            if (unlocked)
                AddRarityGlow(card, hero.rarity);

            // Make the card tappable to open detail
            var cardBtn = card.gameObject.AddComponent<Button>();
            cardBtn.transition = Selectable.Transition.None;
            var capturedHero = hero;
            cardBtn.onClick.AddListener(() => ShowHeroDetail(capturedHero));
        }
    }
}
