using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using IOChef.Economy;

namespace IOChef.UI
{
    public partial class MainMenuUI
    {
        // ─── Shop panel dynamic refs ───
        private TextMeshProUGUI shopCoinsLabel;
        private TextMeshProUGUI shopGemsLabel;
        private RectTransform shopScrollContent;
        private TextMeshProUGUI shopTimerLabel;
        private Coroutine _shopTimerCoroutine;
        private bool _shopDealsLoaded;
        private bool _shopBundlesLoaded;

        // ═══════════════════════════════════════════════════════
        //  BUILD SHOP PANEL
        // ═══════════════════════════════════════════════════════

        private GameObject BuildShopPanel(RectTransform p)
        {
            var panel = MakePanel(p, "ShopPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "ShopBox", new Color(0.95f, 0.88f, 0.75f));
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
            AddLayoutText(box, "ShopTitle", "SHOP", 38, COL_BTN_TEXT, FontStyles.Bold, 50);

            // Currency bar
            var currBar = MakePanel(box, "CurrBar", new Color(0.22f, 0.13f, 0.04f, 0.12f));
            AddLE(currBar.gameObject, 42);
            var chl = currBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            chl.spacing = 30;
            chl.childAlignment = TextAnchor.MiddleCenter;
            chl.childForceExpandWidth = true;
            chl.childForceExpandHeight = false;
            chl.childControlWidth = true;
            chl.childControlHeight = true;
            chl.padding = new RectOffset(16, 16, 4, 4);

            shopCoinsLabel = MakeCurrencyLabel(currBar, "Coins", "coins", "0", 22, COL_BTN_TEXT, 36);
            shopGemsLabel = MakeCurrencyLabel(currBar, "Gems", "gems", "0", 22, new Color(0.20f, 0.55f, 0.85f), 36);

            // Main scroll area
            var scrollGO = new GameObject("ShopScroll", typeof(RectTransform),
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
            shopScrollContent = content.GetComponent<RectTransform>();
            shopScrollContent.anchorMin = new Vector2(0, 1);
            shopScrollContent.anchorMax = new Vector2(1, 1);
            shopScrollContent.pivot = new Vector2(0.5f, 1);
            shopScrollContent.sizeDelta = new Vector2(0, 0);

            var cvl = content.AddComponent<VerticalLayoutGroup>();
            cvl.spacing = 14;
            cvl.childAlignment = TextAnchor.UpperCenter;
            cvl.childForceExpandWidth = true;
            cvl.childForceExpandHeight = false;
            cvl.childControlWidth = true;
            cvl.childControlHeight = true;
            cvl.padding = new RectOffset(8, 8, 8, 8);

            content.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = vpRT;
            scrollRect.content = shopScrollContent;

            // Close
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () =>
                {
                    if (_shopTimerCoroutine != null) StopCoroutine(_shopTimerCoroutine);
                    panel.gameObject.SetActive(false);
                });

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH SHOP
        // ═══════════════════════════════════════════════════════

        private void RefreshShopPanel()
        {
            if (CurrencyManager.Instance != null)
            {
                if (shopCoinsLabel != null)
                    shopCoinsLabel.text = $"{CurrencyManager.Instance.Coins}";
                if (shopGemsLabel != null)
                    shopGemsLabel.text = $"{CurrencyManager.Instance.Gems}";
            }

            if (shopScrollContent == null) return;

            for (int i = shopScrollContent.childCount - 1; i >= 0; i--)
                Destroy(shopScrollContent.GetChild(i).gameObject);

            _shopDealsLoaded = false;
            _shopBundlesLoaded = false;

            // Show loading state while fetching
            var loadingPanel = MakePanel(shopScrollContent, "LoadingPanel", Color.clear);
            AddLE(loadingPanel.gameObject, 60);
            AddLayoutText(loadingPanel, "LoadTxt", "Loading shop...", 20,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic, 50);

            // Fetch server data
            if (DailyDealsManager.Instance != null)
            {
                DailyDealsManager.Instance.FetchDeals(() =>
                {
                    _shopDealsLoaded = true;
                    if (_shopBundlesLoaded) RebuildShopContent();
                });
            }
            else
            {
                _shopDealsLoaded = true;
            }

            if (BundleManager.Instance != null)
            {
                BundleManager.Instance.FetchBundles(() =>
                {
                    _shopBundlesLoaded = true;
                    if (_shopDealsLoaded) RebuildShopContent();
                });
            }
            else
            {
                _shopBundlesLoaded = true;
            }

            // If both managers are null, build immediately with what we have
            if (_shopDealsLoaded && _shopBundlesLoaded)
                RebuildShopContent();

            // Start timer
            if (_shopTimerCoroutine != null) StopCoroutine(_shopTimerCoroutine);
            _shopTimerCoroutine = StartCoroutine(ShopTimerCoroutine());
        }

        private void RebuildShopContent()
        {
            if (shopScrollContent == null) return;

            // Clear everything
            for (int i = shopScrollContent.childCount - 1; i >= 0; i--)
                Destroy(shopScrollContent.GetChild(i).gameObject);

            PopulateDailyDeals();
            PopulateBundles();
            PopulateGemExchange();
        }

        // ═══════════════════════════════════════════════════════
        //  DAILY DEALS
        // ═══════════════════════════════════════════════════════

        private void PopulateDailyDeals()
        {
            if (shopScrollContent == null) return;

            // Section container
            var section = MakePanel(shopScrollContent, "DailyDealsSection", Color.clear);
            AddLE(section.gameObject, 0);

            var sVL = section.gameObject.AddComponent<VerticalLayoutGroup>();
            sVL.spacing = 10;
            sVL.childAlignment = TextAnchor.UpperCenter;
            sVL.childForceExpandWidth = true;
            sVL.childForceExpandHeight = false;
            sVL.childControlWidth = true;
            sVL.childControlHeight = true;
            sVL.padding = new RectOffset(0, 0, 4, 4);

            var sCSF = section.gameObject.AddComponent<ContentSizeFitter>();
            sCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Header with timer
            var headerRow = MakePanel(section, "DealHeader", Color.clear);
            AddLE(headerRow.gameObject, 36);
            var hHL = headerRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hHL.spacing = 10;
            hHL.childAlignment = TextAnchor.MiddleCenter;
            hHL.childForceExpandWidth = true;
            hHL.childForceExpandHeight = false;
            hHL.childControlWidth = true;
            hHL.childControlHeight = true;

            var dealTitle = MakeText(headerRow, "DT", "DAILY DEALS", 26, COL_BTN_TEXT, FontStyles.Bold);
            var dtLE = dealTitle.gameObject.AddComponent<LayoutElement>();
            dtLE.flexibleWidth = 1;
            dtLE.preferredHeight = 32;
            dealTitle.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;

            var timerGO = MakeText(headerRow, "Timer", "00:00:00", 20,
                new Color(0.85f, 0.25f, 0.2f), FontStyles.Bold);
            var timerLE = timerGO.gameObject.AddComponent<LayoutElement>();
            timerLE.preferredWidth = 120;
            timerLE.preferredHeight = 32;
            shopTimerLabel = timerGO.GetComponent<TextMeshProUGUI>();

            // Get deals
            var deals = DailyDealsManager.Instance?.GetTodayDeals();
            if (deals == null || deals.Count == 0)
            {
                AddLayoutText(section, "NoDeal", "No deals available right now", 18,
                    new Color(0.55f, 0.42f, 0.25f), FontStyles.Italic, 40);
                return;
            }

            // Featured deal banner (best unpurchased deal)
            DailyDealData featuredDeal = null;
            int bestValue = 0;
            foreach (var d in deals)
            {
                if (DailyDealsManager.Instance.IsDealPurchased(d.dealId)) continue;
                int value = d.amount * (d.normalGemCost > 0 ? d.normalGemCost : 1);
                if (value > bestValue)
                {
                    bestValue = value;
                    featuredDeal = d;
                }
            }

            if (featuredDeal != null)
                BuildFeaturedDealBanner(section, featuredDeal);

            // Remaining deals in horizontal scroll
            var dealsScrollGO = new GameObject("DealsScroll", typeof(RectTransform),
                typeof(ScrollRect), typeof(Image));
            dealsScrollGO.transform.SetParent(section, false);
            AddLE(dealsScrollGO, 170);
            dealsScrollGO.GetComponent<Image>().color = Color.clear;

            var dealsScrollRect = dealsScrollGO.GetComponent<ScrollRect>();
            dealsScrollRect.horizontal = true;
            dealsScrollRect.vertical = false;
            dealsScrollRect.movementType = ScrollRect.MovementType.Clamped;

            var dealsVP = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            dealsVP.transform.SetParent(dealsScrollGO.transform, false);
            var dealsVPRT = dealsVP.GetComponent<RectTransform>(); Stretch(dealsVPRT);
            dealsVP.GetComponent<Image>().color = Color.white;
            dealsVP.GetComponent<Mask>().showMaskGraphic = false;

            var dealsRow = new GameObject("DealsRow", typeof(RectTransform));
            dealsRow.transform.SetParent(dealsVP.transform, false);
            var dealsRowRT = dealsRow.GetComponent<RectTransform>();
            dealsRowRT.anchorMin = new Vector2(0, 0);
            dealsRowRT.anchorMax = new Vector2(0, 1);
            dealsRowRT.pivot = new Vector2(0, 0.5f);
            dealsRowRT.sizeDelta = new Vector2(0, 0);

            var drHL = dealsRow.AddComponent<HorizontalLayoutGroup>();
            drHL.spacing = 10;
            drHL.childAlignment = TextAnchor.MiddleLeft;
            drHL.childForceExpandWidth = false;
            drHL.childForceExpandHeight = true;
            drHL.childControlWidth = true;
            drHL.childControlHeight = true;

            dealsRow.AddComponent<ContentSizeFitter>().horizontalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            dealsScrollRect.viewport = dealsVPRT;
            dealsScrollRect.content = dealsRowRT;

            var dealsRowParent = dealsRow.GetComponent<RectTransform>();

            foreach (var deal in deals)
            {
                bool purchased = DailyDealsManager.Instance.IsDealPurchased(deal.dealId);
                BuildDealCard(dealsRowParent, deal, purchased);
            }
        }

        private void BuildDealCard(RectTransform parent, DailyDealData deal, bool purchased)
        {
            Color cardBg = deal.isFree
                ? new Color(0.25f, 0.65f, 0.25f, 0.3f)
                : new Color(0.2f, 0.45f, 0.8f, 0.2f);
            if (purchased) cardBg = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            var card = MakePanel(parent, $"Deal_{deal.dealId}", cardBg);
            var cardLE = card.gameObject.AddComponent<LayoutElement>();
            cardLE.preferredWidth = 160;
            cardLE.preferredHeight = 160;

            var vl = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 4;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.padding = new RectOffset(6, 6, 8, 6);

            string typeLabel = DealTypeLabel(deal.type);
            string iconType = DealIconType(deal.type);

            if (!string.IsNullOrEmpty(iconType))
                MakeCurrencyLabel(card, "Amt", iconType, $"{deal.amount}", 28, COL_BTN_TEXT, 34);
            else
                AddLayoutText(card, "Amt", $"{deal.amount}", 28, COL_BTN_TEXT, FontStyles.Bold, 34);
            AddLayoutText(card, "Type", typeLabel, 14,
                new Color(0.42f, 0.32f, 0.18f), FontStyles.Normal, 20);

            // Original price (struck through)
            if (deal.normalGemCost > 0)
            {
                var origRT = MakeText(card, "Orig", $"{deal.normalGemCost}G", 14,
                    new Color(0.6f, 0.4f, 0.4f), FontStyles.Strikethrough);
                AddLE(origRT.gameObject, 18);
            }

            // Buy button or SOLD
            if (purchased)
            {
                AddLayoutText(card, "Sold", "SOLD", 16, COL_DISABLED, FontStyles.Bold, 30);
            }
            else
            {
                string btnLabel = deal.isFree ? "FREE" : $"{deal.dealGemCost}G";
                Color btnColor = deal.isFree ? COL_PLAY : new Color(0.20f, 0.55f, 0.85f);
                Color btnShadow = deal.isFree ? COL_PLAY_SHADOW : new Color(0.12f, 0.38f, 0.62f);
                string capturedId = deal.dealId;
                int capturedAmount = deal.amount;
                string capturedType = typeLabel;
                MakeChunkyButton(card, btnLabel, btnColor, btnShadow, Color.white, 16, 36,
                    () =>
                    {
                        DailyDealsManager.Instance?.PurchaseDeal(capturedId, success =>
                        {
                            if (success)
                            {
                                ShowPurchaseFeedback(shopScrollContent,
                                    $"+{capturedAmount} {capturedType}", COL_PLAY);
                                RefreshShopPanel();
                            }
                        });
                    });
            }
        }

        private void BuildFeaturedDealBanner(RectTransform parent, DailyDealData deal)
        {
            bool purchased = DailyDealsManager.Instance != null
                && DailyDealsManager.Instance.IsDealPurchased(deal.dealId);
            Color bannerBg = deal.isFree
                ? new Color(0.15f, 0.55f, 0.15f, 0.4f)
                : new Color(0.85f, 0.35f, 0.10f, 0.35f);
            if (purchased) bannerBg = new Color(0.5f, 0.5f, 0.5f, 0.3f);

            var banner = MakePanel(parent, "FeaturedDeal", bannerBg);
            AddLE(banner.gameObject, 180);

            var bannerVL = banner.gameObject.AddComponent<VerticalLayoutGroup>();
            bannerVL.spacing = 6;
            bannerVL.childAlignment = TextAnchor.MiddleCenter;
            bannerVL.childForceExpandWidth = true;
            bannerVL.childForceExpandHeight = false;
            bannerVL.childControlWidth = true;
            bannerVL.childControlHeight = true;
            bannerVL.padding = new RectOffset(16, 16, 10, 10);

            // HOT DEAL ribbon
            AddLayoutText(banner, "Ribbon", "HOT DEAL", 16,
                new Color(1, 0.84f, 0.22f), FontStyles.Bold, 24);

            string typeLabel = DealTypeLabel(deal.type);
            string iconType = DealIconType(deal.type);

            if (!string.IsNullOrEmpty(iconType))
                MakeCurrencyLabel(banner, "FAmt", iconType,
                    $"{deal.amount} {typeLabel}", 36, Color.white, 48);
            else
                AddLayoutText(banner, "FAmt", $"{deal.amount} {typeLabel}",
                    36, Color.white, FontStyles.Bold, 48);

            // Strikethrough original price
            if (deal.normalGemCost > 0)
            {
                var origRT = MakeText(banner, "FOrigPrice",
                    $"Was: {deal.normalGemCost} Gems", 16,
                    new Color(0.8f, 0.6f, 0.6f), FontStyles.Strikethrough);
                AddLE(origRT.gameObject, 22);
            }

            // Buy button
            if (!purchased)
            {
                string btnLabel = deal.isFree ? "CLAIM FREE" : $"BUY {deal.dealGemCost}G";
                Color btnColor = deal.isFree ? COL_PLAY : new Color(0.20f, 0.55f, 0.85f);
                Color btnShadow = deal.isFree ? COL_PLAY_SHADOW : new Color(0.12f, 0.38f, 0.62f);
                string capturedId = deal.dealId;
                int capturedAmount = deal.amount;
                string capturedType = typeLabel;
                MakeChunkyButton(banner, btnLabel, btnColor, btnShadow, Color.white, 22, 52,
                    () =>
                    {
                        DailyDealsManager.Instance?.PurchaseDeal(capturedId, success =>
                        {
                            if (success)
                            {
                                ShowPurchaseFeedback(shopScrollContent,
                                    $"+{capturedAmount} {capturedType}", COL_PLAY);
                                RefreshShopPanel();
                            }
                        });
                    });
            }
            else
            {
                AddLayoutText(banner, "FSold", "SOLD", 22, COL_DISABLED, FontStyles.Bold, 40);
            }
        }

        // ═══════════════════════════════════════════════════════
        //  BUNDLES
        // ═══════════════════════════════════════════════════════

        private void PopulateBundles()
        {
            if (shopScrollContent == null) return;

            var bundles = BundleManager.Instance?.GetAvailableBundles();
            if (bundles == null || bundles.Count == 0) return;

            // Section header
            AddLayoutText(shopScrollContent, "BH", "BUNDLES", 26, COL_BTN_TEXT, FontStyles.Bold, 36);

            foreach (var bundle in bundles)
                BuildBundleCard(shopScrollContent, bundle);
        }

        private void BuildBundleCard(RectTransform parent, BundleData bundle)
        {
            var card = MakePanel(parent, $"Bundle_{bundle.bundleId}",
                new Color(0.30f, 0.22f, 0.50f, 0.3f));
            AddLE(card.gameObject, 170);

            var vl = card.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 6;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlWidth = true;
            vl.childControlHeight = true;
            vl.padding = new RectOffset(14, 14, 10, 10);

            // Name + value badge
            var nameRow = MakePanel(card, "NameRow", Color.clear);
            AddLE(nameRow.gameObject, 36);
            var nHL = nameRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            nHL.spacing = 8;
            nHL.childAlignment = TextAnchor.MiddleCenter;
            nHL.childForceExpandWidth = true;
            nHL.childForceExpandHeight = false;
            nHL.childControlWidth = true;
            nHL.childControlHeight = true;

            var nameRT = MakeText(nameRow, "Name", bundle.name, 24, COL_BTN_TEXT, FontStyles.Bold);
            var nameLE = nameRT.gameObject.AddComponent<LayoutElement>();
            nameLE.flexibleWidth = 1;
            nameLE.preferredHeight = 32;
            var nameTMP = nameRT.GetComponent<TextMeshProUGUI>();
            nameTMP.alignment = TextAlignmentOptions.Left;
            nameTMP.overflowMode = TextOverflowModes.Ellipsis;
            nameTMP.enableWordWrapping = false;

            if (bundle.valueMultiplier > 1)
            {
                var badgeBg = MakePanel(nameRow, "Badge", COL_BADGE);
                var badgeLE = badgeBg.gameObject.AddComponent<LayoutElement>();
                badgeLE.preferredWidth = 80;
                badgeLE.preferredHeight = 28;
                var badgeTxt = MakeText(badgeBg, "Txt",
                    $"{bundle.valueMultiplier:F0}X VALUE", 12, Color.white, FontStyles.Bold);
                Stretch(badgeTxt);
            }

            // Contents row with icons
            var contentsRow = MakePanel(card, "Contents", Color.clear);
            AddLE(contentsRow.gameObject, 30);
            var contHL = contentsRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            contHL.spacing = 12;
            contHL.childAlignment = TextAnchor.MiddleCenter;
            contHL.childForceExpandWidth = false;
            contHL.childForceExpandHeight = false;
            contHL.childControlWidth = true;
            contHL.childControlHeight = true;

            foreach (var c in bundle.contents)
            {
                string iconType = c.type switch
                {
                    "coins" => "coins",
                    "gems" => "gems",
                    "heroTokens" => "tokens",
                    _ => "",
                };
                string typeName = c.type switch
                {
                    "coins" => "Coins",
                    "gems" => "Gems",
                    "heroTokens" => "Tokens",
                    "goldChests" => "Gold Chests",
                    _ => c.type,
                };
                if (!string.IsNullOrEmpty(iconType))
                    MakeCurrencyIcon(contentsRow, iconType, 18);

                var itemRT = MakeText(contentsRow, $"C_{c.type}",
                    $"{c.amount} {typeName}", 14,
                    new Color(0.42f, 0.32f, 0.18f), FontStyles.Bold);
                var itemLE = itemRT.gameObject.AddComponent<LayoutElement>();
                itemLE.preferredHeight = 24;
            }

            // Buy button
            string capturedId = bundle.bundleId;
            string capturedName = bundle.name;
            MakeChunkyButton(card, $"BUY ({bundle.gemCost}G)",
                new Color(0.55f, 0.28f, 0.85f), new Color(0.35f, 0.15f, 0.60f),
                Color.white, 20, 52,
                () =>
                {
                    BundleManager.Instance?.PurchaseBundle(capturedId, success =>
                    {
                        if (success)
                        {
                            ShowPurchaseFeedback(shopScrollContent,
                                $"Bundle: {capturedName}!", new Color(0.55f, 0.28f, 0.85f));
                            RefreshShopPanel();
                        }
                    });
                });
        }

        // ═══════════════════════════════════════════════════════
        //  GEM EXCHANGE
        // ═══════════════════════════════════════════════════════

        private void PopulateGemExchange()
        {
            if (shopScrollContent == null) return;

            // Section header
            AddLayoutText(shopScrollContent, "ExH", "GEM EXCHANGE", 26,
                COL_BTN_TEXT, FontStyles.Bold, 36);

            // Description row with icons
            var exDescRow = MakePanel(shopScrollContent, "ExDesc", Color.clear);
            AddLE(exDescRow.gameObject, 28);
            var exDescHL = exDescRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            exDescHL.spacing = 6;
            exDescHL.childAlignment = TextAnchor.MiddleCenter;
            exDescHL.childForceExpandWidth = false;
            exDescHL.childForceExpandHeight = false;
            exDescHL.childControlWidth = true;
            exDescHL.childControlHeight = true;
            MakeCurrencyIcon(exDescRow, "gems", 20);
            var exTxt1 = MakeText(exDescRow, "E1", "1 Gem = ", 18,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic);
            exTxt1.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            MakeCurrencyIcon(exDescRow, "coins", 20);
            var exTxt2 = MakeText(exDescRow, "E2", "100 Coins", 18,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic);
            exTxt2.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

            // Exchange buttons - each in its own row for proper sizing
            Color gemBlue = new(0.20f, 0.55f, 0.85f);
            Color gemBlueSh = new(0.12f, 0.38f, 0.62f);

            var btnRow = MakePanel(shopScrollContent, "ExBtnRow", Color.clear);
            AddLE(btnRow.gameObject, 60);
            var btnHL = btnRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            btnHL.spacing = 10;
            btnHL.childAlignment = TextAnchor.MiddleCenter;
            btnHL.childForceExpandWidth = true;
            btnHL.childForceExpandHeight = true;
            btnHL.childControlWidth = true;
            btnHL.childControlHeight = true;
            btnHL.padding = new RectOffset(4, 4, 4, 4);

            MakeChunkyButton(btnRow, "1 GEM", gemBlue, gemBlueSh, Color.white, 16, 50,
                () =>
                {
                    CurrencyManager.Instance?.ConvertGemToCoins(1);
                    ShowPurchaseFeedback(shopScrollContent, "+100 Coins",
                        new Color(1f, 0.84f, 0.22f));
                    RefreshShopPanel();
                });
            MakeChunkyButton(btnRow, "5 GEMS", gemBlue, gemBlueSh, Color.white, 16, 50,
                () =>
                {
                    CurrencyManager.Instance?.ConvertGemToCoins(5);
                    ShowPurchaseFeedback(shopScrollContent, "+500 Coins",
                        new Color(1f, 0.84f, 0.22f));
                    RefreshShopPanel();
                });
            MakeChunkyButton(btnRow, "10 GEMS", gemBlue, gemBlueSh, Color.white, 16, 50,
                () =>
                {
                    CurrencyManager.Instance?.ConvertGemToCoins(10);
                    ShowPurchaseFeedback(shopScrollContent, "+1000 Coins",
                        new Color(1f, 0.84f, 0.22f));
                    RefreshShopPanel();
                });
        }

        // ═══════════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════════

        private static string DealTypeLabel(string type) => type switch
        {
            "coins" => "Coins",
            "heroTokens" => "Tokens",
            "gems" => "Gems",
            "bronzeChest" => "Bronze Chest",
            _ => type,
        };

        private static string DealIconType(string type) => type switch
        {
            "coins" => "coins",
            "gems" => "gems",
            "heroTokens" => "tokens",
            _ => "",
        };

        // ─── Timer Coroutine ───
        private IEnumerator ShopTimerCoroutine()
        {
            while (true)
            {
                if (DailyDealsManager.Instance == null)
                {
                    if (shopTimerLabel != null) shopTimerLabel.text = "--:--:--";
                    yield return new WaitForSecondsRealtime(2f);
                    continue;
                }

                float secs = DailyDealsManager.Instance.GetSecondsUntilReset();
                if (secs <= 0)
                {
                    if (shopTimerLabel != null) shopTimerLabel.text = "REFRESHING...";
                    DailyDealsManager.Instance.FetchDeals(() => RebuildShopContent());
                    yield return new WaitForSecondsRealtime(5f);
                    continue;
                }

                int h = (int)(secs / 3600);
                int m = (int)((secs % 3600) / 60);
                int s = (int)(secs % 60);
                if (shopTimerLabel != null)
                    shopTimerLabel.text = $"{h:D2}:{m:D2}:{s:D2}";

                yield return new WaitForSecondsRealtime(1f);
            }
        }
    }
}
