using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FGJ.LiarDice.UI
{
    public sealed class OxygenGauge : MonoBehaviour
    {
        [SerializeField] private RectTransform segmentRoot;
        [SerializeField] private Image segmentTemplate;
        [SerializeField] private Color filledColor = new Color32(111, 211, 154, 255);
        [SerializeField] private Color emptyColor = new Color32(70, 80, 74, 255);

        private readonly List<Image> _segments = new List<Image>();

        public int FilledCount { get; private set; }
        public int SegmentCount => _segments.Count;

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

            FilledCount = Mathf.Clamp(current, 0, max);
            for (var i = 0; i < _segments.Count; i++)
            {
                _segments[i].gameObject.SetActive(i < max);
                _segments[i].color = i < FilledCount ? filledColor : emptyColor;
            }
        }
    }
}
