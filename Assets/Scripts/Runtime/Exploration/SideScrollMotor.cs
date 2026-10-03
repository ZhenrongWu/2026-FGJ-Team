using UnityEngine;

namespace FGJ.Exploration
{
    public sealed class SideScrollMotor
    {
        private const float InputDeadZone = 0.1f;

        public float X { get; private set; }
        public float MinX { get; }
        public float MaxX { get; }
        public float MoveSpeed { get; }
        public int Facing { get; private set; } = 1;
        public bool IsMoving { get; private set; }

        public SideScrollMotor(float startX, float minX, float maxX, float moveSpeed)
        {
            MinX = Mathf.Min(minX, maxX);
            MaxX = Mathf.Max(minX, maxX);
            MoveSpeed = moveSpeed;
            X = Mathf.Clamp(startX, MinX, MaxX);
        }

        public void Step(float input, float deltaTime)
        {
            input = Mathf.Clamp(input, -1f, 1f);
            if (Mathf.Abs(input) < InputDeadZone)
            {
                IsMoving = false;
                return;
            }

            Facing = input > 0f ? 1 : -1;
            var nextX = Mathf.Clamp(X + input * MoveSpeed * deltaTime, MinX, MaxX);
            IsMoving = !Mathf.Approximately(nextX, X);
            X = nextX;
        }

        public void Teleport(float x)
        {
            X = Mathf.Clamp(x, MinX, MaxX);
            IsMoving = false;
        }
    }
}
