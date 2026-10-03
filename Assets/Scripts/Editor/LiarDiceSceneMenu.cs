using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class LiarDiceSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Gameplay.unity";
        public const string ConfigPath = "Assets/Data/LiarDice/Level01_LiarDiceConfig.asset";

        [MenuItem("FGJ/Scenes/Build Gameplay Scene")]
        public static void BuildRoomSceneFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildRoomScene();
        }

        public static void BuildRoomScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = LiarDiceUIFactory.Background;
            }

            var config = AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(ConfigPath);
            var controller = LiarDiceRoomBuilder.Build(config);
            controller.gameObject.AddComponent<LiarDiceRoomSceneFlow>()
                .Configure(controller, FlowAssets.Progress, FlowAssets.Router);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildOrder.Apply();
        }
    }
}
