using System;
using FGJ.Exploration;
using FGJ.LiarDice;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FGJ.Editor
{
    public static class DefaultPrefabs
    {
        public const string GameplayFolder = "Assets/Prefabs/Gameplay";
        public const string ExplorationFolder = "Assets/Prefabs/Exploration";
        public const string DieSlotPath = GameplayFolder + "/DieSlot.prefab";
        public const string LiarDiceRoomPath = GameplayFolder + "/LiarDiceRoom.prefab";
        public const string PlayerPath = ExplorationFolder + "/Player.prefab";
        public const string BackgroundPath = ExplorationFolder + "/CaveBackground.prefab";
        public const string HudPath = ExplorationFolder + "/ExplorationHud.prefab";
        public const string PlaceholderEntrancePath = ExplorationFolder + "/BuildingEntrance_Placeholder.prefab";

        public const float TileWidth = 55.99f;
        public const float ViewHeight = 14.17f;
        private const float WidestSupportedAspect = 32f / 9f;
        private const float WalkMinX = 2f;
        public const float WorldMaxX = 100000f;
        private const float PlayerSpeed = 4f;

        private static readonly (string sprite, string name, int order, float factor)[] Layers =
        {
            ("Ocean_04", "Layer_FarStalactites", -30, 0.8f),
            ("Ocean_03", "Layer_MidRocks", -20, 0.5f),
            ("Ocean_02", "Layer_CaveGround", -10, 0f),
            ("Ocean_01", "Layer_ForegroundSilhouettes", 10, 0f)
        };

        [MenuItem("FGJ/Prefabs/Create Missing Default Prefabs")]
        public static void EnsureAll()
        {
            ArtImport.ConfigureAll();
            EnsureFolder("Assets/Prefabs", "Gameplay");
            EnsureFolder("Assets/Prefabs", "Exploration");

            var diePrefab = Ensure(DieSlotPath, () => LiarDiceUIFactory.CreateDie(null).gameObject)
                .GetComponent<DieSlotView>();
            Ensure(LiarDiceRoomPath, () => CreateLiarDiceRoom(diePrefab));
            Ensure(PlayerPath, CreatePlayer);
            Ensure(BackgroundPath, CreateBackground);
            Ensure(HudPath, CreateHud);
            Ensure(PlaceholderEntrancePath, CreatePlaceholderEntrance);
        }

        public static T Load<T>(string path) where T : Component
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<T>();
        }

        private static GameObject Ensure(string path, Func<GameObject> create)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;

            var instance = create();
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreateLiarDiceRoom(DieSlotView diePrefab)
        {
            var config = AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(LiarDiceSceneMenu.ConfigPath);
            var controller = LiarDiceRoomBuilder.Build(config, diePrefab);
            controller.UseEncounter(config, LiarDiceSceneMenu.EnsureDefaultMonster());
            return controller.gameObject;
        }

        private static GameObject CreatePlayer()
        {
            var playerObject = new GameObject("Player");
            var renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = ArtImport.LoadSprite("Right_01");

            var animator = playerObject.AddComponent<SpriteFrameAnimator>();
            animator.Configure(renderer, new[]
            {
                new SpriteClip(SideScrollPlayer.IdleClip, new[] { ArtImport.LoadSprite("Right_01") }, 1f, true),
                new SpriteClip(SideScrollPlayer.WalkClip, Frames("Right_01", "Right_02", "Right_03"), 6f, true),
                new SpriteClip(SideScrollPlayer.EnterClip, Frames("Forward_01", "Forward_02", "Forward_03"), 6f, true)
            });

            playerObject.AddComponent<SideScrollPlayer>()
                .Configure(renderer, animator, WalkMinX, WorldMaxX, PlayerSpeed);
            return playerObject;
        }

        private static Sprite[] Frames(params string[] names) => Array.ConvertAll(names, ArtImport.LoadSprite);

        private static GameObject CreateBackground()
        {
            var root = new GameObject("CaveBackground");
            var tileCount = ParallaxLayer.TilesNeeded(ViewHeight * WidestSupportedAspect, TileWidth);
            foreach (var layer in Layers)
            {
                var layerObject = new GameObject(layer.name);
                layerObject.transform.SetParent(root.transform, false);
                var sprite = ArtImport.LoadSprite(layer.sprite);
                var tiles = new Transform[tileCount];
                for (var i = 0; i < tileCount; i++)
                {
                    var tile = new GameObject($"Tile_{i}");
                    tile.transform.SetParent(layerObject.transform, false);
                    tile.transform.localPosition = new Vector3(i * TileWidth, 0f, 0f);
                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.sortingOrder = layer.order;
                    tiles[i] = tile.transform;
                }
                layerObject.AddComponent<ParallaxLayer>().Configure(null, layer.factor, TileWidth, tiles);
            }
            return root;
        }

        private static GameObject CreateHud()
        {
            var canvasObject = new GameObject("ExplorationHud", typeof(RectTransform));
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

            canvasObject.AddComponent<ExplorationHud>().Configure(promptBackground.gameObject, prompt, fader);
            return canvasObject;
        }

        private static GameObject CreatePlaceholderEntrance()
        {
            var root = new GameObject("BuildingEntrance_Placeholder");

            var exterior = new GameObject("Exterior");
            exterior.transform.SetParent(root.transform, false);
            var exteriorRenderer = exterior.AddComponent<SpriteRenderer>();
            exteriorRenderer.sortingOrder = -5;
            exterior.SetActive(false);

            var marker = new GameObject("PlaceholderMarker");
            marker.transform.SetParent(root.transform, false);
            marker.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            marker.transform.localScale = new Vector3(14f, 30f, 1f);
            var glow = marker.AddComponent<SpriteRenderer>();
            glow.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            glow.color = new Color32(150, 255, 220, 150);
            glow.sortingOrder = -5;

            root.AddComponent<RoomEntrance>().Configure(exteriorRenderer, marker);
            return root;
        }
    }
}
