using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class RoomEntrance : MonoBehaviour
    {
        [SerializeField] private BuildingDefinition building;
        [SerializeField] private SpriteRenderer exterior;

        public BuildingDefinition Building => building;
        public string RoomId => building.BuildingId;
        public float X => transform.position.x;
        public bool IsCleared { get; private set; }
        public string Prompt => IsCleared ? building.ClearedPrompt : building.EnterPrompt;
        public bool HasExterior => exterior != null && exterior.gameObject.activeSelf;

        public void Configure(SpriteRenderer exteriorRenderer)
        {
            exterior = exteriorRenderer;
        }

        public void Bind(BuildingDefinition definition)
        {
            building = definition;
            if (exterior == null)
                return;

            var hasExterior = definition.Exterior != null;
            exterior.gameObject.SetActive(hasExterior);
            if (!hasExterior)
                return;

            exterior.sprite = definition.Exterior;
            exterior.transform.localPosition = definition.ScaledExteriorOffset;
            exterior.transform.localScale = new Vector3(definition.ExteriorScale.x, definition.ExteriorScale.y, 1f);
            exterior.sortingOrder = definition.ExteriorSortingOrder;
        }

        public bool IsInRange(float playerX) => Mathf.Abs(playerX - X) <= building.InteractRange;

        public void SetCleared(bool cleared)
        {
            IsCleared = cleared;
        }
    }
}
