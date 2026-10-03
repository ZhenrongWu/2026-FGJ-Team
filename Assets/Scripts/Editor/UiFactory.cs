using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Editor
{
    public sealed class UiFactory
    {
        private const string BuiltinFontName = "LegacyRuntime.ttf";

        public readonly Color Background = new Color32(10, 18, 14, 255);
        public readonly Color Panel = new Color32(150, 150, 150, 215);
        public readonly Color PanelDark = new Color32(60, 60, 60, 235);
        public readonly Color PanelLight = new Color32(236, 236, 236, 235);
        public readonly Color Bone = new Color32(232, 220, 192, 255);
        public readonly Color Ink = new Color32(24, 20, 16, 255);
        public readonly Color Glow = new Color32(111, 211, 154, 255);
        public readonly Color Danger = new Color32(217, 83, 79, 255);
        public readonly Color Highlight = new Color32(242, 191, 76, 255);
        public readonly Color TextLight = new Color32(250, 250, 250, 255);
        public readonly Color Muted = new Color32(200, 210, 200, 255);

        private Font _font;

        public Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>(BuiltinFontName);

        public RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public Image CreateImage(string name, Transform parent, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public Text CreateText(string name, Transform parent, string content, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var text = CreateRect(name, parent).gameObject.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public Button CreateButton(string name, Transform parent, string label, Color background, Color labelColor,
            int fontSize = 30)
        {
            var image = CreateImage(name, parent, background);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            button.colors = colors;
            Stretch(CreateText("Label", image.transform, label, fontSize, labelColor, style: FontStyle.Bold)
                .rectTransform);
            return button;
        }

        public InputField CreateInputField(string name, Transform parent, string placeholder, int characterLimit)
        {
            var background = CreateImage(name, parent, Bone);
            var input = background.gameObject.AddComponent<InputField>();

            var textComponent = CreateText("Text", background.transform, string.Empty, 36, Ink);
            Stretch(textComponent.rectTransform);
            textComponent.supportRichText = false;

            var placeholderText = CreateText("Placeholder", background.transform, placeholder, 22,
                new Color(Ink.r, Ink.g, Ink.b, 0.45f));
            Stretch(placeholderText.rectTransform);

            input.textComponent = textComponent;
            input.placeholder = placeholderText;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.characterLimit = characterLimit;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }
    }
}
