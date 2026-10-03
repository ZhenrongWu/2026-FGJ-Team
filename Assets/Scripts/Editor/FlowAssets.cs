using FGJ.Flow;
using UnityEditor;
using UnityEngine;

namespace FGJ.Editor
{
    public static class FlowAssets
    {
        public const string Folder = "Assets/Data/Flow";
        public const string ProgressPath = Folder + "/GameProgress.asset";
        public const string RouterPath = Folder + "/SceneRouter.asset";

        public static GameProgress Progress => LoadOrCreate<GameProgress>(ProgressPath);
        public static SceneRouter Router => LoadOrCreate<SceneRouter>(RouterPath);

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            if (!AssetDatabase.IsValidFolder(Folder))
                AssetDatabase.CreateFolder("Assets/Data", "Flow");
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }

    [InitializeOnLoad]
    public static class GameProgressPlayModeReset
    {
        static GameProgressPlayModeReset()
        {
            EditorApplication.playModeStateChanged += ResetOnEnteringPlayMode;
        }

        private static void ResetOnEnteringPlayMode(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode)
                return;

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(GameProgress)}"))
                AssetDatabase.LoadAssetAtPath<GameProgress>(AssetDatabase.GUIDToAssetPath(guid))?.ResetRun();
        }
    }
}
