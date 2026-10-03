using FGJ.LiarDice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class LiarDiceSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Gameplay.unity";
        public const string ConfigPath = "Assets/Data/LiarDice/LiarDiceConfig_SwampWatcher.asset";
        public const string MonsterPath = "Assets/Data/LiarDice/Monster_SwampWatcher.asset";

        [MenuItem("FGJ/Scenes/Build Gameplay Scene")]
        public static void BuildRoomSceneFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildRoomScene();
        }

        public static void BuildRoomScene()
        {
            DefaultPrefabs.EnsureAll();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = LiarDiceUIFactory.Background;
            }

            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabs.LiarDiceRoomPath);
            var room = (GameObject)PrefabUtility.InstantiatePrefab(roomPrefab);
            var controller = room.GetComponent<LiarDiceRoomController>();

            new GameObject("GameplaySceneFlow").AddComponent<LiarDiceRoomSceneFlow>()
                .Configure(controller, FlowAssets.Progress, FlowAssets.Router);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildOrder.Apply();
        }

        public static MonsterProfile EnsureDefaultMonster()
        {
            var monster = AssetDatabase.LoadAssetAtPath<MonsterProfile>(MonsterPath);
            if (monster != null)
                return monster;

            monster = ScriptableObject.CreateInstance<MonsterProfile>();
            AssetDatabase.CreateAsset(monster, MonsterPath);
            AssetDatabase.SaveAssets();
            return monster;
        }
    }
}
