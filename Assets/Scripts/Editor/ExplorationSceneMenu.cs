using FGJ.Exploration;
using FGJ.LiarDice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FGJ.Editor
{
    public static class ExplorationSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Exploration.unity";
        public const string DataFolder = "Assets/Data/Exploration";
        public const string FirstBuildingPath = DataFolder + "/Building_Tavern.asset";
        public const string RoutePath = DataFolder + "/ExplorationRoute.asset";

        private const float GroundY = 1.5f;
        private const float PlayerStartX = 6f;
        private const float FirstBuildingX = 44f;
        private const string TavernId = "Tavern";
        private const string TavernEnterPrompt = "按 E 進入酒館";
        private const string TavernClearedPrompt = "酒館裡已經安靜了";
        private static readonly Vector2 TavernDoorOffset = new Vector2(0.74f, -0.15f);

        [MenuItem("FGJ/Scenes/Build Exploration Scene")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildScene();
        }

        public static void BuildScene()
        {
            DefaultPrefabs.EnsureAll();
            var route = EnsureRoute();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Object.DestroyImmediate(Object.FindFirstObjectByType<Light>()?.gameObject);
            var camera = SetUpCamera();

            Instantiate(DefaultPrefabs.BackgroundPath);
            var player = Instantiate(DefaultPrefabs.PlayerPath).GetComponent<SideScrollPlayer>();
            player.transform.position = new Vector3(PlayerStartX, GroundY, 0f);
            var hud = Instantiate(DefaultPrefabs.HudPath).GetComponent<ExplorationHud>();

            var follow = camera.gameObject.AddComponent<CameraFollow2D>();
            follow.Configure(player.transform, 0f, DefaultPrefabs.WorldMaxX);

            var controller = new GameObject("ExplorationController").AddComponent<ExplorationController>();
            controller.Configure(player, follow, hud);
            controller.SetServices(FlowAssets.Progress, FlowAssets.Router);
            controller.SetRoute(route, DefaultPrefabs.Load<RoomEntrance>(DefaultPrefabs.EntrancePath),
                GroundY);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildOrder.Apply();
        }

        private static GameObject Instantiate(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        }

        private static Camera SetUpCamera()
        {
            var camera = Camera.main;
            camera.orthographic = true;
            camera.orthographicSize = DefaultPrefabs.ViewHeight * 0.5f;
            camera.transform.SetPositionAndRotation(new Vector3(PlayerStartX, DefaultPrefabs.ViewHeight * 0.5f, -10f),
                Quaternion.identity);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(222, 236, 226, 255);
            return camera;
        }

        private static ExplorationRoute EnsureRoute()
        {
            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets/Data", "Exploration");

            var building = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(FirstBuildingPath);
            if (building == null)
            {
                building = ScriptableObject.CreateInstance<BuildingDefinition>();
                building.Configure(TavernId, TavernEnterPrompt, TavernClearedPrompt, 1.5f,
                    AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(LiarDiceSceneMenu.ConfigPath),
                    LiarDiceSceneMenu.EnsureDefaultMonster());
                AssetDatabase.CreateAsset(building, FirstBuildingPath);
            }

            if (building.Exterior == null)
            {
                building.Configure(TavernId, TavernEnterPrompt, TavernClearedPrompt, building.InteractRange,
                    building.GameplayConfig, building.Monster);
                building.SetExterior(ArtImport.LoadSprite("Tavern"), TavernDoorOffset);
                EditorUtility.SetDirty(building);
            }

            var route = AssetDatabase.LoadAssetAtPath<ExplorationRoute>(RoutePath);
            if (route == null)
            {
                route = ScriptableObject.CreateInstance<ExplorationRoute>();
                route.SetPlacements(new[] { new BuildingPlacement(building, FirstBuildingX) });
                AssetDatabase.CreateAsset(route, RoutePath);
            }

            AssetDatabase.SaveAssets();
            return route;
        }
    }
}
