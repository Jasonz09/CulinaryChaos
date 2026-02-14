using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IOChef.UI
{
    public partial class MainMenuUI : MonoBehaviour
    {
        // Minimal helper implementations to satisfy compile-time references.

        private RectTransform MakePanel(RectTransform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            return go.GetComponent<RectTransform>();
        }

        private RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        private RectTransform MakeText(RectTransform parent, string name, string text, int size, Color color, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.fontSize = size;
                tmp.color = color;
                tmp.fontStyle = style;
                tmp.textWrappingMode = TextWrappingModes.Normal;
            }
            return rt;
        }

        private void AddLE(GameObject go, int height)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            le.preferredHeight = height;
        }

        private void AddLE(GameObject go, int width, int height)
        {
            var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (width >= 0) le.preferredWidth = width;
            if (height >= 0) le.preferredHeight = height;
        }

        private void AddLayoutText(RectTransform parent, string name, string text, int size, Color color, FontStyles style, int height)
        {
            var rt = MakeText(parent, name, text, size, color, style);
            AddLE(rt.gameObject, height);
        }

        private TextMeshProUGUI MakeChunkyButtonWithLabel(RectTransform parent, string label, Color face, Color shadow, Color textColor, int fontSize, int height, System.Action onClick)
        {
            MakeChunkyButton(parent, label, face, shadow, textColor, fontSize, height, onClick);
            var child = parent.GetChild(parent.childCount - 1);
            return child.GetComponentInChildren<TextMeshProUGUI>();
        }

        private static Sprite _roundedSprite;

        private Sprite GetRoundedSprite()
        {
            if (_roundedSprite != null) return _roundedSprite;
            int size = 64; 
            int r = 16;
            var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
            Color[] fill = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = x + 0.5f;
                    float v = y + 0.5f;
                    // Distance to nearest corner's center
                    float dist = 0f;
                    if (x < r && y < r) dist = Vector2.Distance(new Vector2(u, v), new Vector2(r, r));
                    else if (x >= size - r && y < r) dist = Vector2.Distance(new Vector2(u, v), new Vector2(size - r, r));
                    else if (x < r && y >= size - r) dist = Vector2.Distance(new Vector2(u, v), new Vector2(r, size - r));
                    else if (x >= size - r && y >= size - r) dist = Vector2.Distance(new Vector2(u, v), new Vector2(size - r, size - r));
                    
                    float alpha = 1f;
                    if (dist > r) alpha = 0f;
                    else if (dist > r - 1f) alpha = 1f - (dist - (r - 1f));

                    fill[y * size + x] = new Color(1, 1, 1, alpha);
                }
            }
            tex.SetPixels(fill);
            tex.Apply();
            _roundedSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            return _roundedSprite;
        }

        private RectTransform CreateMenuTextButton(RectTransform parent, string label, int fontSize, Color color, System.Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            
            // Text object
            var tRT = MakeText(rt, "Content", label, fontSize, color, FontStyles.Bold);
            var tmp = tRT.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            Stretch(tRT);

            // Button component (target the Text for color tint)
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = tmp;
            btn.onClick.AddListener(() => onClick?.Invoke());
            
            // Orange click effect
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white; // Multiplies with the base text color
            cb.highlightedColor = new Color(1f, 1f, 1f, 1f); 
            cb.pressedColor = new Color(1f, 0.6f, 0.0f); // Orange tint when pressed
            cb.selectedColor = cb.normalColor;
            cb.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            btn.colors = cb;

            return rt;
        }

        private void MakeChunkyButton(RectTransform parent, string label, Color face, Color shadow, Color textColor, int fontSize, int height, System.Action onClick)
        {
            var go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            
            var img = go.GetComponent<Image>();
            img.sprite = GetRoundedSprite(); // Use rounded sprite
            img.type = Image.Type.Sliced;
            
            // IMPORTANT: Set base image color to WHITE so ColorBlock can control the color fully
            img.color = Color.white;

            // Neumorphic Soft Shadows
            var s1 = go.AddComponent<Shadow>();
            s1.effectColor = new Color(shadow.r, shadow.g, shadow.b, 0.5f);
            s1.effectDistance = new Vector2(3, -3);

            var s2 = go.AddComponent<Shadow>();
            s2.effectColor = new Color(1f, 1f, 1f, 0.3f);
            s2.effectDistance = new Vector2(-2, 2);

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());
            
            // Button Colors - Orange Click Effect
            ColorBlock cb = btn.colors;
            cb.normalColor = face;
            cb.highlightedColor = face * 1.1f; // Slightly lighter on hover
            cb.pressedColor = new Color(1f, 0.5f, 0.0f); // Orange click!
            cb.selectedColor = face;
            cb.disabledColor = Color.gray;
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.1f;
            btn.colors = cb;
            
            // Text Label
            var txtRT = MakeText(rt, "Label", label, fontSize, textColor, FontStyles.Bold);
            Stretch(txtRT); txtRT.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            
            // Layout
            AddLE(rt.gameObject, height);
        }

        private Slider MakeSlider(RectTransform parent, string name, float value)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
            go.transform.SetParent(parent, false);
            var slider = go.GetComponent<Slider>();
            slider.value = value;
            return slider;
        }

        private RectTransform MakeModernButton(RectTransform parent, string label, string spritePath, Color face, Color border, int fontSize, int height, System.Action onClick, bool big)
        {
            MakeChunkyButton(parent, label, face, border, Color.white, fontSize, height, onClick);
            return parent.GetChild(parent.childCount - 1).GetComponent<RectTransform>();
        }

        private void AddHoverScale(GameObject go, float scale = 1.05f)
        {
            // no-op placeholder
        }

        private void MakeColorDot(RectTransform parent, Color color, int size)
        {
            var rt = MakePanel(parent, "Dot", color);
            rt.sizeDelta = new Vector2(size, size);
        }

        private TextMeshProUGUI MakeCurrencyLabel(RectTransform parent, string label, string iconKey, string initialValue, int fontSize, Color color, int height)
        {
            var row = new GameObject("CurrRow", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(parent, false);
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 6; hl.childAlignment = TextAnchor.MiddleCenter; hl.childForceExpandHeight = false; hl.childForceExpandWidth = false;

            MakeCurrencyIcon(row, iconKey, fontSize);
            var txt = MakeText(row, label + "Val", initialValue, fontSize, color, FontStyles.Normal).GetComponent<TextMeshProUGUI>();
            AddLE(row.gameObject, height);
            return txt;
        }

        private void MakeCurrencyIcon(RectTransform parent, string key, int size)
        {
            var rt = MakePanel(parent, "Icon_" + key, Color.clear);
            var imgGO = new GameObject("I", typeof(RectTransform), typeof(TextMeshProUGUI));
            imgGO.transform.SetParent(rt, false);
            var txt = imgGO.GetComponent<TextMeshProUGUI>();
            txt.text = key switch { "coins" => "◉", "gems" => "◆", "tokens" => "✦", _ => "●" };
            txt.fontSize = size; txt.color = Color.white; txt.alignment = TextAlignmentOptions.Center;
            rt.sizeDelta = new Vector2(size + 8, size + 8);
        }

        private void ShowPurchaseFeedback(Transform parent, string message, Color color)
        {
            if (parent == null) return;
            var go = new GameObject("Feedback", typeof(RectTransform)).GetComponent<RectTransform>();
            go.SetParent(parent, false);
            var tmp = go.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = message;
            tmp.color = color;
            tmp.fontSize = 18;
            tmp.alignment = TextAlignmentOptions.Center;
            var le = go.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 28;
            Object.Destroy(go.gameObject, 2.5f);
        }

        private void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            }
        }

        private void CenterBox(RectTransform rt, int w, int h)
        {
            rt.sizeDelta = new Vector2(w, h);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = rt.anchorMin;
            rt.anchoredPosition = Vector2.zero;
        }

        private RectTransform AnchorTop(RectTransform rt, Vector2 size, Vector2 offset)
        {
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            return rt;
        }

        private void AddSkew(GameObject go, float x)
        {
            var s = go.GetComponent<UISkew>() ?? go.AddComponent<UISkew>();
            s.skewX = x;
        }

        /// <summary>
        /// Creates a button with a skewed parallelogram shape.
        /// </summary>
        private RectTransform MakeSkewedButton(RectTransform parent, string label, Color face, Color shadow, Color textColor, int fontSize, int width, int height, System.Action onClick, float skewX = -0.2f)
        {
            var go = new GameObject("BtnSkew_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            LayoutElement le = go.AddComponent<LayoutElement>();
            if (width > 0) le.preferredWidth = width;
            if (height > 0) le.preferredHeight = height;

            var img = go.GetComponent<Image>();
            // img.color = face; // Handled below with ColorBlock logic
            
            AddSkew(go, skewX);
            
            // Shadow
            if (shadow != Color.clear)
            {
                var s = go.AddComponent<Shadow>();
                s.effectColor = shadow;
                s.effectDistance = new Vector2(2, -2);
            }

            var btn = go.GetComponent<Button>();
            btn.onClick.AddListener(() => onClick?.Invoke());

            // Orange Click Effect
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f); 
            cb.pressedColor = new Color(1f, 0.6f, 0.0f); // Orange tint
            cb.selectedColor = Color.white;
            cb.colorMultiplier = 1f;
            
            // Set base colors
            img.color = Color.white;
            cb.normalColor = face;
            btn.colors = cb;

            // Text
            var tRT = MakeText(go.GetComponent<RectTransform>(), "Label", label, fontSize, textColor, FontStyles.Bold);
            var tmp = tRT.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            Stretch(tRT);

            return go.GetComponent<RectTransform>();
        }
    }
}
