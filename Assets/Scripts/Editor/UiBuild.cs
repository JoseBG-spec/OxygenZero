using MarsOutpost.Core;
using MarsOutpost.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MarsOutpost.EditorTools
{
    /// <summary>
    /// Small, explicit uGUI construction helpers used by the editor builder. Keeping
    /// layout numbers in one place makes the mobile reference resolution easy to audit.
    /// </summary>
    public static class UiBuild
    {
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;

        public static GameObject NewUi(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        public static RectTransform Rect(GameObject go) => (RectTransform)go.transform;

        public static void Stretch(RectTransform rect, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void TopLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void TopRight(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-x, -y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void BottomLeft(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void BottomRight(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static void Center(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        public static Image AddImage(GameObject go, Color color, Sprite sprite = null, bool sliced = false, bool raycast = false)
        {
            var image = go.AddComponent<Image>();
            image.color = color;
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = raycast;
            return image;
        }

        public static Image AddPanel(GameObject go, PanelRole role, bool rounded, UiTheme theme)
        {
            var image = go.AddComponent<Image>();
            image.sprite = rounded ? theme.roundedPanel : null;
            image.type = rounded ? Image.Type.Sliced : Image.Type.Simple;
            image.color = theme.ColorForRole(role);
            image.raycastTarget = false;

            var themed = go.AddComponent<ThemedPanel>();
            themed.role = role;
            themed.useRoundedSprite = rounded;
            return image;
        }

        public static TextMeshProUGUI AddText(
            GameObject go,
            string text,
            TextRole role,
            TextAlignmentOptions alignment,
            UiTheme theme,
            bool secondary = false,
            bool italic = false)
        {
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.font = theme.font;
            tmp.fontSize = theme.bodySize;
            tmp.color = secondary ? theme.textSecondary : theme.textPrimary;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            tmp.richText = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            if (italic)
            {
                tmp.fontStyle = FontStyles.Italic;
            }

            var themed = go.AddComponent<ThemedText>();
            themed.role = role;
            themed.useSecondaryColor = secondary;
            return tmp;
        }

        public static TextMeshProUGUI Text(
            Transform parent,
            string name,
            string text,
            TextRole role,
            TextAlignmentOptions alignment,
            UiTheme theme,
            bool secondary = false,
            bool italic = false)
        {
            GameObject go = NewUi(name, parent);
            return AddText(go, text, role, alignment, theme, secondary, italic);
        }

        public static Button AddButton(GameObject root, string label, bool primary, UiTheme theme, out TextMeshProUGUI labelText)
        {
            var image = root.AddComponent<Image>();
            image.sprite = theme.roundedButton;
            image.type = Image.Type.Sliced;
            image.color = primary ? theme.primary : theme.panelAlt;
            image.raycastTarget = true;

            var button = root.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };

            GameObject labelGo = NewUi("Label", root.transform);
            Stretch(Rect(labelGo), 18f, 8f, 18f, 8f);
            labelText = AddText(labelGo, label, TextRole.Button, TextAlignmentOptions.Center, theme);
            var themedLabel = labelGo.GetComponent<ThemedText>();
            if (themedLabel != null)
            {
                themedLabel.enabled = false;
            }

            var themed = root.AddComponent<ThemedButton>();
            themed.primary = primary;
            themed.background = image;
            themed.label = labelText;

            return button;
        }

        public static Button Button(
            Transform parent,
            string name,
            string label,
            bool primary,
            UiTheme theme,
            out TextMeshProUGUI labelText)
        {
            GameObject root = NewUi(name, parent);
            return AddButton(root, label, primary, theme, out labelText);
        }

        public static LayoutElement Layout(GameObject go, float minHeight, float preferredHeight, float flexibleWidth = 1f)
        {
            var layout = go.AddComponent<LayoutElement>();
            layout.minHeight = minHeight;
            layout.preferredHeight = preferredHeight;
            layout.flexibleHeight = 0f;
            layout.flexibleWidth = flexibleWidth;
            return layout;
        }

        public static VerticalLayoutGroup VerticalLayout(GameObject go, float spacing, int padding)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childAlignment = TextAnchor.UpperCenter;
            return layout;
        }

        public static ScrollRect ScrollView(Transform parent, string name, out RectTransform content)
        {
            GameObject root = NewUi(name, parent);
            var scroll = root.AddComponent<ScrollRect>();

            GameObject viewport = NewUi("Viewport", root.transform);
            var viewportRect = Rect(viewport);
            Stretch(viewportRect, 0f, 0f, 18f, 0f);
            viewport.AddComponent<RectMask2D>();

            GameObject scrollbarGo = NewUi("Scrollbar", root.transform);
            var scrollbarRect = Rect(scrollbarGo);
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.offsetMin = new Vector2(-12f, 0f);
            scrollbarRect.offsetMax = Vector2.zero;
            Image scrollbarTrack = AddImage(scrollbarGo, new Color(1f, 1f, 1f, 0.10f), null);
            scrollbarTrack.raycastTarget = false;
            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            GameObject handle = NewUi("Handle", scrollbarGo.transform);
            var handleRect = Rect(handle);
            Stretch(handleRect);
            Image handleImage = AddImage(handle, new Color(1f, 1f, 1f, 0.45f), null);
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;

            GameObject contentGo = NewUi("Content", viewport.transform);
            var contentRect = Rect(contentGo);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            VerticalLayout(contentGo, 18f, 8);
            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.1f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;
            scroll.scrollSensitivity = 30f;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

            content = contentRect;
            return scroll;
        }

        public static GameObject FullScreen(Transform parent, string name)
        {
            GameObject go = NewUi(name, parent);
            Stretch(Rect(go));
            return go;
        }

        public static void SetActive(GameObject go, bool active)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
}
