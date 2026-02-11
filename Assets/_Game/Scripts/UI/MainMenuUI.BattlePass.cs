using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IOChef.Economy;

namespace IOChef.UI
{
    public partial class MainMenuUI
    {
        // ─── Season Pass panel dynamic refs ───
        private TextMeshProUGUI bpTierLabel;
        private TextMeshProUGUI bpXPLabel;
        private Image bpXPFill;
        private TextMeshProUGUI bpPremiumLabel;
        private RectTransform bpTierTrackContent;
        private ScrollRect bpTierTrackScroll;
        private TextMeshProUGUI bpCountdownLabel;

        // ═══════════════════════════════════════════════════════
        //  BUILD SEASON PASS PANEL
        // ═══════════════════════════════════════════════════════

        private GameObject BuildBattlePassPanel(RectTransform p)
        {
            var panel = MakePanel(p, "BattlePassPanel", COL_PANEL_BG); Stretch(panel);

            var box = MakePanel(panel, "BPBox", new Color(0.95f, 0.88f, 0.75f));
            box.anchorMin = new Vector2(0.03f, 0.02f);
            box.anchorMax = new Vector2(0.97f, 0.98f);
            box.offsetMin = box.offsetMax = Vector2.zero;

            var vl = box.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.spacing = 10;
            vl.childAlignment = TextAnchor.UpperCenter;
            vl.childForceExpandWidth = true;
            vl.childForceExpandHeight = false;
            vl.childControlHeight = true;
            vl.childControlWidth = true;
            vl.padding = new RectOffset(16, 16, 20, 16);

            // Title
            AddLayoutText(box, "BPTitle", "SEASON PASS", 38, COL_BTN_TEXT, FontStyles.Bold, 50);

            int season = BattlePassManager.Instance?.CurrentSeason ?? 1;
            AddLayoutText(box, "BPSeason", $"Season {season}", 20,
                new Color(0.45f, 0.32f, 0.15f), FontStyles.Italic, 28);

            // Season countdown
            var countdownRT = MakeText(box, "BPCountdown", "", 18, new Color(0.85f, 0.25f, 0.2f), FontStyles.Bold);
            AddLE(countdownRT.gameObject, 26);
            bpCountdownLabel = countdownRT.GetComponent<TextMeshProUGUI>();

            // Current tier display
            var tierRT = MakeText(box, "BPTier", "TIER 0", 48,
                new Color(0.55f, 0.28f, 0.85f), FontStyles.Bold);
            AddLE(tierRT.gameObject, 60);
            bpTierLabel = tierRT.GetComponent<TextMeshProUGUI>();

            // XP progress bar
            var xpBarBg = MakePanel(box, "XPBarBg", new Color(0.3f, 0.25f, 0.2f, 0.5f));
            AddLE(xpBarBg.gameObject, 32);

            var xpFillGO = new GameObject("XPFill", typeof(RectTransform), typeof(Image));
            xpFillGO.transform.SetParent(xpBarBg, false);
            bpXPFill = xpFillGO.GetComponent<Image>();
            bpXPFill.color = new Color(0.55f, 0.28f, 0.85f);
            var xpFillRT = xpFillGO.GetComponent<RectTransform>();
            xpFillRT.anchorMin = Vector2.zero;
            xpFillRT.anchorMax = new Vector2(0, 1);
            xpFillRT.offsetMin = xpFillRT.offsetMax = Vector2.zero;

            var xpLabelRT = MakeText(xpBarBg, "XPLbl", "0 / 1000 XP", 16, Color.white, FontStyles.Bold);
            Stretch(xpLabelRT);
            bpXPLabel = xpLabelRT.GetComponent<TextMeshProUGUI>();

            // Premium status / purchase button
            var premRow = MakePanel(box, "PremRow", Color.clear);
            AddLE(premRow.gameObject, 58);
            var premVL = premRow.gameObject.AddComponent<VerticalLayoutGroup>();
            premVL.childAlignment = TextAnchor.MiddleCenter;
            premVL.childForceExpandWidth = true;
            premVL.childForceExpandHeight = false;
            premVL.childControlWidth = true;
            premVL.childControlHeight = true;

            var premLblRT = MakeText(premRow, "PremLbl", "", 22,
                new Color(0.55f, 0.28f, 0.85f), FontStyles.Bold);
            AddLE(premLblRT.gameObject, 50);
            bpPremiumLabel = premLblRT.GetComponent<TextMeshProUGUI>();

            // Horizontal tier track
            AddLayoutText(box, "TrackH", "REWARD TRACK", 22, COL_BTN_TEXT, FontStyles.Bold, 30);

            var trackScrollGO = new GameObject("TrackScroll", typeof(RectTransform),
                typeof(ScrollRect), typeof(Image));
            trackScrollGO.transform.SetParent(box, false);
            var trackScrollLE = trackScrollGO.AddComponent<LayoutElement>();
            trackScrollLE.flexibleHeight = 1;
            trackScrollLE.minHeight = 280;
            trackScrollGO.GetComponent<Image>().color = new Color(0, 0, 0, 0.08f);

            bpTierTrackScroll = trackScrollGO.GetComponent<ScrollRect>();
            bpTierTrackScroll.horizontal = true;
            bpTierTrackScroll.vertical = false;
            bpTierTrackScroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(trackScrollGO.transform, false);
            var vpRT = viewport.GetComponent<RectTransform>(); Stretch(vpRT);
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var trackContent = new GameObject("Content", typeof(RectTransform));
            trackContent.transform.SetParent(viewport.transform, false);
            bpTierTrackContent = trackContent.GetComponent<RectTransform>();
            bpTierTrackContent.anchorMin = new Vector2(0, 0);
            bpTierTrackContent.anchorMax = new Vector2(0, 1);
            bpTierTrackContent.pivot = new Vector2(0, 0.5f);
            bpTierTrackContent.sizeDelta = new Vector2(0, 0);

            var hLayout = trackContent.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 6;
            hLayout.childAlignment = TextAnchor.MiddleLeft;
            hLayout.childForceExpandWidth = false;
            hLayout.childForceExpandHeight = true;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.padding = new RectOffset(8, 8, 8, 8);

            trackContent.AddComponent<ContentSizeFitter>().horizontalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            bpTierTrackScroll.viewport = vpRT;
            bpTierTrackScroll.content = bpTierTrackContent;

            // Close
            MakeChunkyButton(box, "CLOSE", COL_QUIT, COL_QUIT_SHADOW, Color.white, 26, 58,
                () => panel.gameObject.SetActive(false));

            panel.gameObject.SetActive(false);
            return panel.gameObject;
        }

