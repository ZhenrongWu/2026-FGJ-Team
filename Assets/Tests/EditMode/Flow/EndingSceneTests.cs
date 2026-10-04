using System.Linq;
using FGJ.Editor;
using FGJ.Ending;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FGJ.Tests.EditMode.Flow
{
    public class EndingSceneTests
    {
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<SceneAsset>(EndingSceneMenu.ScenePath),
                $"找不到 {EndingSceneMenu.ScenePath}，請執行 FGJ/Scenes/Build Ending Scene");
            _scene = EditorSceneManager.OpenScene(EndingSceneMenu.ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void TearDown()
        {
            EditorSceneManager.CloseScene(_scene, true);
        }

        private T Find<T>() where T : Component
        {
            return _scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true)).FirstOrDefault();
        }

        [Test]
        public void EndingScene_IsEnabledInBuildSettings()
        {
            Assert.IsTrue(EditorBuildSettings.scenes.Any(scene => scene.path == EndingSceneMenu.ScenePath && scene.enabled));
        }

        [Test]
        public void EndingScene_HasMainMenuButtonWiredToRouter()
        {
            var ending = Find<EndingController>();

            Assert.IsNotNull(ending);
            Assert.IsNotNull(ending.MainMenuButton);
            Assert.AreEqual(EndingSceneMenu.MainMenuLabel, ending.MainMenuButton.GetComponentInChildren<Text>().text);
            Assert.AreSame(FlowAssets.Router, new SerializedObject(ending).FindProperty("router").objectReferenceValue);
        }

        [Test]
        public void EndingScene_HasFullScreenSlotForCg()
        {
            var cg = _scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Image>(true))
                .FirstOrDefault(image => image.name == EndingSceneMenu.CgObjectName);

            Assert.IsNotNull(cg);
            Assert.AreEqual(Vector2.zero, cg.rectTransform.anchorMin);
            Assert.AreEqual(Vector2.one, cg.rectTransform.anchorMax);
        }
    }
}
