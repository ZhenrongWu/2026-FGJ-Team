using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Editor
{
    public sealed class GameplayHudBuilder
    {
        public const string PlayerItemsName = "PlayerItems";
        public const string MonsterItemsName = "MonsterItems";
        public const string PlayerOxygenName = "PlayerOxygen";
        public const string MonsterOxygenName = "MonsterOxygen";
        public const string ItemTooltipName = "ItemTooltip";
        public const string ControlPanelName = "ControlPanel";
        public const string ControlFrameName = "Frame";
        public const string BidRowName = "BidRow";
        public const string MatchLogName = "MatchLog";
        public const string LogDecorationName = "LogDecoration";
        public const string LogDecorationSpritePath = "Assets/Art/Sprites/UI/HUD/MatchLog_Skull.png";
        private static readonly Vector2 LogDecorationPosition = Vector2.zero;
        private static readonly Vector2 LogDecorationSize = new Vector2(110f, 141f);
        private const float ItemBarX = 420f;
        private const float ItemBarY = -80f;
        private const float OxygenX = 75f;
        private const float OxygenY = -171f;
        private const float ItemSlotSpacing = 9f;
        private static readonly Vector2 ItemBarSize = new Vector2(417f, 128f);
        private const int ItemBarPaddingLeft = 14;
        private const int ItemBarPaddingOther = 15;
        private static readonly Vector2 ItemSlotSize = new Vector2(90f, 90f);
        private static readonly Vector2 ItemTooltipPosition = new Vector2(ItemBarX, -170f);
        private static readonly Vector2 ItemTooltipSize = new Vector2(600f, 48f);
        private static readonly Vector2 OxygenTankSize = new Vector2(90f, 302f);
        private static readonly Vector2 OxygenBubbleInsetMin = new Vector2(12f, 42f);
        private static readonly Vector2 OxygenBubbleInsetMax = new Vector2(-14f, -12f);
        private static readonly Vector2 OxygenBubbleSize = new Vector2(48f, 48f);
        private static readonly Color SpentBubble = new Color(1f, 1f, 1f, 0.25f);
        private const float DecisionRowY = -96f;
        private const float BidRowY = -180f;
        private static readonly Vector2 ControlPanelPosition = new Vector2(350f, 255f);
        private static readonly Vector2 ControlPanelSize = new Vector2(640f, 450f);
        private static readonly Vector2 ControlFrameSize = new Vector2(560f, 277f);
        private static readonly Vector2 StatusPosition = new Vector2(350f, 524f);
        private static readonly Vector2 StatusSize = new Vector2(640f, 64f);
        private static readonly Vector2 ErrorPosition = new Vector2(350f, 576f);
        private static readonly Vector2 ErrorSize = new Vector2(640f, 36f);
        private static readonly Vector2 DecisionButtonSize = new Vector2(230f, 64f);
        private static readonly Vector2 ContinueButtonSize = new Vector2(530f, 64f);
        private static readonly Vector2 BidRowSize = new Vector2(560f, 78f);
        private static readonly Vector2 TopLeft = new Vector2(0f, 1f);
        private static readonly Vector2 TopRight = new Vector2(1f, 1f);
        private static readonly Vector2 BottomLeft = new Vector2(0f, 0f);
        private static readonly Vector2 BottomRight = new Vector2(1f, 0f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);

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
                playerOxygen = BuildOxygen(canvasObject.transform, PlayerOxygenName, TopLeft, OxygenX),
                monsterOxygen = BuildOxygen(canvasObject.transform, MonsterOxygenName, TopRight, -OxygenX)
            };
            BuildItemBars(canvasObject.transform, parts);
            BuildControlPanel(canvasObject.transform, parts);
            parts.logView = BuildLog(canvasObject.transform);

            var hud = canvasObject.AddComponent<LiarDiceHud>();
            hud.SetParts(parts);
            hud.SetAudio(AudioAssets.GameAudio);
            hud.SetItemIcons(HudArt.ItemIcons);
            return hud;
        }

        private OxygenGauge BuildOxygen(Transform canvas, string name, Vector2 anchor, float x)
        {
            var panel = _ui.CreateImage(name, canvas, Color.white);
            panel.sprite = HudArt.Load(HudArt.OxygenTank);
            panel.preserveAspect = true;
            _ui.Place(panel.rectTransform, anchor, new Vector2(x, OxygenY), OxygenTankSize);

            var segments = _ui.CreateRect("Segments", panel.transform);
            _ui.Stretch(segments);
            segments.offsetMin = OxygenBubbleInsetMin;
            segments.offsetMax = OxygenBubbleInsetMax;
            var layout = segments.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.LowerCenter;
            layout.reverseArrangement = true;
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var template = _ui.CreateImage("SegmentTemplate", panel.transform, Color.white);
            template.sprite = HudArt.Load(HudArt.OxygenBubble);
            template.preserveAspect = true;
            template.raycastTarget = false;
            template.rectTransform.sizeDelta = OxygenBubbleSize;
            template.gameObject.SetActive(false);

            var gauge = panel.gameObject.AddComponent<OxygenGauge>();
            gauge.Configure(segments, template);
            gauge.SetSegmentColors(Color.white, SpentBubble);
            return gauge;
        }

        public void ApplyArt(LiarDiceHud hud)
        {
            var parts = hud.Parts;
            parts.playerOxygen = RebuildOxygen(hud.transform, parts.playerOxygen, PlayerOxygenName, TopLeft, OxygenX);
            parts.monsterOxygen = RebuildOxygen(hud.transform, parts.monsterOxygen, MonsterOxygenName, TopRight,
                -OxygenX);
            hud.SetParts(parts);
            RebuildItemBars(hud);
            ApplyControlPanelArt(hud);
            hud.SetItemIcons(HudArt.ItemIcons);
        }

        private OxygenGauge RebuildOxygen(Transform canvas, OxygenGauge old, string name, Vector2 anchor, float x)
        {
            var siblingIndex = old != null ? old.transform.GetSiblingIndex() : -1;
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
            var gauge = BuildOxygen(canvas, name, anchor, x);
            if (siblingIndex >= 0)
                gauge.transform.SetSiblingIndex(siblingIndex);
            return gauge;
        }

        public void RebuildItemBars(LiarDiceHud hud)
        {
            var parts = hud.Parts;
            foreach (var name in new[] { PlayerItemsName, MonsterItemsName, ItemTooltipName })
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
            parts.itemTooltip = _ui.CreateText(ItemTooltipName, canvas, string.Empty, 22, _ui.Bone, TextAnchor.UpperLeft);
            _ui.Place(parts.itemTooltip.rectTransform, TopLeft, ItemTooltipPosition, ItemTooltipSize);
        }

        private ItemBarView BuildItemBar(Transform canvas, string name, Vector2 anchor, float x, TextAnchor alignment)
        {
            var row = _ui.CreateImage(name, canvas, Color.white);
            row.sprite = HudArt.Load(HudArt.ItemBarFrame);
            _ui.Place(row.rectTransform, anchor, new Vector2(x, ItemBarY), ItemBarSize);

            var slots = _ui.CreateRect("Slots", row.transform);
            _ui.Stretch(slots);
            var layout = slots.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.padding = new RectOffset(ItemBarPaddingLeft, ItemBarPaddingOther, ItemBarPaddingOther,
                ItemBarPaddingOther);
            layout.spacing = ItemSlotSpacing;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var template = _ui.CreateButton("ItemSlotTemplate", row.transform, string.Empty, Color.clear,
                _ui.TextLight, 48);
            ((RectTransform)template.transform).sizeDelta = ItemSlotSize;
            var icon = _ui.CreateImage(ItemBarView.IconName, template.transform, Color.white);
            _ui.Stretch(icon.rectTransform);
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.transform.SetAsFirstSibling();
            template.targetGraphic = icon;
            template.gameObject.SetActive(false);

            var bar = row.gameObject.AddComponent<ItemBarView>();
            bar.Configure(slots, template);
            return bar;
        }

        private void BuildControlPanel(Transform canvas, LiarDiceHudParts parts)
        {
            var panel = _ui.CreateImage(ControlPanelName, canvas, Color.clear);
            panel.raycastTarget = false;

            parts.statusText = _ui.CreateText("Status", canvas, string.Empty, 22, _ui.Muted, TextAnchor.LowerLeft);
            parts.errorText = _ui.CreateText("Error", canvas, string.Empty, 22, _ui.Danger, TextAnchor.LowerLeft);

            parts.believeButton = _ui.CreateButton("BelieveButton", panel.transform, "相信", _ui.Glow, _ui.Ink);
            parts.bluffButton = _ui.CreateButton("BluffButton", panel.transform, "吹牛！開", _ui.Danger, _ui.TextLight);

            parts.continueButton = _ui.CreateButton("ContinueButton", panel.transform, LiarDiceText.NextRoundLabel,
                _ui.Highlight, _ui.Ink);
            parts.continueLabel = parts.continueButton.GetComponentInChildren<Text>();
            parts.continueButton.gameObject.SetActive(false);

            var bidRow = _ui.CreateImage(BidRowName, panel.transform, _ui.PanelDark);

            parts.quantityInput = _ui.CreateInputField("QuantityInput", bidRow.transform, "數量", 2);
            _ui.Place((RectTransform)parts.quantityInput.transform, Center, new Vector2(-220f, 0f), new Vector2(84f, 56f));
            PlaceLabel(bidRow.transform, "個", -150f);
            parts.faceInput = _ui.CreateInputField("FaceInput", bidRow.transform, "1～6", 1);
            _ui.Place((RectTransform)parts.faceInput.transform, Center, new Vector2(-80f, 0f), new Vector2(84f, 56f));
            PlaceLabel(bidRow.transform, "點", -10f);

            parts.submitButton = _ui.CreateButton("SubmitButton", bidRow.transform, "送出（Enter）", _ui.Highlight,
                _ui.Ink, 24);
            _ui.Place((RectTransform)parts.submitButton.transform, Center, new Vector2(160f, 0f), new Vector2(210f, 56f));

            BuildControlFrame(panel.transform);
            LayOutControlPanel(panel.rectTransform, parts);
        }

        public void ApplyControlPanelArt(LiarDiceHud hud)
        {
            var panel = hud.transform.Find(ControlPanelName);
            var background = panel.GetComponent<Image>();
            background.color = Color.clear;
            background.raycastTarget = false;
            var old = panel.Find(ControlFrameName);
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
            BuildControlFrame(panel);
            LayOutControlPanel((RectTransform)panel, hud.Parts);
        }

        private void BuildControlFrame(Transform panel)
        {
            var frame = _ui.CreateImage(ControlFrameName, panel, Color.white);
            frame.sprite = HudArt.Load(HudArt.ControlPanelFrame);
            frame.preserveAspect = true;
            frame.raycastTarget = false;
            frame.transform.SetAsFirstSibling();
        }

        private void LayOutControlPanel(RectTransform panel, LiarDiceHudParts parts)
        {
            _ui.Place(panel, BottomLeft, ControlPanelPosition, ControlPanelSize);
            var frame = (RectTransform)panel.Find(ControlFrameName);
            _ui.Place(frame, TopCenter, new Vector2(0f, -ControlFrameSize.y * 0.5f), ControlFrameSize);

            _ui.Place(parts.statusText.rectTransform, BottomLeft, StatusPosition, StatusSize);
            _ui.Place(parts.errorText.rectTransform, BottomLeft, ErrorPosition, ErrorSize);

            _ui.Place((RectTransform)parts.believeButton.transform, Center, new Vector2(-150f, DecisionRowY),
                DecisionButtonSize);
            _ui.Place((RectTransform)parts.bluffButton.transform, Center, new Vector2(150f, DecisionRowY),
                DecisionButtonSize);
            _ui.Place((RectTransform)parts.continueButton.transform, Center, new Vector2(0f, DecisionRowY),
                ContinueButtonSize);
            _ui.Place((RectTransform)parts.quantityInput.transform.parent, Center, new Vector2(0f, BidRowY),
                BidRowSize);
        }

        private void PlaceLabel(Transform parent, string content, float x)
        {
            var label = _ui.CreateText($"Label_{content}", parent, content, 32, _ui.TextLight, style: FontStyle.Bold);
            _ui.Place(label.rectTransform, Center, new Vector2(x, 0f), new Vector2(56f, 56f));
        }

        public void RebuildLogDecoration(LiarDiceHud hud)
        {
            var log = hud.transform.Find(MatchLogName);
            var old = log.Find(LogDecorationName);
            if (old != null)
                Object.DestroyImmediate(old.gameObject);
            BuildLogDecoration(log);
        }

        private void BuildLogDecoration(Transform log)
        {
            var decoration = _ui.CreateImage(LogDecorationName, log, Color.white);
            decoration.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(LogDecorationSpritePath);
            decoration.preserveAspect = true;
            decoration.raycastTarget = false;
            _ui.Place(decoration.rectTransform, TopCenter, LogDecorationPosition, LogDecorationSize);
        }

        private MatchLogView BuildLog(Transform canvas)
        {
            var panel = _ui.CreateImage(MatchLogName, canvas, _ui.Panel);
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

            BuildLogDecoration(panel.transform);

            var logView = panel.gameObject.AddComponent<MatchLogView>();
            logView.Configure(content, scroll);
            return logView;
        }
    }
}
