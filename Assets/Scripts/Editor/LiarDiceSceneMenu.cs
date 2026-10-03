using System.Linq;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class LiarDiceSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Levels/LiarDiceRoom.unity";
        public const string ConfigPath = "Assets/Data/LiarDice/Level01_LiarDiceConfig.asset";

        [MenuItem("FGJ/Liar Dice/Build Room Scene")]
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
            LiarDiceRoomBuilder.Build(config);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings(ScenePath);
        }

        private static void AddToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == scenePath))
                return;
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
