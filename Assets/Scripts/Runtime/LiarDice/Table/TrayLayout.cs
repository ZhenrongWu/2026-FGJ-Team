using System.Collections.Generic;
using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class TrayLayout
    {
        private const int SingleRingCapacity = 6;
        private const float OuterRingRatio = 0.62f;
        private const float InnerRingRatio = 0.26f;

        public IReadOnlyList<Vector3> Positions(int count, float radius)
        {
            var positions = new List<Vector3>(count);
            if (count <= 0)
                return positions;
            if (count == 1)
            {
                positions.Add(Vector3.zero);
                return positions;
            }

            if (count <= SingleRingCapacity)
            {
                AddRing(positions, count, radius * OuterRingRatio, 0f);
                return positions;
            }

            var innerCount = Mathf.CeilToInt(count / 3f);
            AddRing(positions, count - innerCount, radius * OuterRingRatio, 0f);
            if (innerCount == 1)
                positions.Add(Vector3.zero);
            else
                AddRing(positions, innerCount, radius * InnerRingRatio, 180f / innerCount);
            return positions;
        }

        private void AddRing(List<Vector3> positions, int count, float ringRadius, float angleOffsetDegrees)
        {
            for (var i = 0; i < count; i++)
            {
                var angle = (angleOffsetDegrees + 360f * i / count) * Mathf.Deg2Rad;
                positions.Add(new Vector3(Mathf.Sin(angle) * ringRadius, 0f, Mathf.Cos(angle) * ringRadius));
            }
        }
    }
}
