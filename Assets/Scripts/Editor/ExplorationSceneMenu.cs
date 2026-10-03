using System.Linq;
using FGJ.Exploration;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Editor
{
    public static class ExplorationSceneMenu
    {
        public const string ScenePath = "Assets/Scenes/Exploration.unity";
        public const string DataFolder = "Assets/Data/Exploration";
        public const string FirstBuildingPath = DataFolder + "/Building_SwampWatcher.asset";
        public const string RoutePath = DataFolder + "/ExplorationRoute.asset";

        private const string BackgroundFolder = "Assets/Art/Sprites/Backgrounds/Cave/";
        private const string CharacterFolder = "Assets/Art/Sprites/Characters/Player/";
        private const float BackgroundPixelsPerUnit = 100f;
        private const float CharacterPixelsPerUnit = 600f;
        private const int BackgroundMaxTextureSize = 8192;
        private const int CharacterMaxTextureSize = 2048;
        private static readonly Vector2 CharacterPivot = new Vector2(0.5f, 0.075f);

        private const float TileWidth = 55.99f;
        private const float ViewHeight = 14.17f;
        private const float WidestSupportedAspect = 32f / 9f;
        private const float GroundY = 1.5f;
        private const float PlayerStartX = 6f;
        private const float WalkMinX = 2f;
        private const float WorldMaxX = 100000f;
        private const float PlayerSpeed = 4f;
        private const float FirstBuildingX = 44f;

        private static readonly string[] BackgroundSprites = { "Ocean_01", "Ocean_02", "Ocean_03", "Ocean_04" };
        private static readonly string[] CharacterSprites =
            { "Right_01", "Right_02", "Right_03", "Forward_01", "Forward_02", "Forward_03" };

        private static readonly (string sprite, string name, int order, float factor)[] Layers =
        {
            ("Ocean_04", "Layer_FarStalactites", -30, 0.8f),
            ("Ocean_03", "Layer_MidRocks", -20, 0.5f),
            ("Ocean_02", "Layer_CaveGround", -10, 0f),
            ("Ocean_01", "Layer_ForegroundSilhouettes", 10, 0f)
        };

        [MenuItem("FGJ/Scenes/Build Exploration Scene")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            BuildScene();
        }

        public static void BuildScene()
        {
            ConfigureArtImport();
            var route = EnsureRoute();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            Object.DestroyImmediate(Object.FindFirstObjectByType<Light>()?.gameObject);

            var camera = SetUpCamera();
            var tilesPerLayer = ParallaxLayer.TilesNeeded(ViewHeight * WidestSupportedAspect, TileWidth);
            foreach (var layer in Layers)
                CreateLayer(layer.sprite, layer.name, layer.order, layer.factor, camera, tilesPerLayer);

            var player = CreatePlayer();
            var follow = camera.gameObject.AddComponent<CameraFollow2D>();
            follow.Configure(player.transform, 0f, WorldMaxX);

            var (promptRoot, prompt, fader) = CreateOverlay();
            var controller = new GameObject("ExplorationController").AddComponent<ExplorationController>();
            controller.Configure(player, follow, promptRoot, prompt, fader);
            controller.SetServices(FlowAssets.Progress, FlowAssets.Router);
            controller.SetRoute(route, GroundY, AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"));

            EditorSceneManager.SaveScene(scene, ScenePath);
            SceneBuildOrder.Apply();
        }

        private static ExplorationRoute EnsureRoute()
        {
            if (!AssetDatabase.IsValidFolder(DataFolder))
                AssetDatabase.CreateFolder("Assets/Data", "Exploration");

            var building = AssetDatabase.LoadAssetAtPath<BuildingDefinition>(FirstBuildingPath);
            if (building == null)
            {
                building = ScriptableObject.CreateInstance<BuildingDefinition>();
                building.Configure("SwampWatcher", "按 E 進入洞穴深處", "看守者已經沉默了", 1.5f,
                    AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(LiarDiceSceneMenu.ConfigPath),
                    LiarDiceSceneMenu.EnsureDefaultMonster());
                AssetDatabase.CreateAsset(building, FirstBuildingPath);
            }
            else if (building.Monster == null)
            {
                var serialized = new SerializedObject(building);
                serialized.FindProperty("monster").objectReferenceValue = LiarDiceSceneMenu.EnsureDefaultMonster();
                serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void ConfigureArtImport()
        {
            foreach (var name in BackgroundSprites)
                ConfigureSprite(name, BackgroundPixelsPerUnit, SpriteAlignment.BottomLeft, Vector2.zero,
                    BackgroundMaxTextureSize);
            foreach (var name in CharacterSprites)
                ConfigureSprite(name, CharacterPixelsPerUnit, SpriteAlignment.Custom, CharacterPivot,
                    CharacterMaxTextureSize);
        }

        private static void ConfigureSprite(string name, float pixelsPerUnit, SpriteAlignment alignment, Vector2 pivot,
            int maxTextureSize)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath(name));
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = maxTextureSize;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePixelsPerUnit = pixelsPerUnit;
            settings.spriteAlignment = (int)alignment;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static string SpritePath(string name)
        {
            var folder = BackgroundSprites.Contains(name) ? BackgroundFolder : CharacterFolder;
            return folder + name + ".png";
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath(name));
        }

        private static Camera SetUpCamera()
        {
            var camera = Camera.main;
            camera.orthographic = true;
            camera.orthographicSize = ViewHeight * 0.5f;
            camera.transform.SetPositionAndRotation(new Vector3(PlayerStartX, ViewHeight * 0.5f, -10f),
                Quaternion.identity);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(222, 236, 226, 255);
            return camera;
        }

        private static void CreateLayer(string spriteName, string objectName, int sortingOrder, float factor,
            Camera camera, int tileCount)
        {
            var layer = new GameObject(objectName);
            var sprite = LoadSprite(spriteName);
            var tiles = new Transform[tileCount];
            for (var i = 0; i < tileCount; i++)
            {
                var tile = new GameObject($"Tile_{i}");
                tile.transform.SetParent(layer.transform, false);
                tile.transform.localPosition = new Vector3(i * TileWidth, 0f, 0f);
                var renderer = tile.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sortingOrder = sortingOrder;
                tiles[i] = tile.transform;
            }
            layer.AddComponent<ParallaxLayer>().Configure(camera, factor, TileWidth, tiles);
        }

        private static SideScrollPlayer CreatePlayer()
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.position = new Vector3(PlayerStartX, GroundY, 0f);
            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite("Right_01");

            var animator = playerObject.AddComponent<SpriteFrameAnimator>();
            animator.Configure(renderer, new[]
            {
                new SpriteClip(SideScrollPlayer.IdleClip, new[] { LoadSprite("Right_01") }, 1f, true),
                new SpriteClip(SideScrollPlayer.WalkClip,
                    new[] { LoadSprite("Right_01"), LoadSprite("Right_02"), LoadSprite("Right_03") }, 6f, true),
                new SpriteClip(SideScrollPlayer.EnterClip,
                    new[] { LoadSprite("Forward_01"), LoadSprite("Forward_02"), LoadSprite("Forward_03") }, 6f, true)
            });

            var player = playerObject.AddComponent<SideScrollPlayer>();
            player.Configure(renderer, animator, WalkMinX, WorldMaxX, PlayerSpeed);
            return player;
        }

        private static (GameObject promptRoot, Text prompt, Image fader) CreateOverlay()
        {
            var canvasObject = new GameObject("Overlay", typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var promptBackground = LiarDiceUIFactory.CreateImage("Prompt", canvasObject.transform,
                new Color(0.08f, 0.1f, 0.14f, 0.75f));
            promptBackground.raycastTarget = false;
            LiarDiceUIFactory.Place(promptBackground.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 90),
                new Vector2(620, 80));
            var prompt = LiarDiceUIFactory.CreateText("PromptText", promptBackground.transform, string.Empty, 34,
                LiarDiceUIFactory.Bone);
            LiarDiceUIFactory.Stretch(prompt.rectTransform);
            promptBackground.gameObject.SetActive(false);

            var fader = LiarDiceUIFactory.CreateImage("ScreenFader", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
            LiarDiceUIFactory.Stretch(fader.rectTransform);
            fader.raycastTarget = false;
            return (promptBackground.gameObject, prompt, fader);
        }
    }
}
