using System.Collections.Generic;
using FGJ.Exploration;
using FGJ.Flow;
using FGJ.LiarDice;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.Tests.PlayMode.Exploration
{
    internal sealed class ExplorationTestRig
    {
        public const string RoomId = "TestBuilding";
        public const float BuildingX = 10f;
        public const string EnterPrompt = "按 ↑ 進入測試建築";
        public const string ClearedPrompt = "測試建築已通過";

        private readonly List<Object> _created = new List<Object>();
        private BuildingDefinition _building;

        public GameProgress Progress { get; }
        public RecordingSceneRouter Router { get; }
        public SideScrollPlayer Player { get; private set; }
        public ExplorationController Controller { get; private set; }
        public ExplorationHud Hud { get; private set; }
        public LiarDiceConfig BuildingConfig { get; private set; }

        public BuildingDefinition Building
        {
            get
            {
                if (_building != null)
                    return _building;
                BuildingConfig = Track(ScriptableObject.CreateInstance<LiarDiceConfig>());
                _building = Track(ScriptableObject.CreateInstance<BuildingDefinition>());
                _building.Configure(RoomId, EnterPrompt, ClearedPrompt, 1.5f, BuildingConfig);
                return _building;
            }
        }

        public ExplorationTestRig()
        {
            Progress = Track(ScriptableObject.CreateInstance<GameProgress>());
            Router = Track(ScriptableObject.CreateInstance<RecordingSceneRouter>());
        }

        public void Build(float playerStartX)
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
                new SpriteClip(SideScrollPlayer.EnterClip, new[] { CreateSprite() }, 6f, false)
            });
            Player = playerObject.AddComponent<SideScrollPlayer>();
            Player.Configure(renderer, animator, 0f, 1000f, 50f);
            follow.Configure(playerObject.transform, 0f, 1000f);

            var canvas = Track(new GameObject("Canvas", typeof(Canvas)));
            var promptRoot = new GameObject("Prompt", typeof(RectTransform));
            promptRoot.transform.SetParent(canvas.transform, false);
            var promptText = new GameObject("PromptText", typeof(RectTransform)).AddComponent<Text>();
            promptText.transform.SetParent(promptRoot.transform, false);
            var fader = new GameObject("Fader", typeof(RectTransform)).AddComponent<Image>();
            fader.transform.SetParent(canvas.transform, false);
            Hud = canvas.AddComponent<ExplorationHud>();
            Hud.Configure(promptRoot, promptText, fader);

            Controller = Track(new GameObject("Exploration")).AddComponent<ExplorationController>();
            Controller.Configure(Player, follow, Hud);
            Controller.SetRoute(CreateRoute(), CreateEntranceTemplate(), 0f);
            Controller.SetTransitionDurations(0f, 0f);
            Controller.SetServices(Progress, Router);
        }

        public void Dispose()
        {
            foreach (var created in _created)
            {
                if (created != null)
                    Object.Destroy(created);
            }
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

        private ExplorationRoute CreateRoute()
        {
            var route = Track(ScriptableObject.CreateInstance<ExplorationRoute>());
            route.SetPlacements(new[] { new BuildingPlacement(Building, BuildingX) });
            return route;
        }

        private RoomEntrance CreateEntranceTemplate()
        {
            var template = Track(new GameObject("EntranceTemplate"));
            template.transform.position = new Vector3(-100f, 0f, 0f);
            var entrance = template.AddComponent<RoomEntrance>();
            entrance.Configure(null);
            return entrance;
        }
    }
}
