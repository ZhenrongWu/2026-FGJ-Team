using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public readonly struct SegmentFit
    {
        public readonly float Height;
        public readonly float Spacing;

        public SegmentFit(float height, float spacing)
        {
            Height = height;
            Spacing = spacing;
        }
    }

    public sealed class SegmentFitter
    {
        private const float MaxSpacingShare = 0.25f;

        public SegmentFit Fit(float available, float preferredHeight, float preferredSpacing, int count)
        {
            if (count <= 1)
                return new SegmentFit(Mathf.Min(preferredHeight, available), preferredSpacing);

            var gaps = count - 1;
            var spacing = Mathf.Min(preferredSpacing, available * MaxSpacingShare / gaps);
            var height = Mathf.Min(preferredHeight, (available - spacing * gaps) / count);
            return new SegmentFit(height, spacing);
        }
    }

    public sealed class OxygenGauge : MonoBehaviour
    {
        [SerializeField] private RectTransform segmentRoot;
        [SerializeField] private Image segmentTemplate;
        [SerializeField] private Color filledColor = new Color32(111, 211, 154, 255);
        [SerializeField] private Color emptyColor = new Color32(70, 80, 74, 255);
        [Min(0f)] [SerializeField] private float preferredSpacing = 8f;

        private readonly List<Image> _segments = new List<Image>();
        private readonly SegmentFitter _fitter = new SegmentFitter();

        public int FilledCount { get; private set; }
        public int SegmentCount => _segments.Count;
        public float SegmentHeight { get; private set; }

        public void Configure(RectTransform root, Image template)
        {
            segmentRoot = root;
            segmentTemplate = template;
        }

        public void Show(int current, int max)
        {
            while (_segments.Count < max)
            {
                var segment = Instantiate(segmentTemplate, segmentRoot);
                segment.gameObject.SetActive(true);
                _segments.Add(segment);
            }

            FitSegments(max);
            FilledCount = Mathf.Clamp(current, 0, max);
            for (var i = 0; i < _segments.Count; i++)
            {
                _segments[i].gameObject.SetActive(i < max);
                _segments[i].color = i < FilledCount ? filledColor : emptyColor;
            }
        }

        private void FitSegments(int count)
        {
            var preferred = segmentTemplate.rectTransform.sizeDelta;
            var available = segmentRoot.rect.height > 0f ? segmentRoot.rect.height : preferred.y * count;
            var fit = _fitter.Fit(available, preferred.y, preferredSpacing, count);
            SegmentHeight = fit.Height;

            var layout = segmentRoot.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
                layout.spacing = fit.Spacing;
            foreach (var segment in _segments)
                segment.rectTransform.sizeDelta = new Vector2(preferred.x, fit.Height);
        }
    }
}
