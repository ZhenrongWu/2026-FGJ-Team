using System.Collections.Generic;
using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class BuildingRowLayout
    {
        private readonly float _firstX;
        private readonly float _gap;

        public BuildingRowLayout(float firstX, float gap)
        {
            _firstX = firstX;
            _gap = gap;
        }

        public float[] Positions(IReadOnlyList<Vector2> spans)
        {
            var positions = new float[spans.Count];
            for (var i = 0; i < spans.Count; i++)
                positions[i] = i == 0 ? _firstX : positions[i - 1] + spans[i - 1].y + _gap + spans[i].x;
            return positions;
        }

        public float LoopLength(IReadOnlyList<Vector2> spans)
        {
            if (spans.Count == 0)
                return 0f;
            var positions = Positions(spans);
            var last = spans.Count - 1;
            return positions[last] + spans[last].y + _gap + spans[0].x - _firstX;
        }
    }
}
