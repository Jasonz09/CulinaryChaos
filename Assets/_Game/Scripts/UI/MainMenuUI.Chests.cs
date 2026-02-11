using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using IOChef.Economy;
using IOChef.Heroes;

namespace IOChef.UI
{
    public partial class MainMenuUI
    {
        // ─── Chest panel dynamic refs ───
        private RectTransform chestScrollContent;
        private TextMeshProUGUI chestCoinsLabel;
        private TextMeshProUGUI chestGemsLabel;
        private TextMeshProUGUI chestBronzeCooldownLabel;

        // ─── Chest animation overlay ───
        private GameObject chestAnimOverlay;
        private Image chestAnimGlow;
        private RectTransform chestAnimChestIcon;
        private Image chestAnimHeroCard;
        private TextMeshProUGUI chestAnimHeroName;
        private TextMeshProUGUI chestAnimBadge;
        private TextMeshProUGUI chestAnimRarityLabel;
        private Image chestAnimHeroPortrait;
        private bool _chestAnimSkipRequested;
        private HeroDataSO _featuredHero;

        // ─── Multi-pull results ───
        private GameObject chestMultiOverlay;
        private RectTransform chestMultiGrid;
        private TextMeshProUGUI chestMultiSummary;

        // ═══════════════════════════════════════════════════════
        //  BUILD CHEST PANEL
        // ═══════════════════════════════════════════════════════

        private GameObject BuildChestPanel(RectTransform p)
        {
            var panel = MakePanel(p, "ChestPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "ChestBox", new Color(0.95f, 0.88f, 0.75f));
            box.anchorMin = new Vector2(0.03f, 0.02f);
            box.anchorMax = new Vector2(0.97f, 0.98f);
            box.offsetMin = box.offsetMax = Vector2.zero;

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 12;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.padding = new RectOffset(16, 16, 20, 16);

            AddLayoutText(box, "ChTitle", "HERO CHESTS", 38, COL_BTN_TEXT, FontStyles.Bold, 50);
            AddLayoutText(box, "ChSub", "Open chests to discover new heroes!", 18,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic, 28);

            // Currency bar
            var currBar = MakePanel(box, "ChCurrBar", new Color(0.22f, 0.13f, 0.04f, 0.12f));
            AddLE(currBar.gameObject, 42);
            var currHL = currBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            currHL.spacing = 30;
            currHL.childAlignment = TextAnchor.MiddleCenter;
            currHL.childForceExpandWidth = true;
            currHL.childForceExpandHeight = false;
            currHL.childControlWidth = true;
            currHL.childControlHeight = true;
            currHL.padding = new RectOffset(16, 16, 4, 4);

            chestCoinsLabel = MakeCurrencyLabel(currBar, "Coins", "coins", "0", 22, COL_BTN_TEXT, 36);
            chestGemsLabel = MakeCurrencyLabel(currBar, "Gems", "gems", "0", 22, new Color(0.20f, 0.55f, 0.85f), 36);

            // Scrollable chest tier cards
            var scrollGO = new GameObject("ChScroll", typeof(RectTransform),
                typeof(ScrollRect), typeof(Image));
            scrollGO.transform.SetParent(box, false);
            var scrollLE = scrollGO.AddComponent<LayoutElement>();
            scrollLE.flexibleHeight = 1;
            scrollGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.05f);

            var scrollRect = scrollGO.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGO.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>(); Stretch(vpRT);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            chestScrollContent = content.GetComponent<RectTransform>();
            chestScrollContent.anchorMin = new Vector2(0, 1);
            chestScrollContent.anchorMax = new Vector2(1, 1);
            chestScrollContent.pivot = new Vector2(0.5f, 1);
            chestScrollContent.sizeDelta = new Vector2(0, 0);

            var cvl = content.AddComponent<VerticalLayoutGroup>();
            cvl.spacing = 16;
            cvl.childAlignment = TextAnchor.UpperCenter;
            cvl.childForceExpandWidth = true;
            cvl.childForceExpandHeight = false;
            cvl.childControlWidth = true;
            cvl.childControlHeight = true;
            cvl.padding = new RectOffset(8, 8, 12, 12);

            content.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRT;
            scrollRect.content = chestScrollContent;

            // Close
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () => panel.gameObject.SetActive(false));

