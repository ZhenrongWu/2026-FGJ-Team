using FGJ.Ending;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace FGJ.Editor
{
    public static class EndingSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Ending.unity";
        public const string CgObjectName = "EndingCG";
        public const string MainMenuLabel = "回到主選單";

        private const string ThanksText = "感謝遊玩";
        private static readonly Vector2 ReferenceResolution = new Vector2(1920, 1080);
        private static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);

        [MenuItem("FGJ/Scenes/Build Ending Scene")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildScene();
            SceneBuildOrder.Apply();
        }

        public static void BuildScene()
        {
            var ui = new UiFactory();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            var canvasObject = new GameObject("EndingCanvas", typeof(RectTransform));
            canvasObject.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var cg = ui.CreateImage(CgObjectName, canvasObject.transform, Color.black);
            ui.Stretch(cg.rectTransform);
            cg.preserveAspect = true;
            cg.raycastTarget = false;

            var thanks = ui.CreateText("Thanks", canvasObject.transform, ThanksText, 72, ui.Bone,
                style: FontStyle.Bold);
            ui.Place(thanks.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(1200f, 140f));

            var mainMenu = ui.CreateButton("MainMenuButton", canvasObject.transform, MainMenuLabel, ui.Danger, ui.TextLight, 36);
            ui.Place((RectTransform)mainMenu.transform, BottomCenter, new Vector2(0f, 140f), new Vector2(360f, 90f));

            var ending = new GameObject("EndingController").AddComponent<EndingController>();
            ending.Configure(FlowAssets.Router, mainMenu);
            ending.SetAudio(AudioAssets.GameAudio);

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
