using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public static class LiarDiceRoomBuilder
    {
        public const string RootName = "LiarDiceRoom";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);
        private static readonly Vector2 Top = new Vector2(0.5f, 1f);
        private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

        public static LiarDiceRoomController Build(LiarDiceConfig config)
        {
            var root = new GameObject(RootName);
            EnsureEventSystem(root.transform);

            var canvas = CreateCanvas(root.transform);
            var parts = new LiarDiceViewParts();
            LiarDiceUIFactory.Stretch(
                LiarDiceUIFactory.CreateImage("Background", canvas.transform, LiarDiceUIFactory.Background).rectTransform);

            BuildMonsterArea(canvas.transform, parts);
            BuildTableArea(canvas.transform, parts);
            BuildPlayerArea(canvas.transform, parts);
            BuildInputPanel(canvas.transform, parts);
            BuildResultPanel(canvas.transform, parts);

            var view = canvas.gameObject.AddComponent<LiarDiceView>();
            view.SetParts(parts);

            var controller = root.AddComponent<LiarDiceRoomController>();
            controller.Configure(config, view);
            return controller;
        }

        private static void EnsureEventSystem(Transform root)
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(root, false);
        }

        private static Canvas CreateCanvas(Transform root)
        {
            var canvasObject = new GameObject("Canvas", typeof(RectTransform));
            canvasObject.transform.SetParent(root, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void BuildMonsterArea(Transform canvas, LiarDiceViewParts parts)
        {
            parts.monsterNameText = LiarDiceUIFactory.CreateText("MonsterName", canvas, LiarDiceText.MonsterName, 44,
                LiarDiceUIFactory.Glow);
            LiarDiceUIFactory.Place(parts.monsterNameText.rectTransform, Top, new Vector2(0, -60), new Vector2(800, 60));

            parts.monsterOxygenText = LiarDiceUIFactory.CreateText("MonsterOxygen", canvas, string.Empty, 32,
                LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(parts.monsterOxygenText.rectTransform, Top, new Vector2(0, -115), new Vector2(800, 50));

            parts.monsterDiceRow = CreateDiceRow("MonsterDice", canvas, Top, new Vector2(0, -210));

            parts.monsterLineText = LiarDiceUIFactory.CreateText("MonsterLine", canvas, string.Empty, 34,
                LiarDiceUIFactory.Bone);
            parts.monsterLineText.fontStyle = FontStyle.Italic;
            LiarDiceUIFactory.Place(parts.monsterLineText.rectTransform, Top, new Vector2(0, -310), new Vector2(1400, 60));
        }

        private static void BuildTableArea(Transform canvas, LiarDiceViewParts parts)
        {
            var table = LiarDiceUIFactory.CreateImage("Table", canvas, LiarDiceUIFactory.Panel);
            LiarDiceUIFactory.Place(table.rectTransform, Center, new Vector2(0, 40), new Vector2(900, 190));

            parts.currentBidText = LiarDiceUIFactory.CreateText("CurrentBid", table.transform, string.Empty, 54,
                LiarDiceUIFactory.Highlight);
            LiarDiceUIFactory.Place(parts.currentBidText.rectTransform, Center, new Vector2(0, 40), new Vector2(860, 70));

            parts.wildStatusText = LiarDiceUIFactory.CreateText("WildStatus", table.transform, string.Empty, 28,
                LiarDiceUIFactory.Glow);
            LiarDiceUIFactory.Place(parts.wildStatusText.rectTransform, Center, new Vector2(0, -20), new Vector2(860, 40));

            parts.turnStatusText = LiarDiceUIFactory.CreateText("TurnStatus", table.transform, string.Empty, 28,
                LiarDiceUIFactory.Muted);
            LiarDiceUIFactory.Place(parts.turnStatusText.rectTransform, Center, new Vector2(0, -62), new Vector2(860, 40));
        }

        private static void BuildPlayerArea(Transform canvas, LiarDiceViewParts parts)
        {
            parts.playerDiceRow = CreateDiceRow("PlayerDice", canvas, Bottom, new Vector2(0, 380));

            parts.playerOxygenText = LiarDiceUIFactory.CreateText("PlayerOxygen", canvas, string.Empty, 32,
                LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(parts.playerOxygenText.rectTransform, Bottom, new Vector2(0, 285), new Vector2(800, 50));
        }

        private static void BuildInputPanel(Transform canvas, LiarDiceViewParts parts)
        {
            var panel = LiarDiceUIFactory.CreateImage("InputPanel", canvas, LiarDiceUIFactory.Panel);
            LiarDiceUIFactory.Place(panel.rectTransform, Bottom, new Vector2(0, 150), new Vector2(1240, 130));
            parts.inputPanel = panel.gameObject;

            var quantityLabel = LiarDiceUIFactory.CreateText("QuantityLabel", panel.transform, "數量", 32,
                LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(quantityLabel.rectTransform, Center, new Vector2(-540, 0), new Vector2(90, 60));
            parts.quantityInput = LiarDiceUIFactory.CreateInputField("QuantityInput", panel.transform, "個數", 2);
            LiarDiceUIFactory.Place((RectTransform)parts.quantityInput.transform, Center, new Vector2(-420, 0),
                new Vector2(140, 80));

            var timesLabel = LiarDiceUIFactory.CreateText("Times", panel.transform, "個", 32, LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(timesLabel.rectTransform, Center, new Vector2(-320, 0), new Vector2(60, 60));

            parts.faceInput = LiarDiceUIFactory.CreateInputField("FaceInput", panel.transform, "1～6", 1);
            LiarDiceUIFactory.Place((RectTransform)parts.faceInput.transform, Center, new Vector2(-220, 0),
                new Vector2(140, 80));
            var faceLabel = LiarDiceUIFactory.CreateText("FaceLabel", panel.transform, "點", 32, LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(faceLabel.rectTransform, Center, new Vector2(-120, 0), new Vector2(60, 60));

            parts.raiseButton = LiarDiceUIFactory.CreateButton("RaiseButton", panel.transform, "相信並加注（Enter）",
                LiarDiceUIFactory.Glow, LiarDiceUIFactory.Ink);
            LiarDiceUIFactory.Place((RectTransform)parts.raiseButton.transform, Center, new Vector2(155, 0),
                new Vector2(330, 80));

            parts.challengeButton = LiarDiceUIFactory.CreateButton("ChallengeButton", panel.transform, "質疑！開",
                LiarDiceUIFactory.Danger, LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place((RectTransform)parts.challengeButton.transform, Center, new Vector2(470, 0),
                new Vector2(250, 80));

            parts.errorText = LiarDiceUIFactory.CreateText("Error", canvas, string.Empty, 28, LiarDiceUIFactory.Danger);
            LiarDiceUIFactory.Place(parts.errorText.rectTransform, Bottom, new Vector2(0, 55), new Vector2(1400, 50));
        }

        private static void BuildResultPanel(Transform canvas, LiarDiceViewParts parts)
        {
            var overlay = LiarDiceUIFactory.CreateImage("ResultPanel", canvas, new Color(0f, 0f, 0f, 0.25f));
            LiarDiceUIFactory.Stretch(overlay.rectTransform);
            parts.resultPanel = overlay.gameObject;

            var card = LiarDiceUIFactory.CreateImage("ResultCard", overlay.transform, LiarDiceUIFactory.ResultCard);
            LiarDiceUIFactory.Place(card.rectTransform, Center, new Vector2(0, 65), new Vector2(1000, 280));

            parts.resultText = LiarDiceUIFactory.CreateText("ResultText", card.transform, string.Empty, 34,
                LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Place(parts.resultText.rectTransform, Center, new Vector2(0, 40), new Vector2(960, 160));

            parts.continueButton = LiarDiceUIFactory.CreateButton("ContinueButton", card.transform,
                LiarDiceText.NextRoundLabel, LiarDiceUIFactory.Highlight, LiarDiceUIFactory.Ink);
            LiarDiceUIFactory.Place((RectTransform)parts.continueButton.transform, Center, new Vector2(0, -90),
                new Vector2(280, 66));
            parts.continueLabel = parts.continueButton.GetComponentInChildren<Text>();

            overlay.gameObject.SetActive(false);
        }

        private static RectTransform CreateDiceRow(string name, Transform canvas, Vector2 anchor, Vector2 position)
        {
            var row = LiarDiceUIFactory.Place(LiarDiceUIFactory.CreateRect(name, canvas), anchor, position,
                new Vector2(1400, 120));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return row;
        }
    }
}
