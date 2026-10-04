using System;
using FGJ.Exploration;
using FGJ.LiarDice;
using FGJ.LiarDice.Table;
using FGJ.LiarDice.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FGJ.Editor
{
    public static class DefaultPrefabs
    {
        public const string GameplayFolder = "Assets/Prefabs/Gameplay";
        public const string ExplorationFolder = "Assets/Prefabs/Exploration";
        public const string DiePath = GameplayFolder + "/Die.prefab";
        public const string DiceCupPath = GameplayFolder + "/DiceCup.prefab";
        public const string DiceTablePath = GameplayFolder + "/DiceTable.prefab";
        public const string GameplayHudPath = GameplayFolder + "/GameplayHud.prefab";
        public const string MonsterPath = GameplayFolder + "/Monster.prefab";
        public const string LiarDiceRoomPath = GameplayFolder + "/LiarDiceRoom.prefab";
        public const string PlayerPath = ExplorationFolder + "/Player.prefab";
        public const string BackgroundPath = ExplorationFolder + "/CaveBackground.prefab";
        public const string HudPath = ExplorationFolder + "/ExplorationHud.prefab";
        public const string EntrancePath = ExplorationFolder + "/BuildingEntrance.prefab";

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

            var dice = new DicePlaceholderBuilder();
            var ui = new UiFactory();
            var diePrefab = Ensure(DiePath, dice.CreateDie).GetComponent<DieView>();
            var cupPrefab = Ensure(DiceCupPath,
                () => dice.CreateCup(new Vector3(0.42f, 0.08f, 0.2f), new Vector3(0f, 0f, -22f)));
            var tablePrefab = Ensure(DiceTablePath, () => dice.CreateTable(cupPrefab, diePrefab));
            var hudPrefab = Ensure(GameplayHudPath, () => new GameplayHudBuilder(ui).Build().gameObject);
            var monsterPrefab = Ensure(MonsterPath, new MonsterPlaceholderBuilder().CreateMonster);
            Ensure(LiarDiceRoomPath, () => CreateLiarDiceRoom(hudPrefab, tablePrefab, monsterPrefab));
            Ensure(PlayerPath, CreatePlayer);
            Ensure(BackgroundPath, CreateBackground);
            Ensure(HudPath, () => CreateHud(ui));
            Ensure(EntrancePath, CreateEntrance);
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

        private static readonly Vector3 MonsterLocalPosition = new Vector3(0f, 0.12f, 1.1f);

        private static GameObject CreateLiarDiceRoom(GameObject hudPrefab, GameObject tablePrefab,
            GameObject monsterPrefab)
        {
            var root = new GameObject("LiarDiceRoom");
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(root.transform, false);

            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, root.transform);
            var table = (GameObject)PrefabUtility.InstantiatePrefab(tablePrefab, root.transform);
            var monster = (GameObject)PrefabUtility.InstantiatePrefab(monsterPrefab, root.transform);
            monster.transform.localPosition = MonsterLocalPosition;

            var controller = root.AddComponent<LiarDiceRoomController>();
            controller.Configure(hud.GetComponent<LiarDiceHud>(), table.GetComponent<DiceTableView>(),
                monster.GetComponent<MonsterView>());
            var config = AssetDatabase.LoadAssetAtPath<LiarDiceConfig>(LiarDiceSceneMenu.ConfigPath);
            controller.UseEncounter(config, LiarDiceSceneMenu.EnsureDefaultMonster());
            return root;
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
                new SpriteClip(SideScrollPlayer.EnterClip, Frames("Forward_01"), 6f, false)
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

        private static GameObject CreateHud(UiFactory ui)
        {
            var canvasObject = new GameObject("ExplorationHud", typeof(RectTransform));
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var promptBackground = ui.CreateImage("Prompt", canvasObject.transform,
                new Color(0.08f, 0.1f, 0.14f, 0.75f));
            promptBackground.raycastTarget = false;
            ui.Place(promptBackground.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 90),
                new Vector2(620, 80));
            var prompt = ui.CreateText("PromptText", promptBackground.transform, string.Empty, 34,
                ui.Bone);
            ui.Stretch(prompt.rectTransform);
            promptBackground.gameObject.SetActive(false);

            var fader = ui.CreateImage("ScreenFader", canvasObject.transform, new Color(0f, 0f, 0f, 0f));
            ui.Stretch(fader.rectTransform);
            fader.raycastTarget = false;

            canvasObject.AddComponent<ExplorationHud>().Configure(promptBackground.gameObject, prompt, fader);
            return canvasObject;
        }

        private static GameObject CreateEntrance()
        {
            var root = new GameObject("BuildingEntrance");

            var exterior = new GameObject("Exterior");
            exterior.transform.SetParent(root.transform, false);
            var exteriorRenderer = exterior.AddComponent<SpriteRenderer>();
            exteriorRenderer.sortingOrder = -5;
            exterior.SetActive(false);

            var outline = new GameObject("Outline");
            outline.transform.SetParent(root.transform, false);
            var outlineRenderer = outline.AddComponent<SpriteRenderer>();
            outlineRenderer.sortingOrder = -6;
            outline.SetActive(false);

            root.AddComponent<RoomEntrance>().Configure(exteriorRenderer, outlineRenderer);
            return root;
        }
    }
}
