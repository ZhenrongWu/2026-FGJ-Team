using FGJ.Flow;
using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class RoomEntrance : MonoBehaviour
    {
        private static readonly Color PlaceholderGlow = new Color32(150, 255, 220, 150);
        private static readonly Vector3 PlaceholderScale = new Vector3(14f, 30f, 1f);
        private static readonly Vector3 PlaceholderOffset = new Vector3(0f, 1.6f, 0f);

        [SerializeField] private BuildingDefinition building;
        [SerializeField] private GameObject activeMarker;

        public BuildingDefinition Building => building;
        public string RoomId => building.BuildingId;
        public string RoomScene => building.GameplayScene;
        public float X => transform.position.x;
        public bool IsCleared => GameSession.IsCleared(RoomId);
        public string Prompt => IsCleared ? building.ClearedPrompt : building.EnterPrompt;

        public void Configure(BuildingDefinition definition, GameObject marker)
        {
            building = definition;
            activeMarker = marker;
        }

        public bool IsInRange(float playerX) => Mathf.Abs(playerX - X) <= building.InteractRange;

        public static RoomEntrance Spawn(BuildingDefinition definition, float x, float groundY, Sprite placeholder,
            Transform parent)
        {
            var root = new GameObject($"Building_{definition.BuildingId}");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(x, groundY, 0f);

            GameObject marker = null;
            if (definition.Exterior != null)
            {
                CreateVisual(root.transform, "Exterior", definition.Exterior, Color.white,
                    definition.ExteriorOffset, Vector3.one, definition.ExteriorSortingOrder);
            }
            else if (placeholder != null)
            {
                marker = CreateVisual(root.transform, "PlaceholderMarker", placeholder, PlaceholderGlow,
                    PlaceholderOffset, PlaceholderScale, definition.ExteriorSortingOrder);
            }

            var entrance = root.AddComponent<RoomEntrance>();
            entrance.Configure(definition, marker);
            return entrance;
        }

        private static GameObject CreateVisual(Transform parent, string name, Sprite sprite, Color color,
            Vector3 localPosition, Vector3 localScale, int sortingOrder)
        {
            var visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return visual;
        }

        private void Start()
        {
            if (activeMarker != null)
                activeMarker.SetActive(!IsCleared);
        }
    }
}