        // ═══════════════════════════════════════════════════════
        //  REFRESH SEASON PASS
        // ═══════════════════════════════════════════════════════

        private void RefreshBattlePassPanel()
        {
            var bp = BattlePassManager.Instance;
            if (bp == null)
            {
                PopulateBattlePassUI();
                return;
            }

            // Fetch config from server
            bp.FetchConfig(() => PopulateBattlePassUI());
        }

        private void PopulateBattlePassUI()
        {
            var bp = BattlePassManager.Instance;
            if (bp == null) return;

            // Tier
            if (bpTierLabel != null)
                bpTierLabel.text = $"TIER {bp.CurrentTier}";

            // Season countdown (using server-adjusted time)
            if (bpCountdownLabel != null)
            {
                System.TimeSpan remaining = bp.SeasonEndUtc - bp.ServerUtcNow;

                if (remaining.TotalSeconds <= 0)
                {
                    bpCountdownLabel.text = "SEASON ENDED";
                    bpCountdownLabel.color = new Color(0.85f, 0.25f, 0.2f);
                }
                else if (remaining.TotalDays < 3)
                {
                    int hours = (int)remaining.TotalHours;
                    bpCountdownLabel.text = $"{hours}h Remaining!";
                    bpCountdownLabel.color = new Color(0.85f, 0.25f, 0.2f);
                }
                else
                {
                    int days = (int)remaining.TotalDays;
                    bpCountdownLabel.text = $"{days} Days Remaining";
                    bpCountdownLabel.color = new Color(0.45f, 0.32f, 0.15f);
                }
            }

            // XP bar
            if (bpXPFill != null)
            {
                float pct = bp.XPPerTier > 0 ? (float)bp.CurrentXP / bp.XPPerTier : 0;
                bpXPFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(pct), 1);
            }
            if (bpXPLabel != null)
                bpXPLabel.text = $"{bp.CurrentXP} / {bp.XPPerTier} XP";

