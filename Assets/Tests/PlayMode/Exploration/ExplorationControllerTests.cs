using System.Collections;
using System.Collections.Generic;
using FGJ.Exploration;
using FGJ.Flow;
using FGJ.LiarDice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace FGJ.Tests.PlayMode.Exploration
{
    public class ExplorationControllerTests
    {
        private const string RoomId = "TestBuilding";
        private const float BuildingX = 10f;
        private const string EnterPrompt = "按 E 進入測試建築";
        private const string ClearedPrompt = "測試建築已通過";

        private readonly List<Object> _created = new List<Object>();
        private GameProgress _progress;
        private RecordingSceneRouter _router;
        private SideScrollPlayer _player;
        private ExplorationController _controller;
        private ExplorationHud _hud;
        private LiarDiceConfig _buildingConfig;

        [SetUp]
        public void SetUp()
        {
            _progress = Track(ScriptableObject.CreateInstance<GameProgress>());
            _router = Track(ScriptableObject.CreateInstance<RecordingSceneRouter>());
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.Destroy(created);
            _created.Clear();
        }

        private T Track<T>(T created) where T : Object
        {
            _created.Add(created);
            return created;
        }

        private Sprite CreateSprite()
        {
            var texture = Track(new Texture2D(4, 4));
            return Track(Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f)));
        }

        private BuildingDefinition _building;

        private ExplorationRoute CreateRoute()
        {
            var route = Track(ScriptableObject.CreateInstance<ExplorationRoute>());
            route.SetPlacements(new[] { new BuildingPlacement(Building, BuildingX) });
            return route;
        }

        private BuildingDefinition Building
        {
            get
            {
                if (_building != null)
                    return _building;
                _buildingConfig = Track(ScriptableObject.CreateInstance<LiarDiceConfig>());
                _building = Track(ScriptableObject.CreateInstance<BuildingDefinition>());
                _building.Configure(RoomId, EnterPrompt, ClearedPrompt, 1.5f, _buildingConfig);
                return _building;
            }
        }

        private void BuildExploration(float playerStartX)
        {
            var cameraObject = Track(new GameObject("Camera", typeof(Camera)));
            cameraObject.GetComponent<Camera>().orthographic = true;
            var follow = cameraObject.AddComponent<CameraFollow2D>();

            var playerObject = Track(new GameObject("Player"));
            playerObject.transform.position = new Vector3(playerStartX, 0f, 0f);
            var renderer = playerObject.AddComponent<SpriteRenderer>();
            var animator = playerObject.AddComponent<SpriteFrameAnimator>();
            animator.Configure(renderer, new[]
            {
                new SpriteClip(SideScrollPlayer.IdleClip, new[] { CreateSprite() }, 1f, true),
                new SpriteClip(SideScrollPlayer.WalkClip, new[] { CreateSprite(), CreateSprite() }, 6f, true),
                new SpriteClip(SideScrollPlayer.EnterClip, new[] { CreateSprite() }, 6f, true)
            });
            _player = playerObject.AddComponent<SideScrollPlayer>();
            _player.Configure(renderer, animator, 0f, 1000f, 50f);
            follow.Configure(playerObject.transform, 0f, 1000f);

            var canvas = Track(new GameObject("Canvas", typeof(Canvas)));
            var promptRoot = new GameObject("Prompt", typeof(RectTransform));
            promptRoot.transform.SetParent(canvas.transform, false);
            var promptText = new GameObject("PromptText", typeof(RectTransform)).AddComponent<Text>();
            promptText.transform.SetParent(promptRoot.transform, false);
            var fader = new GameObject("Fader", typeof(RectTransform)).AddComponent<Image>();
            fader.transform.SetParent(canvas.transform, false);
            _hud = canvas.AddComponent<ExplorationHud>();
            _hud.Configure(promptRoot, promptText, fader);

            _controller = Track(new GameObject("Exploration")).AddComponent<ExplorationController>();
            _controller.Configure(_player, follow, _hud);
            _controller.SetRoute(CreateRoute(), CreateEntranceTemplate(), 0f);
            _controller.SetTransitionDurations(0f, 0f);
            _controller.SetServices(_progress, _router);
        }

        private RoomEntrance CreateEntranceTemplate()
        {
            var template = Track(new GameObject("EntranceTemplate"));
            template.transform.position = new Vector3(-100f, 0f, 0f);
            var marker = new GameObject("PlaceholderMarker");
            marker.transform.SetParent(template.transform, false);
            var entrance = template.AddComponent<RoomEntrance>();
            entrance.Configure(null, marker);
            return entrance;
        }

        private static IEnumerator WaitFrames(int count)
        {
            for (var i = 0; i < count; i++)
                yield return null;
        }

        [UnityTest]
        public IEnumerator Start_SpawnsBuildingsFromRoute()
        {
            BuildExploration(2f);
            yield return null;

            Assert.AreEqual(1, _controller.Entrances.Count);
            Assert.AreEqual(RoomId, _controller.Entrances[0].RoomId);
            Assert.AreEqual(BuildingX, _controller.Entrances[0].X, 1e-4f);
            Assert.IsNotNull(_controller.Entrances[0].transform.Find("PlaceholderMarker"));
        }

        [UnityTest]
        public IEnumerator HoldingRight_MovesPlayerAndPlaysWalk()
        {
            BuildExploration(2f);
            yield return null;
            var startX = _player.X;
            var animator = _player.GetComponent<SpriteFrameAnimator>();

            _player.SetMoveInputOverride(1f);
            yield return WaitFrames(3);

            Assert.Greater(_player.X, startX);
            Assert.AreEqual(SideScrollPlayer.WalkClip, animator.CurrentClipName);
            Assert.IsFalse(_player.GetComponent<SpriteRenderer>().flipX);

            _player.SetMoveInputOverride(-1f);
            yield return WaitFrames(2);
            Assert.IsTrue(_player.GetComponent<SpriteRenderer>().flipX);

            _player.SetMoveInputOverride(0f);
            yield return null;
            Assert.AreEqual(SideScrollPlayer.IdleClip, animator.CurrentClipName);
        }

        [UnityTest]
        public IEnumerator FarFromBuilding_HidesPrompt()
        {
            BuildExploration(2f);
            yield return WaitFrames(2);

            Assert.IsNull(_controller.NearbyEntrance);
            Assert.AreEqual(string.Empty, _hud.PromptMessage);
        }

        [UnityTest]
        public IEnumerator NearBuilding_InteractEntersGameplayWithBuildingConfig()
        {
            BuildExploration(BuildingX + 1f);
            yield return WaitFrames(2);
            Assert.AreEqual(EnterPrompt, _hud.PromptMessage);

            _controller.RequestInteract();
            yield return WaitFrames(3);

            Assert.IsTrue(_controller.IsEnteringRoom);
            Assert.IsTrue(_player.InputLocked);
            Assert.AreEqual(SideScrollPlayer.EnterClip, _player.GetComponent<SpriteFrameAnimator>().CurrentClipName);
            Assert.AreSame(Building, _progress.CurrentBuilding);
            Assert.AreSame(_buildingConfig, _progress.CurrentBuilding.GameplayConfig);
            CollectionAssert.AreEqual(new[] { SceneNames.Gameplay }, _router.LoadedScenes);
        }

        [UnityTest]
        public IEnumerator ReturningFromClearedBuilding_SpawnsThereAndBlocksReentry()
        {
            _progress.EnterBuilding(Building);
            _progress.CompleteBuilding(true);
            BuildExploration(2f);
            yield return WaitFrames(2);

            Assert.AreEqual(BuildingX, _player.X, 1e-4f);
            Assert.AreEqual(ClearedPrompt, _hud.PromptMessage);
            Assert.IsFalse(_controller.Entrances[0].transform.Find("PlaceholderMarker").gameObject.activeSelf);

            _controller.RequestInteract();
            yield return WaitFrames(2);

            Assert.IsFalse(_controller.IsEnteringRoom);
            Assert.IsEmpty(_router.LoadedScenes);
        }
    }
}
