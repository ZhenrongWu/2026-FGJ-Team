using System;
using UnityEngine;

namespace FGJ.LiarDice.Table
{
    public sealed class DieFaceLayout
    {
        private static readonly Vector3[] FaceNormals =
        {
            Vector3.up,
            Vector3.back,
            Vector3.right,
            Vector3.left,
            Vector3.forward,
            Vector3.down
        };

        public Vector3 NormalFor(int value)
        {
            if (value < LiarDiceRules.MinFace || value > LiarDiceRules.MaxFace)
                throw new ArgumentOutOfRangeException(nameof(value));
            return FaceNormals[value - 1];
        }

        public Quaternion RotationShowing(int value, float yawDegrees)
        {
            return Quaternion.AngleAxis(yawDegrees, Vector3.up) * Quaternion.FromToRotation(NormalFor(value), Vector3.up);
        }

        public int ValueFacingUp(Quaternion rotation)
        {
            var bestValue = LiarDiceRules.MinFace;
            var bestDot = float.MinValue;
            for (var value = LiarDiceRules.MinFace; value <= LiarDiceRules.MaxFace; value++)
            {
                var dot = Vector3.Dot(rotation * NormalFor(value), Vector3.up);
                if (dot <= bestDot)
                    continue;
                bestDot = dot;
                bestValue = value;
            }
            return bestValue;
        }
    }
}