            // Premium status
            if (bpPremiumLabel != null)
            {
                // Clear existing children in parent row (remove old buttons)
                var premParent = bpPremiumLabel.transform.parent;
                for (int i = premParent.childCount - 1; i >= 0; i--)
                {
                    var child = premParent.GetChild(i);
                    if (child != bpPremiumLabel.transform)
                        Destroy(child.gameObject);
                }

                if (bp.IsPremiumPass)
                {
                    bpPremiumLabel.text = "PREMIUM ACTIVE";
                    bpPremiumLabel.color = new Color(0.55f, 0.28f, 0.85f);
                }
                else
                {
                    bpPremiumLabel.text = "";
                    var premBtnParent = premParent.GetComponent<RectTransform>();
                    MakeChunkyButton(premBtnParent, $"BUY PREMIUM ({bp.PremiumCost} GEMS)",
                        new Color(0.55f, 0.28f, 0.85f), new Color(0.35f, 0.15f, 0.60f),
                        Color.white, 20, 50, OnBuyPremiumPass);
                }
            }

            // Populate tier track
            PopulateTierTrack();
        }

        private void OnBuyPremiumPass()
        {
            var bp = BattlePassManager.Instance;
            if (bp == null || bp.IsPremiumPass) return;

            ShowConfirmDialog(
                "Buy Premium Pass",
                $"Upgrade to Premium for {bp.PremiumCost} Gems?\n\n" +
                "Premium unlocks bonus rewards at every tier and 1.5x XP boost!",
                "BUY", new Color(0.55f, 0.28f, 0.85f), () =>
                {
                    bp.PurchasePremiumPass(success =>
                    {
                        if (success)
                            PopulateBattlePassUI();
                        else
                            ShowConfirmDialog("Purchase Failed",
                                "Not enough gems or an error occurred.", "OK", COL_BTN, null);
                    });
                },
                "CANCEL", COL_BTN, null);
        }

        private void PopulateTierTrack()
        {
            if (bpTierTrackContent == null) return;

            for (int i = bpTierTrackContent.childCount - 1; i >= 0; i--)
                Destroy(bpTierTrackContent.GetChild(i).gameObject);

            var bp = BattlePassManager.Instance;
            if (bp == null) return;

            int maxT = bp.MaxTier;
            int curT = bp.CurrentTier;

            for (int t = 0; t <= maxT; t++)
            {
                BuildTierCard(t, curT, bp);
            }

            // Auto-scroll to current tier
            if (bpTierTrackScroll != null && maxT > 0)
            {
                float pos = (float)curT / maxT;
                // Delay to let layout rebuild
                StartCoroutine(ScrollToTierCoroutine(Mathf.Clamp01(pos)));
            }
        }

        private System.Collections.IEnumerator ScrollToTierCoroutine(float normalizedPos)
        {
            yield return null; // wait one frame for layout
            yield return null;
            if (bpTierTrackScroll != null)
            {
                bpTierTrackScroll.horizontalNormalizedPosition =
                    Mathf.Clamp01(normalizedPos - 0.1f);
            }
        }

