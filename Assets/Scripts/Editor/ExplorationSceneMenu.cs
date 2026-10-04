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
        public const string RoutePath = DataFolder + "/ExplorationRoute.asset";
        public static int LevelCount => LevelArt.Length;

        private const float GroundY = 1.5f;
        private const float PlayerStartX = 6f;
        private const float FirstBuildingX = 44f;
        private const float BuildingSpacing = 24f;
        private const float InteractRange = 1.5f;
        private const string EnterPrompt = "按 ↑ 進入";
        private const string ClearedPrompt = "這裡已經安靜了";
        private const string ExteriorSuffix = "_Exterior";
        private const string OutlineSuffix = "_Outline";
        private static readonly Vector2 ExteriorScale = new Vector2(2f, 2f);

        private static readonly (string art, Vector2 doorOffset) Tavern = ("Tavern", new Vector2(0.74f, -0.15f));
        private static readonly (string art, Vector2 doorOffset) Mayor = ("Mayor", new Vector2(0.22f, -0.01f));
        private static readonly (string art, Vector2 doorOffset)[] LevelArt = { Tavern, Mayor, Tavern, Tavern };

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

            var placements = new BuildingPlacement[LevelCount];
            for (var i = 0; i < LevelCount; i++)
                placements[i] = new BuildingPlacement(EnsureBuilding(i + 1), FirstBuildingX + i * BuildingSpacing);

            var route = AssetDatabase.LoadAssetAtPath<ExplorationRoute>(RoutePath);
            if (route == null)
            {
                route = ScriptableObject.CreateInstance<ExplorationRoute>();
                AssetDatabase.CreateAsset(route, RoutePath);
            }
            route.SetPlacements(placements);
            route.SetLoopLength(LevelCount * BuildingSpacing);
            EditorUtility.SetDirty(route);

            AssetDatabase.SaveAssets();
            return route;
        }

        private static string BuildingPath(int level) => $"{DataFolder}/Building_{level:00}.asset";

        private static string BuildingId(int level) => $"Building{level:00}";

        private static BuildingDefinition EnsureBuilding(int level)
        {
            var path = BuildingPath(level);
            var building = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(path);
            if (building == null)
            {
                building = ScriptableObject.CreateInstance<BuildingDefinition>();
                AssetDatabase.CreateAsset(building, path);
            }

            building.Configure(BuildingId(level), EnterPrompt, ClearedPrompt, InteractRange,
                AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(LiarDiceSceneMenu.ConfigPath),
                LiarDiceSceneMenu.EnsureDefaultMonster());
            var (art, doorOffset) = LevelArt[level - 1];
            building.SetExterior(ArtImport.LoadSprite(art + ExteriorSuffix), doorOffset);
            building.SetOutline(ArtImport.LoadSprite(art + OutlineSuffix));
            building.SetExteriorScale(ExteriorScale);
            var span = new BuildingInteractSpanMeasurer().Measure(building);
            building.SetInteractSpan(span.x, span.y);
            EditorUtility.SetDirty(building);
            return building;
        }
    }
}
