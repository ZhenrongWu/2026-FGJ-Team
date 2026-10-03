using System.Collections;
using System.Collections.Generic;
using FGJ.Flow;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FGJ.Exploration
{
    public sealed class ExplorationController : MonoBehaviour
    {
        [SerializeField] private SideScrollPlayer player;
        [SerializeField] private CameraFollow2D cameraFollow;
        [SerializeField] private GameProgress progress;
        [SerializeField] private SceneRouter router;
        [SerializeField] private ExplorationRoute route;
        [SerializeField] private float buildingGroundY = 1.5f;
        [SerializeField] private Sprite placeholderMarker;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText;
        [SerializeField] private Image screenFader;
        [Min(0f)] [SerializeField] private float enterDuration = 1.2f;
        [Min(0f)] [SerializeField] private float fadeInDuration = 0.6f;

        private readonly List<RoomEntrance> _entrances = new List<RoomEntrance>();
        private bool _interactRequested;

        public RoomEntrance NearbyEntrance { get; private set; }
        public IReadOnlyList<RoomEntrance> Entrances => _entrances;
        public bool IsEnteringRoom { get; private set; }
        public string PromptMessage => promptRoot.activeSelf ? promptText.text : string.Empty;

        public void Configure(SideScrollPlayer explorationPlayer, CameraFollow2D follow, GameObject prompt,
            Text promptLabel, Image fader)
        {
            player = explorationPlayer;
            cameraFollow = follow;
            promptRoot = prompt;
            promptText = promptLabel;
            screenFader = fader;
        }

        public void SetRoute(ExplorationRoute explorationRoute, float groundY, Sprite placeholder)
        {
            route = explorationRoute;
            buildingGroundY = groundY;
            placeholderMarker = placeholder;
        }

        public void SetServices(GameProgress gameProgress, SceneRouter sceneRouter)
        {
            progress = gameProgress;
            router = sceneRouter;
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
            SetFade(1f);
            StartCoroutine(Fade(1f, 0f, fadeInDuration));
        }

        private void Update()
        {
            if (IsEnteringRoom)
                return;

            NearbyEntrance = FindEntranceInRange(player.X);
            UpdatePrompt();

            var interact = _interactRequested || InteractKeyPressed();
            _interactRequested = false;
            if (interact && NearbyEntrance != null && !NearbyEntrance.IsCleared)
                StartCoroutine(EnterRoom(NearbyEntrance));
        }

        private void SpawnBuildings()
        {
            _entrances.Clear();
            if (route == null)
                return;

            foreach (var placement in route.Placements)
            {
                if (placement.building == null)
                    continue;
                var entrance = RoomEntrance.Spawn(placement.building, placement.x, buildingGroundY, placeholderMarker,
                    transform);
                entrance.SetCleared(progress.IsCleared(entrance.RoomId));
                _entrances.Add(entrance);
            }
        }

        private void PlacePlayerAtReturnEntrance()
        {
            var returnRoomId = progress.ConsumeReturnBuilding();
            if (returnRoomId == null)
                return;

            foreach (var entrance in _entrances)
            {
                if (entrance.RoomId != returnRoomId)
                    continue;
                player.PlaceAt(entrance.X);
                cameraFollow.SnapToTarget();
                return;
            }
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

        private void UpdatePrompt()
        {
            var visible = NearbyEntrance != null;
            promptRoot.SetActive(visible);
            if (visible)
                promptText.text = NearbyEntrance.Prompt;
        }

        private IEnumerator EnterRoom(RoomEntrance entrance)
        {
            IsEnteringRoom = true;
            promptRoot.SetActive(false);
            player.PlayEnter();
            progress.EnterBuilding(entrance.Building);
            yield return Fade(0f, 1f, enterDuration);
            router.GoToGameplay();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                SetFade(Mathf.Lerp(from, to, elapsed / duration));
                elapsed += Time.deltaTime;
                yield return null;
            }
            SetFade(to);
        }

        private void SetFade(float alpha)
        {
            if (screenFader == null)
                return;
            var color = screenFader.color;
            color.a = alpha;
            screenFader.color = color;
            screenFader.raycastTarget = alpha > 0.01f;
        }

        private static bool InteractKeyPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame ||
                                        keyboard.wKey.wasPressedThisFrame);
        }
    }
}