            // Build animation overlay (hidden)
            chestAnimOverlay = BuildChestAnimOverlay(p);
            chestMultiOverlay = BuildChestMultiOverlay(p);

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private void BuildChestTierCard(ChestRarity rarity, string name, int cost,
            Color cardColor, Color cardDarkColor, string dropRates)
        {
            var card = MakePanel(chestScrollContent, $"Chest_{rarity}", cardColor);
            AddLE(card.gameObject, 260);

            var cardVL = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardVL.spacing = 8;
            cardVL.childAlignment = TextAnchor.UpperCenter;
            cardVL.childForceExpandWidth = true;
            cardVL.childForceExpandHeight = false;
            cardVL.childControlWidth = true;
            cardVL.childControlHeight = true;
            cardVL.padding = new RectOffset(16, 16, 14, 14);

            // Chest name
            AddLayoutText(card, "Name", name, 30, Color.white, FontStyles.Bold, 38);

            // Price / Cooldown
            if (rarity == ChestRarity.Bronze)
            {
                bool onCooldown = ChestManager.Instance != null && ChestManager.Instance.IsBronzeOnCooldown;
                string priceStr = onCooldown ? "ON COOLDOWN" : "FREE (1 per day)";
                Color priceCol = onCooldown ? new Color(0.85f, 0.55f, 0.2f) : new Color(0.3f, 0.85f, 0.3f);
                AddLayoutText(card, "Price", priceStr, 22, priceCol, FontStyles.Bold, 30);

                // Cooldown timer
                if (onCooldown)
                {
                    float remaining = ChestManager.Instance.BronzeCooldownRemaining;
                    int hours = (int)(remaining / 3600f);
                    int minutes = (int)((remaining % 3600f) / 60f);
                    int seconds = (int)(remaining % 60f);
                    string timeStr = $"Available in {hours:D2}:{minutes:D2}:{seconds:D2}";

                    var cooldownGO = new GameObject("Cooldown", typeof(RectTransform), typeof(TextMeshProUGUI));
                    cooldownGO.transform.SetParent(card, false);
                    AddLE(cooldownGO, 24);
                    chestBronzeCooldownLabel = cooldownGO.GetComponent<TextMeshProUGUI>();
                    chestBronzeCooldownLabel.text = timeStr;
                    chestBronzeCooldownLabel.fontSize = 16;
                    chestBronzeCooldownLabel.color = new Color(1, 1, 1, 0.7f);
                    chestBronzeCooldownLabel.fontStyle = FontStyles.Italic;
                    chestBronzeCooldownLabel.alignment = TextAlignmentOptions.Center;
                }
            }
            else
            {
                string priceStr = $"{cost} GEMS";
                AddLayoutText(card, "Price", priceStr, 22, Color.white, FontStyles.Bold, 30);
            }

            // Drop rates
            AddLayoutText(card, "Rates", dropRates, 15,
                new Color(1, 1, 1, 0.8f), FontStyles.Normal, 56);

            // Button row
            var btnRow = MakePanel(card, "BtnRow", Color.clear);
            AddLE(btnRow.gameObject, 60);
            var btnHL = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnHL.spacing = 14;
            btnHL.childAlignment = TextAnchor.MiddleCenter;
            btnHL.childForceExpandWidth = true;
            btnHL.childForceExpandHeight = false;
            btnHL.childControlWidth = true;
            btnHL.childControlHeight = true;

            if (rarity == ChestRarity.Bronze)
            {
                // Bronze: only x1, no x10 (server blocks multi-pull for bronze)
                bool canOpen = ChestManager.Instance == null || !ChestManager.Instance.IsBronzeOnCooldown;
                string x1Label = canOpen ? "OPEN FREE CHEST" : "ON COOLDOWN";
                Color btnColor = canOpen ? cardDarkColor : COL_DISABLED;
                Color btnShadow = canOpen ? cardDarkColor * 0.7f : COL_DISABLED_SH;
                MakeChunkyButton(btnRow, x1Label, btnColor, btnShadow,
                    canOpen ? Color.white : COL_DISABLED_TXT, 18, 52,
                    canOpen ? () => OnOpenChest(rarity, 1) : null);
                if (!canOpen)
                {
                    var cooldownBtn = btnRow.GetChild(btnRow.childCount - 1).GetComponent<Button>();
                    if (cooldownBtn != null) cooldownBtn.interactable = false;
                }
            }
            else
            {
                // Silver/Gold: x1 and x10 buttons
                string x1Label = $"OPEN x1 ({cost}G)";
                MakeChunkyButton(btnRow, x1Label, cardDarkColor, cardDarkColor * 0.7f,
                    Color.white, 18, 52,
                    () => OnOpenChest(rarity, 1));

                int x10Cost = cost * 10;
                string x10Label = $"OPEN x10 ({x10Cost}G)";
                MakeChunkyButton(btnRow, x10Label, cardDarkColor, cardDarkColor * 0.7f,
                    Color.white, 18, 52,
                    () => OnOpenChest(rarity, 10));
            }
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH CHESTS
        // ═══════════════════════════════════════════════════════

        private void RefreshChestPanel()
        {
            // Update currency display
            if (CurrencyManager.Instance != null)
            {
                if (chestCoinsLabel != null)
                    chestCoinsLabel.text = $"{CurrencyManager.Instance.Coins}";
                if (chestGemsLabel != null)
                    chestGemsLabel.text = $"{CurrencyManager.Instance.Gems}";
            }

            if (chestScrollContent == null) return;

            for (int i = chestScrollContent.childCount - 1; i >= 0; i--)
                Destroy(chestScrollContent.GetChild(i).gameObject);

            ChestManager.Instance?.SyncCooldown();

            BuildFeaturedHeroSection();

            BuildChestTierCard(ChestRarity.Bronze, "BRONZE CHEST", 0,
                new Color(0.62f, 0.45f, 0.25f), new Color(0.45f, 0.32f, 0.18f),
                "Common: 70% | Rare: 25% | Epic: 4% | Legendary: 1%");

            BuildChestTierCard(ChestRarity.Silver, "SILVER CHEST", 50,
                new Color(0.62f, 0.64f, 0.70f), new Color(0.42f, 0.44f, 0.50f),
                "Common: 50% | Rare: 35% | Epic: 12% | Legendary: 3%");

            BuildChestTierCard(ChestRarity.Gold, "GOLD CHEST", 150,
                new Color(0.85f, 0.72f, 0.22f), new Color(0.65f, 0.52f, 0.08f),
                "Common: 25% | Rare: 40% | Epic: 25% | Legendary: 10%");
        }

        // ═══════════════════════════════════════════════════════
        //  CHEST OPEN LOGIC
        // ═══════════════════════════════════════════════════════

        private void OnOpenChest(ChestRarity rarity, int count)
        {
            // Bronze: check cooldown
            if (rarity == ChestRarity.Bronze && ChestManager.Instance != null && ChestManager.Instance.IsBronzeOnCooldown)
            {
                if (chestScrollContent != null)
                    ShowPurchaseFeedback(chestScrollContent, "Chest on cooldown!", new Color(0.85f, 0.55f, 0.2f));
                return;
            }

            // Pre-check affordability for user feedback
            int cost = ChestManager.Instance?.GetChestCost(rarity) ?? 0;
            int totalCost = cost * count;
            if (totalCost > 0 && (CurrencyManager.Instance == null || !CurrencyManager.Instance.CanAffordPremium(totalCost)))
            {
                if (chestScrollContent != null)
                    ShowPurchaseFeedback(chestScrollContent, "Not enough Gems!", new Color(0.85f, 0.25f, 0.2f));
                return;
            }

            if (count <= 1)
            {
                // Subscribe to get hero details for animation
                System.Action<string, bool, string> handler = null;
                handler = (heroId, wasNew, heroRarity) =>
                {
                    UpdateChestAnimCard(heroId, wasNew, heroRarity);
                    if (ChestManager.Instance != null)
                        ChestManager.Instance.OnChestOpened -= handler;
                };
                if (ChestManager.Instance != null)
                    ChestManager.Instance.OnChestOpened += handler;

                ChestManager.Instance?.OpenChest(rarity, heroId =>
                {
                    if (!string.IsNullOrEmpty(heroId))
                    {
                        StartCoroutine(PlayChestAnimation(rarity));
                    }
                    else
                    {
                        if (ChestManager.Instance != null)
                            ChestManager.Instance.OnChestOpened -= handler;
                        RefreshChestPanel();
                    }
                });
            }
            else
            {
                ChestManager.Instance?.OpenChestMulti(rarity, count, results =>
                {
                    if (results != null && results.Count > 0)
                        ShowMultiPullResults(results);
                    else
                        RefreshChestPanel();
                });
            }
        }

        // ═══════════════════════════════════════════════════════
        //  CHEST OPENING ANIMATION (Single)
        // ═══════════════════════════════════════════════════════

        private GameObject BuildChestAnimOverlay(RectTransform p)
        {
            var panel = MakePanel(p, "ChestAnimOverlay", new Color(0, 0, 0, 0.92f)); Stretch(panel);

            // Glow circle
            var glowGO = new GameObject("Glow", typeof(RectTransform), typeof(Image));
            glowGO.transform.SetParent(panel, false);
            chestAnimGlow = glowGO.GetComponent<Image>();
            chestAnimGlow.color = new Color(1, 0.84f, 0.22f, 0);
            var glowRT = glowGO.GetComponent<RectTransform>();
            glowRT.anchorMin = glowRT.anchorMax = new Vector2(0.5f, 0.55f);
            glowRT.pivot = new Vector2(0.5f, 0.5f);
            glowRT.sizeDelta = new Vector2(300, 300);

            // Chest icon
            var chestGO = new GameObject("ChestIcon", typeof(RectTransform), typeof(Image));
            chestGO.transform.SetParent(panel, false);
            chestAnimChestIcon = chestGO.GetComponent<RectTransform>();
            chestGO.GetComponent<Image>().color = new Color(0.72f, 0.52f, 0.28f);
            chestAnimChestIcon.anchorMin = chestAnimChestIcon.anchorMax = new Vector2(0.5f, 0.55f);
            chestAnimChestIcon.pivot = new Vector2(0.5f, 0.5f);
            chestAnimChestIcon.sizeDelta = new Vector2(160, 160);

            // Hero card (revealed after animation)
            var heroCardGO = new GameObject("HeroCard", typeof(RectTransform), typeof(Image));
            heroCardGO.transform.SetParent(panel, false);
            chestAnimHeroCard = heroCardGO.GetComponent<Image>();
            chestAnimHeroCard.color = new Color(0.95f, 0.88f, 0.75f);
            var hcRT = heroCardGO.GetComponent<RectTransform>();
            hcRT.anchorMin = hcRT.anchorMax = new Vector2(0.5f, 0.50f);
            hcRT.pivot = new Vector2(0.5f, 0.5f);
            hcRT.sizeDelta = new Vector2(300, 400);
            heroCardGO.SetActive(false);

            // Hero portrait on card
            var portraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            portraitGO.transform.SetParent(heroCardGO.transform, false);
            chestAnimHeroPortrait = portraitGO.GetComponent<Image>();
            chestAnimHeroPortrait.preserveAspect = true;
            chestAnimHeroPortrait.color = Color.white;
            chestAnimHeroPortrait.raycastTarget = false;
            var prtRT = portraitGO.GetComponent<RectTransform>();
            prtRT.anchorMin = new Vector2(0.05f, 0.35f);
            prtRT.anchorMax = new Vector2(0.95f, 0.95f);
            prtRT.offsetMin = prtRT.offsetMax = Vector2.zero;

            // Hero name on card
            var nameGO = new GameObject("HeroName", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(heroCardGO.transform, false);
            chestAnimHeroName = nameGO.GetComponent<TextMeshProUGUI>();
            chestAnimHeroName.fontSize = 28;
            chestAnimHeroName.color = COL_BTN_TEXT;
            chestAnimHeroName.fontStyle = FontStyles.Bold;
            chestAnimHeroName.alignment = TextAlignmentOptions.Center;
            var nameRt = nameGO.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0);
            nameRt.anchorMax = new Vector2(1, 0.25f);
            nameRt.offsetMin = new Vector2(8, 8);
            nameRt.offsetMax = new Vector2(-8, -8);

            // Rarity label
            var rarGO = new GameObject("Rarity", typeof(RectTransform), typeof(TextMeshProUGUI));
            rarGO.transform.SetParent(heroCardGO.transform, false);
            chestAnimRarityLabel = rarGO.GetComponent<TextMeshProUGUI>();
            chestAnimRarityLabel.fontSize = 20;
            chestAnimRarityLabel.fontStyle = FontStyles.Bold;
            chestAnimRarityLabel.alignment = TextAlignmentOptions.Center;
            var rarRt = rarGO.GetComponent<RectTransform>();
            rarRt.anchorMin = new Vector2(0, 0.25f);
            rarRt.anchorMax = new Vector2(1, 0.35f);
            rarRt.offsetMin = new Vector2(8, 0);
            rarRt.offsetMax = new Vector2(-8, 0);

            // Badge text
            var badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeGO.transform.SetParent(panel, false);
            chestAnimBadge = badgeGO.GetComponent<TextMeshProUGUI>();
            chestAnimBadge.fontSize = 36;
            chestAnimBadge.color = new Color(1, 0.84f, 0.22f);
            chestAnimBadge.fontStyle = FontStyles.Bold;
            chestAnimBadge.alignment = TextAlignmentOptions.Center;
            var badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0, 0.22f);
            badgeRT.anchorMax = new Vector2(1, 0.30f);
            badgeRT.offsetMin = badgeRT.offsetMax = Vector2.zero;
            badgeGO.SetActive(false);

            // Tap-to-skip (full-screen transparent button on top)
            var skipGO = new GameObject("SkipBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            skipGO.transform.SetParent(panel, false);
            var skipRT = skipGO.GetComponent<RectTransform>(); Stretch(skipRT);
            skipGO.GetComponent<Image>().color = Color.clear;
            skipGO.GetComponent<Button>().transition = Selectable.Transition.None;
            skipGO.GetComponent<Button>().onClick.AddListener(() => _chestAnimSkipRequested = true);
            skipGO.transform.SetAsLastSibling();

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private IEnumerator PlayChestAnimation(ChestRarity rarity)
        {
            if (chestAnimOverlay == null) yield break;

            _chestAnimSkipRequested = false;
            chestAnimOverlay.SetActive(true);

            // Reset state
            chestAnimChestIcon.gameObject.SetActive(true);
            chestAnimHeroCard.gameObject.SetActive(false);
            chestAnimBadge.gameObject.SetActive(false);
            chestAnimGlow.color = new Color(1, 0.84f, 0.22f, 0);
            chestAnimChestIcon.localScale = Vector3.one;
            chestAnimChestIcon.localRotation = Quaternion.identity;

            float timeMult = rarity switch
            {
                ChestRarity.Gold => 2f,
                ChestRarity.Silver => 1.5f,
                _ => 1f,
            };

            Color rarityGlow = rarity switch
            {
                ChestRarity.Gold => new Color(1, 0.84f, 0.22f),
                ChestRarity.Silver => new Color(0.70f, 0.72f, 0.78f),
                _ => new Color(0.72f, 0.52f, 0.28f),
            };

            // Set chest icon color
            chestAnimChestIcon.GetComponent<Image>().color = rarityGlow;

            // Phase 1: Shake (0.8s * timeMult)
            float shakeDuration = 0.8f * timeMult;
            float shakeTimer = 0;
            while (shakeTimer < shakeDuration)
            {
                if (_chestAnimSkipRequested) { _chestAnimSkipRequested = false; break; }
                float intensity = shakeTimer / shakeDuration;
                float angle = Mathf.Sin(shakeTimer * 30f) * 12f * intensity;
                chestAnimChestIcon.localRotation = Quaternion.Euler(0, 0, angle);
                chestAnimChestIcon.localScale = Vector3.one * (1f + intensity * 0.15f);
                chestAnimGlow.color = new Color(rarityGlow.r, rarityGlow.g, rarityGlow.b, intensity * 0.5f);
                shakeTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Phase 2: Burst (0.3s)
            chestAnimChestIcon.gameObject.SetActive(false);
            float burstTimer = 0;
            while (burstTimer < 0.3f)
            {
                if (_chestAnimSkipRequested) { _chestAnimSkipRequested = false; break; }
                float t = burstTimer / 0.3f;
                float glowScale = 1f + t * 3f;
                chestAnimGlow.rectTransform.sizeDelta = new Vector2(300 * glowScale, 300 * glowScale);
                chestAnimGlow.color = new Color(rarityGlow.r, rarityGlow.g, rarityGlow.b, 1f - t);
                burstTimer += Time.unscaledDeltaTime;
                yield return null;
            }
            chestAnimGlow.color = new Color(1, 1, 1, 0);

            // Phase 3: Card Reveal (0.5s) with EaseOutBack
            chestAnimHeroCard.gameObject.SetActive(true);
            var heroCardRT = chestAnimHeroCard.GetComponent<RectTransform>();
            Vector2 startPos = new Vector2(0, -400);
            Vector2 endPos = Vector2.zero;
            float revealTimer = 0;
            while (revealTimer < 0.5f)
            {
                if (_chestAnimSkipRequested) { _chestAnimSkipRequested = false; break; }
                float t = revealTimer / 0.5f;
                float eased = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
                heroCardRT.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
                revealTimer += Time.unscaledDeltaTime;
                yield return null;
            }
            heroCardRT.anchoredPosition = endPos;

            // Phase 4: Badge (0.3s)
            chestAnimBadge.gameObject.SetActive(true);
            float badgeTimer = 0;
            while (badgeTimer < 0.3f)
            {
                if (_chestAnimSkipRequested) { _chestAnimSkipRequested = false; break; }
                float t = badgeTimer / 0.3f;
                chestAnimBadge.color = new Color(chestAnimBadge.color.r,
                    chestAnimBadge.color.g, chestAnimBadge.color.b, t);
                badgeTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Phase 5: Wait for final tap to dismiss
            _chestAnimSkipRequested = false;
            yield return new WaitUntil(() => _chestAnimSkipRequested);
            chestAnimOverlay.SetActive(false);
            RefreshChestPanel();
            RefreshHeroesPanel();
        }

        private void UpdateChestAnimCard(string heroId, bool wasNew, string heroRarity)
        {
            if (chestAnimHeroName != null)
            {
                var hero = HeroManager.Instance?.GetHeroById(heroId);
                string name = hero != null ? hero.heroName : heroId;
                chestAnimHeroName.text = name;
            }

            if (chestAnimHeroPortrait != null)
            {
                var heroData = HeroManager.Instance?.GetHeroById(heroId);
                if (heroData != null)
                {
                    chestAnimHeroPortrait.sprite = heroData.heroPortrait ?? heroData.heroArt;
                    chestAnimHeroPortrait.color = Color.white;
                    chestAnimHeroPortrait.gameObject.SetActive(heroData.heroPortrait != null || heroData.heroArt != null);
                }
                else
                {
                    chestAnimHeroPortrait.gameObject.SetActive(false);
                }
            }

            if (chestAnimHeroCard != null)
            {
                Color rarBg = heroRarity switch
                {
                    "Legendary" => new Color(1.00f, 0.94f, 0.75f),
                    "Epic" => new Color(0.90f, 0.82f, 0.92f),
                    "Rare" => new Color(0.82f, 0.88f, 0.95f),
                    _ => new Color(0.95f, 0.88f, 0.75f),
                };
                chestAnimHeroCard.color = rarBg;
            }

            if (chestAnimRarityLabel != null)
            {
                chestAnimRarityLabel.text = heroRarity;
                chestAnimRarityLabel.color = heroRarity switch
                {
                    "Common" => COL_RARITY_COMMON,
                    "Rare" => COL_RARITY_RARE,
                    "Epic" => COL_RARITY_EPIC,
                    "Legendary" => COL_RARITY_LEGENDARY,
                    _ => Color.white,
                };
            }

            if (chestAnimBadge != null)
            {
                chestAnimBadge.text = wasNew ? "NEW HERO!" : "DUPLICATE (+5 Tokens)";
                chestAnimBadge.color = wasNew ? new Color(0.3f, 0.85f, 0.3f) : new Color(1, 0.84f, 0.22f);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  FEATURED HERO BANNER
        // ═══════════════════════════════════════════════════════

        private HeroDataSO PickFeaturedHero()
        {
            if (HeroManager.Instance == null) return null;
            var all = HeroManager.Instance.GetAllHeroes();
            var candidates = all.FindAll(h =>
                (h.rarity == HeroRarity.Epic || h.rarity == HeroRarity.Legendary)
                && !HeroManager.Instance.IsHeroUnlocked(h.heroId));
            if (candidates.Count == 0)
                candidates = all.FindAll(h => h.rarity == HeroRarity.Epic || h.rarity == HeroRarity.Legendary);
            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private void BuildFeaturedHeroSection()
        {
            _featuredHero = PickFeaturedHero();
            if (_featuredHero == null || chestScrollContent == null) return;

            Color rarColor = GetRarityColor(_featuredHero.rarity);
            Color rarColorDark = GetRarityColorDark(_featuredHero.rarity);

            var section = MakePanel(chestScrollContent, "FeaturedHero", rarColorDark);
            AddLE(section.gameObject, 200);

            var sectionHL = section.gameObject.AddComponent<HorizontalLayoutGroup>();
            sectionHL.spacing = 12;
            sectionHL.childAlignment = TextAnchor.MiddleCenter;
            sectionHL.childForceExpandWidth = false;
            sectionHL.childForceExpandHeight = true;
            sectionHL.childControlWidth = true;
            sectionHL.childControlHeight = true;
            sectionHL.padding = new RectOffset(14, 14, 10, 10);

            // Portrait side
            var portraitHolder = MakePanel(section, "FPH", new Color(0, 0, 0, 0.2f));
            AddLE(portraitHolder.gameObject, -1, 140);
            if (_featuredHero.heroPortrait != null || _featuredHero.heroArt != null)
            {
                var imgGO = new GameObject("FPImg", typeof(RectTransform), typeof(Image));
                imgGO.transform.SetParent(portraitHolder, false);
                var img = imgGO.GetComponent<Image>();
                img.sprite = _featuredHero.heroPortrait ?? _featuredHero.heroArt;
                img.preserveAspect = true;
                img.raycastTarget = false;
                Stretch(imgGO.GetComponent<RectTransform>());
            }

            // Info side
            var infoGO = new GameObject("FInfo", typeof(RectTransform));
            infoGO.transform.SetParent(section, false);
            AddLE(infoGO, -1);
            var infoVL = infoGO.AddComponent<VerticalLayoutGroup>();
            infoVL.spacing = 4;
            infoVL.childAlignment = TextAnchor.MiddleCenter;
            infoVL.childForceExpandWidth = true;
            infoVL.childForceExpandHeight = false;
            infoVL.childControlWidth = true;
            infoVL.childControlHeight = true;
            infoVL.padding = new RectOffset(4, 4, 4, 4);

            var infoRT = infoGO.GetComponent<RectTransform>();

            AddLayoutText(infoRT, "FName", _featuredHero.heroName, 26,
                Color.white, FontStyles.Bold, 34);
            AddLayoutText(infoRT, "FRarity",
                $"{_featuredHero.rarity} {GetRarityStars(_featuredHero.rarity)}", 18,
                rarColor, FontStyles.Bold, 24);
            AddLayoutText(infoRT, "FBoost", "BOOSTED RATE!", 22,
                new Color(1, 0.84f, 0.22f), FontStyles.Bold, 30);

            section.transform.SetAsFirstSibling();
        }

        // ═══════════════════════════════════════════════════════
        //  MULTI-PULL RESULTS
        // ═══════════════════════════════════════════════════════

        private GameObject BuildChestMultiOverlay(RectTransform p)
        {
            var panel = MakePanel(p, "ChestMultiOverlay", new Color(0, 0, 0, 0.92f)); Stretch(panel);

            var box = MakePanel(panel, "MultiBox", new Color(0.95f, 0.88f, 0.75f));
            box.anchorMin = new Vector2(0.05f, 0.05f);
            box.anchorMax = new Vector2(0.95f, 0.95f);
            box.offsetMin = box.offsetMax = Vector2.zero;

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 10;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.padding = new RectOffset(16, 16, 20, 16);

            AddLayoutText(box, "MultiTitle", "CHEST RESULTS", 34, COL_BTN_TEXT, FontStyles.Bold, 44);

            // Grid area
            var gridGO = new GameObject("Grid", typeof(RectTransform));
            gridGO.transform.SetParent(box, false);
            var gridLE = gridGO.AddComponent<LayoutElement>();
            gridLE.flexibleHeight = 1;
            chestMultiGrid = gridGO.GetComponent<RectTransform>();

            var gl = gridGO.AddComponent<GridLayoutGroup>();
            gl.cellSize = new Vector2(120, 140);
            gl.spacing = new Vector2(10, 10);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = 5;
            gl.childAlignment = TextAnchor.UpperCenter;
            gl.padding = new RectOffset(8, 8, 8, 8);

            // Summary
            var sumGO = new GameObject("Summary", typeof(RectTransform), typeof(TextMeshProUGUI));
            sumGO.transform.SetParent(box, false);
            AddLE(sumGO, 50);
            chestMultiSummary = sumGO.GetComponent<TextMeshProUGUI>();
            chestMultiSummary.fontSize = 20;
            chestMultiSummary.color = COL_BTN_TEXT;
            chestMultiSummary.fontStyle = FontStyles.Bold;
            chestMultiSummary.alignment = TextAlignmentOptions.Center;

            // Close button
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 24, 54,
                () =>
                {
                    panel.gameObject.SetActive(false);
                    RefreshChestPanel();
                    RefreshHeroesPanel();
                });

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        private void ShowMultiPullResults(List<ChestResult> results)
        {
            if (chestMultiOverlay == null || chestMultiGrid == null) return;

            // Clear grid
            for (int i = chestMultiGrid.childCount - 1; i >= 0; i--)
                Destroy(chestMultiGrid.GetChild(i).gameObject);

            int newCount = 0;
            int dupCount = 0;

            foreach (var r in results)
            {
                if (r.wasNew) newCount++;
                else dupCount++;

                Color rarCol = r.heroRarity switch
                {
                    "Common" => COL_RARITY_COMMON,
                    "Rare" => COL_RARITY_RARE,
                    "Epic" => COL_RARITY_EPIC,
                    "Legendary" => COL_RARITY_LEGENDARY,
                    _ => COL_RARITY_COMMON,
                };

                var card = MakePanel(chestMultiGrid, $"R_{r.heroId}", new Color(0.2f, 0.15f, 0.1f, 0.8f));

                var cardVL = card.gameObject.AddComponent<VerticalLayoutGroup>();
                cardVL.spacing = 2;
                cardVL.childAlignment = TextAnchor.UpperCenter;
                cardVL.childForceExpandWidth = true;
                cardVL.childForceExpandHeight = false;
                cardVL.childControlWidth = true;
                cardVL.childControlHeight = true;
                cardVL.padding = new RectOffset(4, 4, 4, 4);

                // Rarity border at top
                var border = MakePanel(card, "Border", rarCol);
                AddLE(border.gameObject, 4);

                // Portrait placeholder
                var hero = HeroManager.Instance?.GetHeroById(r.heroId);
                if (hero?.heroPortrait != null)
                {
                    var imgHolder = MakePanel(card, "ImgH", Color.clear);
                    AddLE(imgHolder.gameObject, 70);
                    var imgGO = new GameObject("Img", typeof(RectTransform), typeof(Image));
                    imgGO.transform.SetParent(imgHolder, false);
                    var img = imgGO.GetComponent<Image>();
                    img.sprite = hero.heroPortrait;
                    img.preserveAspect = true;
                    img.raycastTarget = false;
                    Stretch(imgGO.GetComponent<RectTransform>());
                }
                else
                {
                    var placeholder = MakePanel(card, "Ph", rarCol * 0.5f);
                    AddLE(placeholder.gameObject, 70);
                }

                // Name
                string name = hero != null ? hero.heroName : r.heroId;
                if (name.Length > 8) name = name[..8] + "..";
                AddLayoutText(card, "Name", name, 12, Color.white, FontStyles.Bold, 18);

                // New/Dup label
                string badge = r.wasNew ? "NEW!" : "DUP";
                Color badgeCol = r.wasNew ? new Color(0.3f, 0.85f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                AddLayoutText(card, "Badge", badge, 11, badgeCol, FontStyles.Bold, 16);
            }

            if (chestMultiSummary != null)
            {
                int tokenGain = dupCount * 5;
                chestMultiSummary.text = $"{newCount} New Heroes! {dupCount} Duplicates (+{tokenGain} Tokens)";
            }

            chestMultiOverlay.SetActive(true);
        }
    }
}
