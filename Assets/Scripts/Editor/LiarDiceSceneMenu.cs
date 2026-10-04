using FGJ.LiarDice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class LiarDiceSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Gameplay.unity";
        public const string ConfigPath = "Assets/Data/LiarDice/LiarDiceConfig_Level01.asset";
        public const string MonsterPath = "Assets/Data/LiarDice/Monster_BlobfishScumbag.asset";

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
            SetUpCamera(Camera.main);
            SetUpLighting();

            var roomPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultPrefabs.LiarDiceRoomPath);
            var room = (GameObject)PrefabUtility.InstantiatePrefab(roomPrefab);
            var controller = room.GetComponent<LiarDiceRoomController>();

            new GameObject("GameplaySceneFlow").AddComponent<LiarDiceRoomSceneFlow>()
                .Configure(controller, FlowAssets.Progress, FlowAssets.Router);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildOrder.Apply();
        }

        private static readonly Vector3 CameraPosition = new Vector3(0f, 1.55f, -1.4f);
        private static readonly Vector3 CameraLookTarget = new Vector3(0f, 0.78f, 0.22f);
        private static readonly Vector3 CandlePosition = new Vector3(0.15f, 1.55f, 0.1f);

        private static void SetUpCamera(Camera camera)
        {
            camera.transform.position = CameraPosition;
            camera.transform.LookAt(CameraLookTarget);
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new UiFactory().Background;
        }

        private static void SetUpLighting()
        {
            var directional = Object.FindFirstObjectByType<Light>();
            directional.color = new Color32(120, 150, 170, 255);
            directional.intensity = 0.25f;
            directional.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            var candle = new GameObject("CandleLight").AddComponent<Light>();
            candle.type = LightType.Point;
            candle.color = new Color32(255, 196, 130, 255);
            candle.intensity = 2.2f;
            candle.range = 3.2f;
            candle.shadows = LightShadows.Soft;
            candle.transform.position = CandlePosition;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color32(28, 36, 32, 255);
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
