using FGJ.LiarDice.UI;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Editor
{
    public sealed class GameplayHudBuilder
    {
        public const string PlayerItemsName = "PlayerItems";
        public const string MonsterItemsName = "MonsterItems";
        private const float ItemBarX = 420f;
        private static readonly Vector2 ItemBarSize = new Vector2(560f, 100f);
        private static readonly Vector2 ItemSlotSize = new Vector2(124f, 80f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);
        private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);

        private readonly UiFactory _ui;

        public GameplayHudBuilder(UiFactory ui)
        {
            _ui = ui;
        }

        public LiarDiceHud Build()
        {
            var canvasObject = new GameObject("GameplayHud", typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var parts = new LiarDiceHudParts
            {
                playerOxygen = BuildOxygen(canvasObject.transform, "PlayerOxygen", TopLeft, 75f),
                monsterOxygen = BuildOxygen(canvasObject.transform, "MonsterOxygen", TopRight, -75f)
            };
            BuildItemBars(canvasObject.transform, parts);
            BuildControlPanel(canvasObject.transform, parts);
            parts.logView = BuildLog(canvasObject.transform);

            var hud = canvasObject.AddComponent<LiarDiceHud>();
            hud.SetParts(parts);
            hud.SetAudio(AudioAssets.GameAudio);
            return hud;
        }

        private OxygenGauge BuildOxygen(Transform canvas, string name, Vector2 anchor, float x)
        {
            var panel = _ui.CreateImage(name, canvas, _ui.Panel);
            _ui.Place(panel.rectTransform, anchor, new Vector2(x, -150f), new Vector2(90f, 260f));

            var segments = _ui.CreateRect("Segments", panel.transform);
            _ui.Stretch(segments);
            segments.offsetMin = new Vector2(10f, 14f);
            segments.offsetMax = new Vector2(-10f, -14f);
            var layout = segments.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.reverseArrangement = true;
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var template = _ui.CreateImage("SegmentTemplate", panel.transform, _ui.Glow);
            template.rectTransform.sizeDelta = new Vector2(56f, 44f);
            template.gameObject.SetActive(false);

            var gauge = panel.gameObject.AddComponent<OxygenGauge>();
            gauge.Configure(segments, template);
            return gauge;
        }

        public void RebuildItemBars(LiarDiceHud hud)
        {
            var parts = hud.Parts;
            foreach (var name in new[] { PlayerItemsName, MonsterItemsName })
            {
                var old = hud.transform.Find(name);
                if (old != null)
                    Object.DestroyImmediate(old.gameObject);
            }
            BuildItemBars(hud.transform, parts);
            hud.SetParts(parts);
        }

        private void BuildItemBars(Transform canvas, LiarDiceHudParts parts)
        {
            parts.playerItems = BuildItemBar(canvas, PlayerItemsName, TopLeft, ItemBarX, TextAnchor.MiddleLeft);
            parts.monsterItems = BuildItemBar(canvas, MonsterItemsName, TopRight, -ItemBarX, TextAnchor.MiddleRight);
        }

        private ItemBarView BuildItemBar(Transform canvas, string name, Vector2 anchor, float x, TextAnchor alignment)
        {
            var row = _ui.CreateImage(name, canvas, _ui.Panel);
            _ui.Place(row.rectTransform, anchor, new Vector2(x, -70f), ItemBarSize);

            var slots = _ui.CreateRect("Slots", row.transform);
            _ui.Stretch(slots);
            slots.offsetMin = new Vector2(10f, 10f);
            slots.offsetMax = new Vector2(-10f, -10f);
            var layout = slots.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var template = _ui.CreateButton("ItemSlotTemplate", row.transform, string.Empty, _ui.PanelDark,
                _ui.TextLight, 22);
            ((RectTransform)template.transform).sizeDelta = ItemSlotSize;
            template.gameObject.AddComponent<Outline>().effectColor = new Color(0.7f, 0.7f, 0.7f, 0.9f);
            template.gameObject.SetActive(false);

            var bar = row.gameObject.AddComponent<ItemBarView>();
            bar.Configure(slots, template);
            return bar;
        }

        private void BuildControlPanel(Transform canvas, LiarDiceHudParts parts)
        {
            var panel = _ui.CreateImage("ControlPanel", canvas, _ui.PanelLight);
            _ui.Place(panel.rectTransform, BottomLeft, new Vector2(350f, 140f), new Vector2(640f, 220f));

            parts.statusText = _ui.CreateText("Status", canvas, string.Empty, 22, _ui.Muted, TextAnchor.LowerLeft);
            _ui.Place(parts.statusText.rectTransform, BottomLeft, new Vector2(350f, 284f), new Vector2(640f, 64f));
            parts.errorText = _ui.CreateText("Error", canvas, string.Empty, 22, _ui.Danger, TextAnchor.LowerLeft);
            _ui.Place(parts.errorText.rectTransform, BottomLeft, new Vector2(350f, 336f), new Vector2(640f, 36f));

            parts.believeButton = _ui.CreateButton("BelieveButton", panel.transform, "相信", _ui.Glow, _ui.Ink);
            _ui.Place((RectTransform)parts.believeButton.transform, Center, new Vector2(-150f, 52f), new Vector2(230f, 64f));
            parts.bluffButton = _ui.CreateButton("BluffButton", panel.transform, "吹牛！開", _ui.Danger, _ui.TextLight);
            _ui.Place((RectTransform)parts.bluffButton.transform, Center, new Vector2(150f, 52f), new Vector2(230f, 64f));

            parts.continueButton = _ui.CreateButton("ContinueButton", panel.transform, LiarDiceText.NextRoundLabel,
                _ui.Highlight, _ui.Ink);
            _ui.Place((RectTransform)parts.continueButton.transform, Center, new Vector2(0f, 52f), new Vector2(530f, 64f));
            parts.continueLabel = parts.continueButton.GetComponentInChildren<Text>();
            parts.continueButton.gameObject.SetActive(false);

            var bidRow = _ui.CreateImage("BidRow", panel.transform, _ui.PanelDark);
            _ui.Place(bidRow.rectTransform, Center, new Vector2(0f, -48f), new Vector2(560f, 78f));

            parts.quantityInput = _ui.CreateInputField("QuantityInput", bidRow.transform, "數量", 2);
            _ui.Place((RectTransform)parts.quantityInput.transform, Center, new Vector2(-220f, 0f), new Vector2(84f, 56f));
            PlaceLabel(bidRow.transform, "個", -150f);
            parts.faceInput = _ui.CreateInputField("FaceInput", bidRow.transform, "1～6", 1);
            _ui.Place((RectTransform)parts.faceInput.transform, Center, new Vector2(-80f, 0f), new Vector2(84f, 56f));
            PlaceLabel(bidRow.transform, "點", -10f);

            parts.submitButton = _ui.CreateButton("SubmitButton", bidRow.transform, "送出（Enter）", _ui.Highlight,
                _ui.Ink, 24);
            _ui.Place((RectTransform)parts.submitButton.transform, Center, new Vector2(160f, 0f), new Vector2(210f, 56f));
        }

        private void PlaceLabel(Transform parent, string content, float x)
        {
            var label = _ui.CreateText($"Label_{content}", parent, content, 32, _ui.TextLight, style: FontStyle.Bold);
            _ui.Place(label.rectTransform, Center, new Vector2(x, 0f), new Vector2(56f, 56f));
        }

        private MatchLogView BuildLog(Transform canvas)
        {
            var panel = _ui.CreateImage("MatchLog", canvas, _ui.Panel);
            _ui.Place(panel.rectTransform, BottomRight, new Vector2(-350f, 160f), new Vector2(640f, 260f));

            var viewport = _ui.CreateImage("Viewport", panel.transform, new Color(0f, 0f, 0f, 0.15f));
            var viewportRect = viewport.rectTransform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(16f, 14f);
            viewportRect.offsetMax = new Vector2(-16f, -14f);
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = _ui.CreateText("Content", viewport.transform, string.Empty, 22, _ui.TextLight,
                TextAnchor.UpperLeft);
            content.supportRichText = true;
            content.lineSpacing = 1.15f;
            var contentRect = content.rectTransform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = new Vector2(8f, 0f);
            contentRect.offsetMax = new Vector2(-8f, 0f);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = panel.gameObject.AddComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            var logView = panel.gameObject.AddComponent<MatchLogView>();
            logView.Configure(content, scroll);
            return logView;
        }
    }
}
