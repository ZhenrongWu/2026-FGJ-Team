using System.Collections;
using System.Collections.Generic;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FGJ.Exploration
{
    public sealed class ExplorationController : MonoBehaviour
    {
        [SerializeField] private SideScrollPlayer player;
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private ExplorationHud hud;
        [SerializeField] private GameProgress progress;
        [SerializeField] private SceneRouter router;
        [SerializeField] private ExplorationRoute route;
        [SerializeField] private RoomEntrance defaultEntrancePrefab;
        [SerializeField] private float buildingGroundY = 1.5f;
        [Min(0f)] [SerializeField] private float enterDuration = 1.2f;
        [Min(0f)] [SerializeField] private float fadeInDuration = 0.6f;

        private readonly List<RoomEntrance> _entrances = new List<RoomEntrance>();
        private readonly List<float> _entranceBaseXs = new List<float>();
        private bool _interactRequested;

        public RoomEntrance NearbyEntrance { get; private set; }
        public IReadOnlyList<RoomEntrance> Entrances => _entrances;
        public bool IsEnteringRoom { get; private set; }

        public void Configure(SideScrollPlayer explorationPlayer, CameraFollow2D follow, ExplorationHud explorationHud)
        {
            player = explorationPlayer;
            cameraFollow = follow;
            hud = explorationHud;
        }

        public void SetServices(GameProgress gameProgress, SceneRouter sceneRouter)
        {
            progress = gameProgress;
            router = sceneRouter;
        }

        public void SetRoute(ExplorationRoute explorationRoute, RoomEntrance entrancePrefab, float groundY)
        {
            route = explorationRoute;
            defaultEntrancePrefab = entrancePrefab;
            buildingGroundY = groundY;
        }

        public void SetTransitionDurations(float enter, float fadeIn)
        {
            enterDuration = Mathf.Max(0f, enter);
            fadeInDuration = Mathf.Max(0f, fadeIn);
        }

        public void RequestInteract()
        {
            _interactRequested = true;
        }

        private void Start()
        {
            SpawnBuildings();
            PlacePlayerAtReturnEntrance();
            MoveEntrancesNear(player.X);
            hud.SetFade(1f);
            StartCoroutine(hud.Fade(1f, 0f, fadeInDuration));
        }

        private void Update()
        {
            if (IsEnteringRoom)
                return;

            MoveEntrancesNear(player.X);
            NearbyEntrance = FindEntranceInRange(player.X);
            HighlightOnly(NearbyEntrance != null && !NearbyEntrance.IsCleared ? NearbyEntrance : null);
            if (NearbyEntrance != null)
                hud.ShowPrompt(NearbyEntrance.Prompt);
            else
                hud.HidePrompt();

            var interact = _interactRequested || InteractKeyPressed();
            _interactRequested = false;
            if (interact && NearbyEntrance != null && !NearbyEntrance.IsCleared)
                StartCoroutine(EnterRoom(NearbyEntrance));
        }

        private void SpawnBuildings()
        {
            _entrances.Clear();
            _entranceBaseXs.Clear();
            if (route == null)
                return;

            foreach (var placement in route.Placements)
            {
                if (placement.building == null)
                    continue;

                var prefab = placement.building.EntrancePrefab != null
                    ? placement.building.EntrancePrefab
                    : defaultEntrancePrefab;
                var entrance = Instantiate(prefab, new Vector3(placement.x, buildingGroundY, 0f), Quaternion.identity,
                    transform);
                entrance.name = $"Building_{placement.building.BuildingId}";
                entrance.Bind(placement.building);
                entrance.SetCleared(progress.IsCleared(entrance.RoomId));
                _entrances.Add(entrance);
                _entranceBaseXs.Add(placement.x);
            }
        }

        private void MoveEntrancesNear(float x)
        {
            for (var i = 0; i < _entrances.Count; i++)
            {
                var entranceTransform = _entrances[i].transform;
                var position = entranceTransform.position;
                position.x = route.NearestCopyX(_entranceBaseXs[i], x);
                entranceTransform.position = position;
            }
        }

        private void PlacePlayerAtReturnEntrance()
        {
            var returnRoomId = progress.ConsumeReturnBuilding();
            if (returnRoomId == null)
                return;

            for (var i = 0; i < _entrances.Count; i++)
            {
                if (_entrances[i].RoomId != returnRoomId)
                    continue;
                player.PlaceAt(route.NearestCopyX(_entranceBaseXs[i], progress.ReturnX));
                cameraFollow.SnapToTarget();
                return;
            }
        }

        private void HighlightOnly(RoomEntrance target)
        {
            foreach (var entrance in _entrances)
                entrance.SetHighlighted(entrance == target);
        }

        private RoomEntrance FindEntranceInRange(float playerX)
        {
            foreach (var entrance in _entrances)
            {
                if (entrance.IsInRange(playerX))
                    return entrance;
            }
            return null;
        }

        private IEnumerator EnterRoom(RoomEntrance entrance)
        {
            IsEnteringRoom = true;
            hud.HidePrompt();
            HighlightOnly(null);
            player.PlayEnter();
            progress.EnterBuilding(entrance.Building, entrance.X);
            yield return hud.Fade(0f, 1f, enterDuration);
            router.GoToGameplay();
        }

        private bool InteractKeyPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame);
        }
    }
}
