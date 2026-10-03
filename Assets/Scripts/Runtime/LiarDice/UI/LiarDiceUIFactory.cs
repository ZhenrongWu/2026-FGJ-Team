using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public static class LiarDiceUIFactory
    {
        public static readonly Color Background = new Color32(10, 18, 14, 255);
        public static readonly Color Panel = new Color32(20, 34, 26, 235);
        public static readonly Color ResultCard = new Color32(16, 28, 21, 255);
        public static readonly Color Bone = new Color32(232, 220, 192, 255);
        public static readonly Color Ink = new Color32(24, 20, 16, 255);
        public static readonly Color Glow = new Color32(111, 211, 154, 255);
        public static readonly Color Danger = new Color32(217, 83, 79, 255);
        public static readonly Color Highlight = new Color32(242, 191, 76, 255);
        public static readonly Color HiddenDie = new Color32(46, 58, 50, 255);
        public static readonly Color Muted = new Color32(150, 170, 156, 255);

        private const string BuiltinFontName = "LegacyRuntime.ttf";
        private static Font _font;

        public static Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>(BuiltinFontName);

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static RectTransform Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            var image = CreateRect(name, parent).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var text = CreateRect(name, parent).gameObject.AddComponent<Text>();
            text.font = Font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color background, Color labelColor)
        {
            var image = CreateImage(name, parent, background);
            var button = image.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
            button.colors = colors;
            Stretch(CreateText("Label", image.transform, label, 30, labelColor).rectTransform);
            return button;
        }

        public static InputField CreateInputField(string name, Transform parent, string placeholder, int characterLimit)
        {
            var background = CreateImage(name, parent, Bone);
            var input = background.gameObject.AddComponent<InputField>();

            var textComponent = CreateText("Text", background.transform, string.Empty, 40, Ink);
            Stretch(textComponent.rectTransform);
            textComponent.supportRichText = false;

            var placeholderText = CreateText("Placeholder", background.transform, placeholder, 26,
                new Color(Ink.r, Ink.g, Ink.b, 0.45f));
            Stretch(placeholderText.rectTransform);

            input.textComponent = textComponent;
            input.placeholder = placeholderText;
            input.contentType = InputField.ContentType.IntegerNumber;
            input.characterLimit = characterLimit;
            input.lineType = InputField.LineType.SingleLine;
            return input;
        }

        public static DieSlot CreateDie(Transform parent)
        {
            var face = CreateImage("Die", parent, Bone);
            ((RectTransform)face.transform).sizeDelta = new Vector2(110, 110);
            var label = CreateText("Pips", face.transform, string.Empty, 64, Ink);
            Stretch(label.rectTransform);
            return new DieSlot(face, label);
        }
    }

    public sealed class DieSlot
    {
        public Image Face { get; }
        public Text Label { get; }

        public DieSlot(Image face, Text label)
        {
            Face = face;
            Label = label;
        }
    }
}
