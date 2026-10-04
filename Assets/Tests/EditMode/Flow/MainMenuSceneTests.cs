using System.Linq;
using FGJ.MainMenu;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FGJ.Tests.EditMode.Flow
{
    public class MainMenuSceneTests
    {
        private const string ScenePath = "Assets/Scenes/MainMenu.unity";
        private const string BackgroundSpritePath = "Assets/Art/Sprites/Backgrounds/MainMenu/MainMenu_Background.png";

        private static readonly Vector2 StartSlotPosition = new Vector2(611f, -255f);
        private static readonly Vector2 ExitSlotPosition = new Vector2(573f, -412f);

        private Scene _scene;
        private Transform _canvas;
        private MainMenuController _menu;

        [SetUp]
        public void SetUp()
        {
            _scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            _menu = _scene.GetRootGameObjects()
                .Select(root => root.GetComponentInChildren<MainMenuController>(true))
                .First(menu => menu != null);
            _canvas = _menu.transform;
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.CloseScene(_scene, true);
        }

        [Test]
        public void MainMenu_ButtonsSitOnBackgroundSoTheyScaleWithTheArt()
        {
            var background = _canvas.Find("Background");

            Assert.IsNotNull(background);
            Assert.AreSame(background, StartButton.transform.parent);
            Assert.AreSame(background, ExitButton.transform.parent);
        }

        [Test]
        public void MainMenu_ButtonsAlignWithSlotsInBackgroundArt()
        {
            Assert.AreEqual(StartSlotPosition, ((RectTransform)StartButton.transform).anchoredPosition);
            Assert.AreEqual(ExitSlotPosition, ((RectTransform)ExitButton.transform).anchoredPosition);
        }

        private Button StartButton => SerializedButton("startButton");

        private Button ExitButton => SerializedButton("exitButton");

        private Button SerializedButton(string field)
        {
            return (Button)new SerializedObject(_menu).FindProperty(field).objectReferenceValue;
        }

        [Test]
        public void MainMenu_BackgroundShowsMainMenuArt()
        {
            var image = _canvas.Find("Background").GetComponent<Image>();

            Assert.IsNotNull(image.sprite, $"找不到 {BackgroundSpritePath}");
            Assert.AreSame(AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundSpritePath), image.sprite);
            Assert.IsFalse(image.raycastTarget);
        }

        [Test]
        public void MainMenu_BackgroundCoversScreenWithoutStretching()
        {
            var fitter = _canvas.Find("Background").GetComponent<AspectRatioFitter>();

            Assert.IsNotNull(fitter);
            Assert.AreEqual(AspectRatioFitter.AspectMode.EnvelopeParent, fitter.aspectMode);
            Assert.AreEqual(16f / 9f, fitter.aspectRatio, 1e-4f);
        }
    }
}
