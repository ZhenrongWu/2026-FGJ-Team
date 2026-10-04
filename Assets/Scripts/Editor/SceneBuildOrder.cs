using System.Collections.Generic;
using System.IO;
using System.Linq;
using FGJ.Flow;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class SceneBuildOrder
    {
        public const string BootstrapScenePath = "Assets/Scenes/Bootstrap.unity";

        private static readonly string[] OrderedSceneNames =
            { SceneNames.Bootstrap, SceneNames.MainMenu, SceneNames.Exploration, SceneNames.Gameplay, SceneNames.Ending };

        [MenuItem("FGJ/Scenes/Build All Scenes")]
        public static void BuildAllFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildAll();
        }

        public static void BuildAll()
        {
            BuildBootstrapScene();
            LiarDiceSceneMenu.BuildRoomScene();
            ExplorationSceneMenu.BuildScene();
            EndingSceneMenu.BuildScene();
            Apply();
        }

        [MenuItem("FGJ/Scenes/Build Bootstrap Scene")]
        public static void BuildBootstrapSceneFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildBootstrapScene();
            Apply();
        }

        public static void BuildBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            new GameObject("GameBootstrap").AddComponent<GameBootstrap>().Configure(FlowAssets.Progress, FlowAssets.Router);
            EditorSceneManager.SaveScene(scene, BootstrapScenePath);
        }

        public static void Apply()
        {
            var ordered = new List<EditorBuildSettingsScene>();
            foreach (var sceneName in OrderedSceneNames)
            {
                var path = FindScenePath(sceneName);
                if (path != null)
                    ordered.Add(new EditorBuildSettingsScene(path, true));
            }

            var orderedPaths = new HashSet<string>(ordered.Select(scene => scene.path));
            var remaining = EditorBuildSettings.scenes.Where(scene =>
                !orderedPaths.Contains(scene.path) && File.Exists(scene.path) &&
                !OrderedSceneNames.Contains(Path.GetFileNameWithoutExtension(scene.path)));

            EditorBuildSettings.scenes = ordered.Concat(remaining).ToArray();
        }

        private static string FindScenePath(string sceneName)
        {
            return AssetDatabase.FindAssets($"{sceneName} t:Scene", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == sceneName);
        }
    }
}