        private void BuildTierCard(int tier, int currentTier, BattlePassManager bp)
        {
            bool reached = tier <= currentTier;
            bool isCurrent = tier == currentTier;
            bool freeClaimed = bp.IsFreeTierClaimed(tier);
            bool premClaimed = bp.IsPremiumTierClaimed(tier);
            bool isPremium = bp.IsPremiumPass;
            bool isMilestone = tier > 0 && tier % 10 == 0;

            Color borderColor = isMilestone ? new Color(1, 0.84f, 0.22f)
                : isCurrent ? new Color(1, 0.84f, 0.22f)
                : reached ? new Color(0.3f, 0.75f, 0.3f)
                : new Color(0.5f, 0.5f, 0.5f);

            var card = MakePanel(bpTierTrackContent, $"Tier_{tier}", borderColor);
            var cardLE = card.gameObject.AddComponent<LayoutElement>();
            cardLE.preferredWidth = isMilestone ? 150 : 110;

            var cardVL = card.gameObject.AddComponent<VerticalLayoutGroup>();
            cardVL.spacing = 4;
            cardVL.childAlignment = TextAnchor.UpperCenter;
            cardVL.childForceExpandWidth = true;
            cardVL.childForceExpandHeight = false;
            cardVL.childControlWidth = true;
            cardVL.childControlHeight = true;
            cardVL.padding = new RectOffset(4, 4, 4, 4);

            // Milestone glow overlay
            if (isMilestone)
            {
                var glowGO = new GameObject("MilestoneGlow", typeof(RectTransform), typeof(Image));
                glowGO.transform.SetParent(card, false);
                var glowImg = glowGO.GetComponent<Image>();
                glowImg.color = new Color(1, 0.84f, 0.22f, 0.12f);
                glowImg.raycastTarget = false;
                var glowRT = glowGO.GetComponent<RectTransform>();
                Stretch(glowRT);
                var glowLE = glowGO.AddComponent<LayoutElement>();
                glowLE.ignoreLayout = true;

                var pulse = glowGO.AddComponent<AlphaPulse>();
                pulse.speed = 2f;
                pulse.minAlpha = 0.05f;
                pulse.maxAlpha = 0.20f;
            }

            // Tier number
            Color tierNumCol = isMilestone ? new Color(1, 0.84f, 0.22f)
                : isCurrent ? new Color(1, 0.84f, 0.22f) : Color.white;
            string tierStr = isMilestone ? $"* T{tier} *" : $"T{tier}";
            int tierFontSize = isMilestone ? 20 : 16;
            AddLayoutText(card, "Num", tierStr, tierFontSize, tierNumCol, FontStyles.Bold, isMilestone ? 28 : 24);

            // Free reward section
            var freeSection = MakePanel(card, "Free",
                reached ? new Color(0.3f, 0.65f, 0.3f, 0.3f) : new Color(0.4f, 0.4f, 0.4f, 0.2f));
            AddLE(freeSection.gameObject, 100);

            var freeVL = freeSection.gameObject.AddComponent<VerticalLayoutGroup>();
            freeVL.spacing = 2;
            freeVL.childAlignment = TextAnchor.MiddleCenter;
            freeVL.childForceExpandWidth = true;
            freeVL.childForceExpandHeight = false;
            freeVL.childControlWidth = true;
            freeVL.childControlHeight = true;
            freeVL.padding = new RectOffset(4, 4, 4, 4);

            AddLayoutText(freeSection, "FH", "FREE", 10, new Color(0.3f, 0.75f, 0.3f), FontStyles.Bold, 14);

            var config = bp.GetTierRewardConfig(tier);
            if (config != null)
            {
                BuildRewardIcons(freeSection, config.freeCoins, config.freeGems, 0);
            }
            else
            {
                AddLayoutText(freeSection, "FR", "?", 16, COL_DISABLED, FontStyles.Bold, 30);
            }

            if (freeClaimed)
            {
                AddLayoutText(freeSection, "FC", "CLAIMED", 10,
                    new Color(0.5f, 0.5f, 0.5f), FontStyles.Italic, 18);
            }
            else if (reached)
            {
                int capturedTier = tier;
                MakeChunkyButton(freeSection, "CLAIM", COL_PLAY, COL_PLAY_SHADOW,
                    Color.white, 10, 26,
                    () =>
                    {
                        bp.ClaimTierReward(capturedTier, (success, error) =>
                        {
                            var feedbackParent = bpTierLabel != null
                                ? bpTierLabel.transform.parent.GetComponent<RectTransform>()
                                : null;
                            if (success)
                            {
                                if (feedbackParent != null)
                                    ShowPurchaseFeedback(feedbackParent, "Reward claimed!", new Color(0.3f, 0.85f, 0.3f));
                                PopulateBattlePassUI();
                            }
                            else
                            {
                                if (feedbackParent != null)
                                    ShowPurchaseFeedback(feedbackParent, error ?? "Claim failed", new Color(0.85f, 0.25f, 0.2f));
                            }
                        });
                    });
            }

            // Premium reward section
            Color premBg = isPremium
                ? (reached ? new Color(0.45f, 0.25f, 0.70f, 0.3f) : new Color(0.45f, 0.25f, 0.70f, 0.15f))
                : new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var premSection = MakePanel(card, "Prem", premBg);
            AddLE(premSection.gameObject, 100);

            var premVL = premSection.gameObject.AddComponent<VerticalLayoutGroup>();
            premVL.spacing = 2;
            premVL.childAlignment = TextAnchor.MiddleCenter;
            premVL.childForceExpandWidth = true;
            premVL.childForceExpandHeight = false;
            premVL.childControlWidth = true;
            premVL.childControlHeight = true;
            premVL.padding = new RectOffset(4, 4, 4, 4);

            Color premLabelCol = isPremium ? new Color(0.55f, 0.28f, 0.85f) : COL_DISABLED;
            AddLayoutText(premSection, "PH", "PREMIUM", 10, premLabelCol, FontStyles.Bold, 14);

            if (config != null)
            {
                BuildRewardIcons(premSection, config.premiumCoins, config.premiumGems, config.premiumTokens);
            }
            else
            {
                AddLayoutText(premSection, "PR", "?", 16, COL_DISABLED, FontStyles.Bold, 30);
            }

            if (!isPremium)
            {
                AddLayoutText(premSection, "Lock", "LOCKED", 10,
                    COL_DISABLED, FontStyles.Italic, 18);
            }
            else if (premClaimed)
            {
                AddLayoutText(premSection, "PC", "CLAIMED", 10,
                    new Color(0.5f, 0.5f, 0.5f), FontStyles.Italic, 18);
            }
            else if (reached)
            {
                int capturedTier = tier;
                MakeChunkyButton(premSection, "CLAIM",
                    new Color(0.55f, 0.28f, 0.85f), new Color(0.35f, 0.15f, 0.60f),
                    Color.white, 10, 26,
                    () =>
                    {
                        bp.ClaimTierReward(capturedTier, (success, error) =>
                        {
                            var feedbackParent = bpTierLabel != null
                                ? bpTierLabel.transform.parent.GetComponent<RectTransform>()
                                : null;
                            if (success)
                            {
                                if (feedbackParent != null)
                                    ShowPurchaseFeedback(feedbackParent, "Premium reward claimed!", new Color(0.55f, 0.28f, 0.85f));
                                PopulateBattlePassUI();
                            }
                            else
                            {
                                if (feedbackParent != null)
                                    ShowPurchaseFeedback(feedbackParent, error ?? "Claim failed", new Color(0.85f, 0.25f, 0.2f));
                            }
                        });
                    });
            }
        }

