using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class RoomEntrance : MonoBehaviour
    {
        [SerializeField] private BuildingDefinition building;
        [SerializeField] private SpriteRenderer exterior;
        [SerializeField] private GameObject activeMarker;

        public BuildingDefinition Building => building;
        public string RoomId => building.BuildingId;
        public float X => transform.position.x;
        public bool IsCleared { get; private set; }
        public string Prompt => IsCleared ? building.ClearedPrompt : building.EnterPrompt;

        public void Configure(SpriteRenderer exteriorRenderer, GameObject marker)
        {
            exterior = exteriorRenderer;
            activeMarker = marker;
        }

        public void Bind(BuildingDefinition definition)
        {
            building = definition;
            if (exterior == null || definition.Exterior == null)
                return;

            exterior.sprite = definition.Exterior;
            exterior.transform.localPosition = definition.ExteriorOffset;
            exterior.sortingOrder = definition.ExteriorSortingOrder;
            exterior.gameObject.SetActive(true);
            HidePlaceholderMarker();
        }

        public bool HasExterior => exterior != null && exterior.gameObject.activeSelf;

        private void HidePlaceholderMarker()
        {
            if (activeMarker == null)
                return;
            activeMarker.SetActive(false);
            activeMarker = null;
        }

        public bool IsInRange(float playerX) => Mathf.Abs(playerX - X) <= building.InteractRange;

        public void SetCleared(bool cleared)
        {
            IsCleared = cleared;
            if (activeMarker != null)
                activeMarker.SetActive(!cleared);
        }
    }
}
