using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class RoomEntrance : MonoBehaviour
    {
        [SerializeField] private BuildingDefinition building;
        [SerializeField] private SpriteRenderer exterior;
        [SerializeField] private SpriteRenderer outline;
        [SerializeField] private OutlinePulse outlinePulse = new OutlinePulse();

        private float _highlightElapsed;

        public BuildingDefinition Building => building;
        public string RoomId => building.BuildingId;
        public float X => transform.position.x;
        public bool IsCleared { get; private set; }
        public bool IsHighlighted { get; private set; }
        public string Prompt => IsCleared ? building.ClearedPrompt : building.EnterPrompt;
        public bool HasExterior => exterior != null && exterior.gameObject.activeSelf;
        public bool HasOutline => outline != null && outline.sprite != null;
        public float OutlineAlpha => outline != null && outline.gameObject.activeSelf ? outline.color.a : 0f;

        public void Configure(SpriteRenderer exteriorRenderer, SpriteRenderer outlineRenderer = null)
        {
            exterior = exteriorRenderer;
            outline = outlineRenderer;
        }

        public void SetOutlinePulse(OutlinePulse pulse)
        {
            outlinePulse = pulse;
        }

        public void Bind(BuildingDefinition definition)
        {
            building = definition;
            BindExterior(definition);
            BindOutline(definition);
        }

        public bool IsInRange(float playerX) => Mathf.Abs(playerX - X) <= building.InteractRange;

        public void SetCleared(bool cleared)
        {
            IsCleared = cleared;
        }

        public void SetHighlighted(bool highlighted)
        {
            if (IsHighlighted == highlighted)
                return;

            IsHighlighted = highlighted;
            _highlightElapsed = 0f;
            if (outline != null)
                outline.gameObject.SetActive(highlighted && HasOutline);
            ApplyOutlineAlpha();
        }

        public void AdvanceHighlight(float deltaTime)
        {
            if (!IsHighlighted)
                return;
            _highlightElapsed += deltaTime;
            ApplyOutlineAlpha();
        }

        private void Update()
        {
            AdvanceHighlight(Time.deltaTime);
        }

        private void BindExterior(BuildingDefinition definition)
        {
            if (exterior == null)
                return;

            var hasExterior = definition.Exterior != null;
            exterior.gameObject.SetActive(hasExterior);
            if (!hasExterior)
                return;

            exterior.sprite = definition.Exterior;
            PlaceLikeExterior(exterior.transform, definition);
            exterior.sortingOrder = definition.ExteriorSortingOrder;
        }

        private void BindOutline(BuildingDefinition definition)
        {
            if (outline == null)
                return;

            outline.sprite = definition.Outline;
            PlaceLikeExterior(outline.transform, definition);
            outline.sortingOrder = definition.ExteriorSortingOrder + 1;
            outline.gameObject.SetActive(IsHighlighted && HasOutline);
            ApplyOutlineAlpha();
        }

        private void PlaceLikeExterior(Transform target, BuildingDefinition definition)
        {
            target.localPosition = definition.ScaledExteriorOffset;
            target.localScale = new Vector3(definition.ExteriorScale.x, definition.ExteriorScale.y, 1f);
        }

        private void ApplyOutlineAlpha()
        {
            if (outline == null)
                return;
            var color = outline.color;
            color.a = IsHighlighted ? outlinePulse.AlphaAt(_highlightElapsed) : 0f;
            outline.color = color;
        }
    }
}