        private void BuildRewardIcons(RectTransform parent, int coins, int gems, int tokens)
        {
            var row = MakePanel(parent, "RewardIcons", Color.clear);
            AddLE(row.gameObject, 30);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 4;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandWidth = false;
            hl.childForceExpandHeight = false;
            hl.childControlWidth = true;
            hl.childControlHeight = true;

            if (coins > 0)
            {
                MakeCurrencyIcon(row, "coins", 14);
                var t = MakeText(row, "C", $"{coins}", 11, COL_BTN_TEXT, FontStyles.Bold);
                t.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            }
            if (gems > 0)
            {
                MakeCurrencyIcon(row, "gems", 14);
                var t = MakeText(row, "G", $"{gems}", 11, new Color(0.20f, 0.55f, 0.85f), FontStyles.Bold);
                t.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            }
            if (tokens > 0)
            {
                MakeCurrencyIcon(row, "tokens", 14);
                var t = MakeText(row, "T", $"{tokens}", 11, new Color(0.30f, 0.75f, 0.30f), FontStyles.Bold);
                t.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
            }
            if (coins == 0 && gems == 0 && tokens == 0)
            {
                AddLayoutText(row, "None", "\u2014", 11, COL_DISABLED, FontStyles.Normal, 18);
            }
        }
    }
}
